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

    private async void OnDoctorsTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DoctorsPage());
    }
}