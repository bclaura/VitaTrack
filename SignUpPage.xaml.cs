using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace VitaTrack;

public partial class SignUpPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public SignUpPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
    }

    private void OnEyeClicked(object sender, EventArgs e)
    {
        passwordEntry.IsPassword = !passwordEntry.IsPassword;
        eyeButton.Source = passwordEntry.IsPassword ? "eye_icon_black.png" : "eye_icon_black_cut.png";
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        if (!DateTime.TryParse(dobEntry.Text?.Trim(), out DateTime dob))
        {
            await DisplayAlert("Error", "Please enter a valid date of birth.", "OK");
            return;
        }
        var registrationDto = new UserRegistrationDto
        {
            FirstName = firstNameEntry.Text?.Trim(),
            LastName = lastNameEntry.Text?.Trim(),
            Email = emailEntry.Text?.Trim(),
            Password = passwordEntry.Text,
            MobileNumber = mobileEntry.Text?.Trim(),
            DateOfBirth = dob //maybe change dob to some picker
        };

        try
        {
            Console.WriteLine($"Sending request to: {_httpClient.BaseAddress}api/Users/Register");

            // 4) Trimiterea POST-ului
            var response = await _httpClient.PostAsJsonAsync("api/Users/Register", registrationDto);

            Console.WriteLine($"Response status: {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                // 5a) Dacă e ok, poate deserializezi răspunsul
                var createdUser = await response.Content.ReadFromJsonAsync<User>();

                await DisplayAlert("Succes", "Cont creat cu email: " + createdUser.Email, "OK");
                // navighează mai departe, de ex. la pagina de login:
                await Navigation.PushAsync(new LoginPage());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // 5b) Email duplicat
                await DisplayAlert("Eroare", "Email-ul există deja.", "OK");
            }
            else
            {
                // 5c) Orice alt cod
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Eroare", error, "OK");
            }
        }
        catch (Exception ex)
        {
            // 6) Probleme de rețea, JSON invalid, etc
            await DisplayAlert("Eroare", "Nu am putut lua legătura cu serverul:\n" + ex.Message, "OK");
        }

    }

    public class UserRegistrationDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string MobileNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
    }


}
