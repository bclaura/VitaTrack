using System.Net.Http.Json;

namespace VitaTrack;

public partial class LoginPage : ContentPage
{
    private readonly HttpClient _httpClient;
    public LoginPage()
	{
		InitializeComponent();

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        // IP-ul emulatorului Android
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://10.0.2.2:7203/")
        };

    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = emailEntry.Text?.Trim();
        string password = passwordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Please enter both email and password.", "OK");
            return;
        }

        try
        {
            // Construim DTO-ul pentru login
            var loginDto = new
            {
                Email = email,
                Password = password
            };

            // Trimitem cererea de login la API
            var response = await _httpClient.PostAsJsonAsync("api/Users/Login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                // Citim datele utilizatorului din răspuns
                var user = await response.Content.ReadFromJsonAsync<UserDto>();

                // Salvăm ID-ul utilizatorului pentru sesiune
                await SecureStorage.SetAsync("UserId", user.Id.ToString());
                await SecureStorage.SetAsync("UserFirstName", user.FirstName);
                await SecureStorage.SetAsync("UserLastName", user.LastName);
                await SecureStorage.SetAsync("UserEmail", user.Email);
                await SecureStorage.SetAsync("UserRole", user.Role);
                await SecureStorage.SetAsync("ProfilePictureBase64", user.ProfilePictureBase64 ?? "");

                // Navigăm la dashboard
                await Navigation.PushAsync(new UserDashboardPage());
            }
            else
            {
                await DisplayAlert("Error", "Invalid email or password.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unable to connect to server: {ex.Message}", "OK");
        }
    }






    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnEyeClicked(object sender, EventArgs e)
    {
        passwordEntry.IsPassword = !passwordEntry.IsPassword;
        eyeButton.Source = passwordEntry.IsPassword ? "eye_icon_black.png" : "eye_icon_black_cut.png";
    }

    private async void OnSignUpTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ForgotPasswordPage());
    }



}

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string ProfilePictureBase64 { get; set; }
}