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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Actualizeaz? imaginea de profil când pagina devine activ?
        OnPropertyChanged(nameof(ProfileImage));
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

}