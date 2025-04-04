namespace VitaTrack;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
        NavigateToLogin();
    }

    private async void NavigateToLogin()
    {
        await Task.Delay(7000); // 7 secunde
        await Navigation.PushAsync(new WelcomePage());
    }
}
