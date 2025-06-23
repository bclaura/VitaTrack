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
        await Task.Delay(5000); // 5 secunde
      //await Navigation.PushAsync(new WelcomePage());

        
        SessionManager.LoadUserData();

        if (SessionManager.IsLoggedIn)
        {
            
            Application.Current.MainPage = new AppShell();
        }
        else
        {
            
            Application.Current.MainPage = new NavigationPage(new WelcomePage());
        }

    }
}
