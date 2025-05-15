namespace VitaTrack;

public partial class UserProfileEditPage : ContentPage
{
    public User LoggedInUser { get; set; }
    public UserProfileEditPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        LoggedInUser = SessionManager.LoggedInUser;
        BindingContext = this;
    }

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        // Salv?m modific?rile utilizatorului
        SessionManager.UpdateUser(LoggedInUser);
        await DisplayAlert("Success", "Profile updated successfully.", "OK");
        await Navigation.PopAsync();
    }

    private async void OnEditProfileImageClicked(object sender, EventArgs e)
    {
        // Alege imaginea de profil (po?i reutiliza logica pentru schimbarea imaginii)
        await DisplayAlert("Change Profile Image", "This feature is coming soon.", "OK");
    }

    private async void OnProfileRowTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UserProfileEditPage());
    }
}