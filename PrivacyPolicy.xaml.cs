namespace VitaTrack;

public partial class PrivacyPolicy : ContentPage
{
	public PrivacyPolicy()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserDashboardPage());
    }

    private void OnMessagesClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MessageDoctorPage());
    }

    private void OnProfileClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserProfilePage());
    }


    private void OnCalendaryClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CalendarPage());
    }
}