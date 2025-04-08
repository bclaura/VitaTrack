namespace VitaTrack;

public partial class ResetPasswordPage : ContentPage
{
    private readonly string userEmail;
    private bool showNewPassword = false;
    private bool showConfirmPassword = false;

    public ResetPasswordPage(string email)
    {
        InitializeComponent();
        userEmail = email;
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private void ToggleNewPasswordVisibility(object sender, EventArgs e)
    {
        showNewPassword = !showNewPassword;
        newPasswordEntry.IsPassword = !showNewPassword;
        eyeButtonNew.Source = newPasswordEntry.IsPassword ? "eye_icon_black.png" : "eye_icon_black_cut.png";
    }

    private void ToggleConfirmPasswordVisibility(object sender, EventArgs e)
    {
        showConfirmPassword = !showConfirmPassword;
        confirmPasswordEntry.IsPassword = !showConfirmPassword;
        eyeButtonNewConfirm.Source = confirmPasswordEntry.IsPassword ? "eye_icon_black.png" : "eye_icon_black_cut.png";
    }

    private async void OnCreatePasswordClicked(object sender, EventArgs e)
    {
        string newPassword = newPasswordEntry.Text?.Trim();
        string confirmPassword = confirmPasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlert("Error", "Please fill in both fields.", "OK");
            return;
        }

        if (newPassword != confirmPassword)
        {
            await DisplayAlert("Error", "Passwords do not match.", "OK");
            return;
        }

        MockDatabase.UpdatePassword(userEmail, newPassword);
        await DisplayAlert("Success", "Password updated successfully!", "OK");

        // Navigăm înapoi la pagina de login
        await Navigation.PushAsync(new LoginPage());
    }
}
