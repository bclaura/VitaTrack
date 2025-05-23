using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace VitaTrack
{
    public partial class ChatPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        private System.Timers.Timer refreshTimer;
        private bool isRefreshing = false;

        public ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();
        private Doctor currentDoctor;

        public ChatPage(Doctor doctor)
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
            BindingContext = this;
            currentDoctor = doctor;
            DoctorNameLabel.Text = $"Dr. {doctor.FullName}";
            LoadMessages();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartRefreshTimer();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            refreshTimer = null;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSendMessageClicked(object sender, EventArgs e)
        {
            try
            {
                int? userId = await SessionManager.GetLoggedInUserIdAsync();
                if (userId == null)
                {
                    await DisplayAlert("Error", "User not logged in. Please log in again.", "OK");
                    return;
                }

                var messageToSend = new SendMessageDto
                {
                    SenderId = userId.Value,
                    ReceiverId = currentDoctor.Id,
                    Message = MessageEntry.Text.Trim()
                };

                var response = await _httpClient.PostAsJsonAsync("/api/messages", messageToSend);
                if (response.IsSuccessStatusCode)
                {
                    Messages.Add(new ChatMessage
                    {
                        SenderId = userId.Value,
                        ReceiverId = currentDoctor.Id,
                        Message = messageToSend.Message,
                        SentAt = DateTime.Now,
                        IsRead = false,
                        IsOwnMessage = true
                    });
                }
                MessageEntry.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                await DisplayAlert("Error", "Failed to send message. Please try again.", "OK");
            }
        }

        private void OnMessageEntryCompleted(object sender, EventArgs e)
        {
            OnSendMessageClicked(sender, e);
        }

        private async void LoadMessages()
        {
            try
            {
                int? userId = await SessionManager.GetLoggedInUserIdAsync();

                var response = await _httpClient.GetAsync($"/api/messages/conversation?userId={userId}&doctorId={currentDoctor.Id}");

                var rawJson = await response.Content.ReadAsStringAsync();

                var messages = JsonSerializer.Deserialize<List<ChatMessage>>(rawJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                foreach (var msg in messages)
                {
                    msg.IsOwnMessage = msg.SenderId == userId;
                }
                    

                Messages.Clear();
                foreach (var msg in messages)
                    Messages.Add(msg);

                var lastMessage = Messages.LastOrDefault();
                if (lastMessage != null)
                {
                    MessagesCollectionView.ScrollTo(lastMessage, position: ScrollToPosition.End, animate: true);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] {ex.Message}");
            }


        }

        private void StartRefreshTimer()
        {
            refreshTimer = new System.Timers.Timer(5000); // la 5 secunde
            refreshTimer.Elapsed += async (s, e) => await RefreshMessagesAsync();
            refreshTimer.AutoReset = true;
            refreshTimer.Start();
        }

        private async Task RefreshMessagesAsync()
        {
            if (isRefreshing) return; // evit? apeluri suprapuse
            isRefreshing = true;

            try
            {
                int? userId = await SessionManager.GetLoggedInUserIdAsync();
                if (userId == null) return;

                var response = await _httpClient.GetAsync($"/api/messages/conversation?userId={userId}&doctorId={currentDoctor.Id}");
                var rawJson = await response.Content.ReadAsStringAsync();
                var messages = JsonSerializer.Deserialize<List<ChatMessage>>(rawJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                foreach (var msg in messages)
                    msg.IsOwnMessage = msg.SenderId == userId;

                // Update vizual doar dac? s-a schimbat ceva
                if (messages.Count != Messages.Count || messages.Any(m => !Messages.Any(x => x.Id == m.Id)))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (MessagesCollectionView == null)
                            return;

                        Messages.Clear();
                        foreach (var msg in messages)
                            Messages.Add(msg);

                        var lastMessage = Messages.LastOrDefault();
                        if (lastMessage != null)
                        {
                            MessagesCollectionView.ScrollTo(lastMessage, position: ScrollToPosition.End, animate: true);
                        }
                    });


                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REFRESH ERROR] {ex.Message}");
            }
            finally
            {
                isRefreshing = false;
            }
        }



    }
}
