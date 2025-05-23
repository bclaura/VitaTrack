using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace VitaTrack;

public partial class UserProfileEditPage : ContentPage
{
    private UserProfileViewModel _viewModel;
    private readonly HttpClient _httpClient;
    public User LoggedInUser { get; set; }
    public event PropertyChangedEventHandler PropertyChanged;

    public UserProfileEditPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
        LoggedInUser = SessionManager.LoggedInUser;
        _viewModel = new UserProfileViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfileAsync();
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async Task LoadProfileAsync()
    {
        int? userId = await SessionManager.GetLoggedInUserIdAsync();
        if (userId == null) return;

        var user = await _httpClient.GetFromJsonAsync<User>($"api/users/{userId}");
        var patient = await _httpClient.GetFromJsonAsync<Patient>($"api/patients/byUserId/{userId}");

        _viewModel.FirstName = user.FirstName;
        _viewModel.LastName = user.LastName;
        _viewModel.FullName = $"{user.FirstName} {user.LastName}";
        _viewModel.Email = user.Email;


        if (DateTime.TryParse(user.DateOfBirth, out var parsedDate))
        {
            _viewModel.DateOfBirth = parsedDate.ToString("yyyy-MM-dd");
        }
        else
        {
            _viewModel.DateOfBirth = "";
        }

        _viewModel.Cnp = patient?.Cnp ?? "";
        _viewModel.Phone = patient?.PhoneNumber ?? "";
        _viewModel.AddressStreet = patient?.AdressStreet ?? "";
        _viewModel.AddressCity = patient?.AdressCity ?? "";
        _viewModel.AddressCounty = patient?.AdressCounty ?? "";
        _viewModel.Occupation = patient?.Occupation ?? "";
        _viewModel.Workplace = patient?.Workplace ?? "";
    }

    private async void OnUpdateProfileClicked(object sender, EventArgs e)
    {
        try
        {
            int? userId = await SessionManager.GetLoggedInUserIdAsync();
            if (userId == null)
            {
                await DisplayAlert("Error", "User not logged in", "OK");
                return;
            }

            // Ob?ine ID-ul pacientului asociat (poate fi salvat în ViewModel dup? `LoadProfileAsync()`)
            var patient = await _httpClient.GetFromJsonAsync<Patient>($"api/patients/byUserId/{userId}");
            if (patient == null)
            {
                await DisplayAlert("Error", "Patient not found", "OK");
                return;
            }

            var updateDto = new PatientUpdateDto
            {
                Id = patient.Id,
                Email = _viewModel.Email,
                PhoneNumber = _viewModel.Phone,
                AdressStreet = _viewModel.AddressStreet,
                AdressCity = _viewModel.AddressCity,
                AdressCounty = _viewModel.AddressCounty,
                Occupation = _viewModel.Occupation,
                Workplace = _viewModel.Workplace,
                Cnp = _viewModel.Cnp
            };

            var response = await _httpClient.PutAsJsonAsync($"api/patients/{updateDto.Id}", updateDto);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Profile updated successfully", "OK");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"Update failed: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unexpected error: {ex.Message}", "OK");
        }
    }

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        // Salv?m modific?rile utilizatorului
        SessionManager.UpdateUser(LoggedInUser);
        await DisplayAlert("Success", "Profile updated successfully.", "OK");
        await Navigation.PopAsync();
    }

    private async void OnEditProfileImageClicked(object sender, EventArgs e)
    {
        // Alege imaginea de profil (po?i reutiliza logica pentru schimbarea imaginii)
        await DisplayAlert("Change Profile Image", "This feature is coming soon.", "OK");
    }

    private async void OnProfileRowTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UserProfileEditPage());
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

}