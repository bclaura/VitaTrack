namespace VitaTrack;

public partial class DoctorDetailPage : ContentPage
{
    private Doctor _selectedDoctor;
    public DoctorDetailPage(Doctor doctor)
    {
        InitializeComponent();
        _selectedDoctor = doctor;
        BindingContext = _selectedDoctor;
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserDashboardPage());
    }




}