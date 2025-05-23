using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Windows.Input;

namespace VitaTrack;

public partial class MessageDoctorPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private ObservableCollection<Doctor> Doctors { get; set; } = new ObservableCollection<Doctor>();
    public ICommand DoctorTappedCommand { get; }

    public MessageDoctorPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
        LoadDoctors();
    }

    private async void LoadDoctors()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Doctor>>("/api/doctors");
            if (response != null)
            {
                Doctors.Clear();
                foreach (var doctor in response)
                {
                    Doctors.Add(doctor);
                }

                DoctorsCollectionView.ItemsSource = Doctors;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading doctors: {ex.Message}");
            await DisplayAlert("Error", "Failed to load doctors. Please try again.", "OK");
        }
    }

    private async void OnDoctorSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Doctor selectedDoctor)
        {
            await Navigation.PushAsync(new ChatPage(selectedDoctor));
        }
    }

    private void OnDoctorTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is Doctor doctor)
        {
            Navigation.PushAsync(new ChatPage(doctor));
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserDashboardPage());
    }

    private void OnProfileClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserProfilePage());
    }

    private void OnCalendaryClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CalendarPage());
    }

}