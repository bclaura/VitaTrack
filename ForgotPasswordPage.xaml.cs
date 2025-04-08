namespace VitaTrack;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        string email = emailEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Error", "Please enter your email.", "OK");
            return;
        }

        if (!MockDatabase.EmailExists(email))
        {
            await DisplayAlert("Error", "Email not found.", "OK");
            return;
        }

        // Navigăm la pagina de schimbare parolă și trimitem emailul
        await Navigation.PushAsync(new ResetPasswordPage(email));
    }
}
