namespace VitaTrack;

public partial class SignUpPage : ContentPage
{
    public SignUpPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
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
        string fullName = fullNameEntry.Text?.Trim();
        string email = emailEntry.Text?.Trim();
        string password = passwordEntry.Text?.Trim();
        string phone = mobileEntry.Text?.Trim();
        string dob = dobEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Eroare", "Completează toate câmpurile obligatorii.", "OK");
            return;
        }

        if (MockDatabase.EmailExists(email))
        {
            await DisplayAlert("Eroare", "Acest email este deja folosit.", "OK");
            return;
        }

        var user = new User
        {
            FullName = fullName,
            Email = email,
            Password = password,
            Phone = phone,
            DateOfBirth = dob
        };

        MockDatabase.AddUser(user);

        await DisplayAlert("Succes", "Contul a fost creat cu succes!", "OK");

        // Opțional: Navighează spre LoginPage
        await Navigation.PopAsync();
    }


}
