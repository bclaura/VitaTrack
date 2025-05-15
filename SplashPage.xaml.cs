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

        // Înc?rc?m datele utilizatorului dac? sunt salvate
        SessionManager.LoadUserData();

        if (SessionManager.IsLoggedIn)
        {
            // Navig?m la AppShell dac? utilizatorul este autentificat
            Application.Current.MainPage = new AppShell();
        }
        else
        {
            // Navig?m la pagina de login dac? nu este autentificat
            Application.Current.MainPage = new NavigationPage(new WelcomePage());
        }

    }
}
