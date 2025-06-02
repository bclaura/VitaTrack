using System.Net.Http.Json;

namespace VitaTrack;

public partial class HealthPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public HealthPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        BindingContext = this;
        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPatientAndHighlightAsync();
    }

    private string GetAgeCategory(int age)
    {
        if (age <= 1) return "0-1 ani";
        if (age <= 10) return "1–10 ani";
        if (age <= 17) return "11–17 ani";
        if (age <= 45) return "18–45 ani";
        return "45+ ani";
    }

    private async Task LoadPatientAndHighlightAsync()
    {
        int? userId = await SessionManager.GetLoggedInUserIdAsync();
        if (userId == null) return;

        try
        {
            var user = await _httpClient.GetFromJsonAsync<User>($"api/users/{userId}");
            var patient = await _httpClient.GetFromJsonAsync<Patient>($"api/patients/byUserId/{userId}");

            int age = patient.Age;
            string category = GetAgeCategory(age);

            HighlightAgeCategory(category);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", $"Nu s-a putut înc?rca datele: {ex.Message}", "OK");
        }
    }

    private void HighlightAgeCategory(string category)
    {
        Color highlight = Color.FromArgb("#D0E8FF");

        switch (category)
        {
            case "0-1 ani":
                AgeLabel_0_1.BackgroundColor = highlight;
                MaleLabel_0_1.BackgroundColor = highlight;
                FemaleLabel_0_1.BackgroundColor = highlight;
                break;

            case "1–10 ani":
                AgeLabel_1_10.BackgroundColor = highlight;
                MaleLabel_1_10.BackgroundColor = highlight;
                FemaleLabel_1_10.BackgroundColor = highlight;
                break;

            case "11–17 ani":
                AgeLabel_11_17.BackgroundColor = highlight;
                MaleLabel_11_17.BackgroundColor = highlight;
                FemaleLabel_11_17.BackgroundColor = highlight;
                break;

            case "18–45 ani":
                AgeLabel_18_45.BackgroundColor = highlight;
                MaleLabel_18_45.BackgroundColor = highlight;
                FemaleLabel_18_45.BackgroundColor = highlight;
                break;

            case "45+ ani":
                AgeLabel_45Plus.BackgroundColor = highlight;
                MaleLabel_45Plus.BackgroundColor = highlight;
                FemaleLabel_45Plus.BackgroundColor = highlight;
                break;
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

    private void OnMessagesClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MessageDoctorPage());
    }

    private void OnCalendaryClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CalendarPage());
    }

    private void OnProfileClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserProfilePage());
    }
}