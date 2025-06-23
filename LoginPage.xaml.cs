using System.Net.Http.Json;

namespace VitaTrack;

public partial class LoginPage : ContentPage
{
    private readonly HttpClient _httpClient;
    public LoginPage()
	{
		InitializeComponent();

        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();

    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        Console.WriteLine("Trimitem POST către: " + _httpClient.BaseAddress + "api/Users/Login");

        string email = emailEntry.Text?.Trim();
        string password = passwordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Please enter both email and password.", "OK");
            return;
        }

        Console.WriteLine("Email: " + email);
        Console.WriteLine("Password: " + password);


        try
        {
            
            var loginDto = new
            {
                Email = email,
                Password = password
            };

            
            var response = await _httpClient.PostAsJsonAsync("api/Users/Login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                
                var user = await response.Content.ReadFromJsonAsync<UserDto>();

                
                await SecureStorage.SetAsync("UserId", user.Id.ToString());
                await SecureStorage.SetAsync("UserFirstName", user.FirstName);
                await SecureStorage.SetAsync("UserLastName", user.LastName);
                await SecureStorage.SetAsync("UserEmail", user.Email);
                await SecureStorage.SetAsync("UserRole", user.Role);
                await SecureStorage.SetAsync("ProfilePictureBase64", user.ProfilePictureBase64 ?? "");

                
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