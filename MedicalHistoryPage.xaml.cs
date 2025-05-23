using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VitaTrack;

public partial class MedicalHistoryPage : ContentPage
{
    private readonly HttpClient _httpClient;
    public ObservableCollection<MedicalHistory> MedicalHistoryList { get; set; } = new();
    public bool NoHistoryFound { get; set; } = false;
    public PatientDto Patient { get; set; } = new();

    public MedicalHistoryPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
        BindingContext = this;
        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            int? userId = await SessionManager.GetLoggedInUserIdAsync();
            if (userId == null) return;

            var user = await _httpClient.GetFromJsonAsync<User>($"api/users/{userId}");
            if (user == null) return;

            var patient = await _httpClient.GetFromJsonAsync<Patient>($"api/patients/byUserId/{userId}");
            if (patient == null) return;

            Patient = new PatientDto
            {
                FullName = $"{user.FirstName} {user.LastName}",
                Age = patient.Age,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,
                Address = $"{patient.AdressStreet}, \n{patient.AdressCity}, \n{patient.AdressCounty}"
            };

            var response = await _httpClient.GetAsync($"api/MedicalHistory/patient/{patient.Id}");
            if (!response.IsSuccessStatusCode) return;

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var medicalHistories = JsonSerializer.Deserialize<List<MedicalHistory>>(content, options);


            OnPropertyChanged(nameof(Patient));

            MedicalHistoryList.Clear();
            if (medicalHistories != null && medicalHistories.Any())
            {
                foreach (var item in medicalHistories)
                    MedicalHistoryList.Add(item);

                NoHistoryFound = false;
            }
            else
            {
                NoHistoryFound = true;
            }

            OnPropertyChanged(nameof(NoHistoryFound));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
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

    private void OnMessagesClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MessageDoctorPage());
    }

    private void OnCalendaryClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CalendarPage());
    }
}


public class MedicalHistory
{
    public int Id { get; set; }
    public int PatientId { get; set; }

    [JsonPropertyName("history")]
    public string? Diagnosis { get; set; }

    public string? Allergies { get; set; }

    [JsonPropertyName("cardiologyConsultations")]
    public string? CardiologyConsultation { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime Date { get; set; }

    public string DateFormatted => Date.ToString("dd MMM yyyy");
}

public class PatientDto
{
    public string FullName { get; set; }
    public int Age {  get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}