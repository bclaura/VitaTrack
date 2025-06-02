namespace VitaTrack;

public partial class UserDashboardPage : ContentPage
{
    public string ProfileImage => SessionManager.GetProfileImage();
    public UserDashboardPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        string profilePictureBase64 = await SecureStorage.GetAsync("ProfilePictureBase64");
        if (!string.IsNullOrEmpty(profilePictureBase64))
        {
            profileImage.Source = ConvertFromBase64(profilePictureBase64);
        }
        else
        {
            // Imagine default
            profileImage.Source = "default_user_icon.png";
        }

        string firstName = await SecureStorage.GetAsync("UserFirstName");
        string lastName = await SecureStorage.GetAsync("UserLastName");

        if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
        {
            nameLabel.Text = $"{firstName} {lastName}";
        }
        else
        {
            nameLabel.Text = "Guest";
        }
    }

    private ImageSource ConvertFromBase64(string base64String)
    {
        byte[] imageBytes = Convert.FromBase64String(base64String);
        return ImageSource.FromStream(() => new MemoryStream(imageBytes));
    }

    private async void OnDoctorsTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DoctorsPage());
    }

    private async void OnFavoriteTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new FavoritePage());
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UserProfilePage());
    }

    private async void OnMessagesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MessageDoctorPage());
    }

    private async void OnCalendaryClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CalendarPage());
    }

    private async void OnLocationClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LocationPage());
    }

    private async void OnHealthClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HealthPage());
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await SessionManager.LogoutAsync();
    }

}