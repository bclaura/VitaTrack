using System.Collections.ObjectModel;
using System.Globalization;

namespace VitaTrack;

public partial class FavoritePage : ContentPage
{

    public FavoritePage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        BindingContext = new FavoriteDoctorsViewModel();
        LoadFavoriteDoctors();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is FavoriteDoctorsViewModel vm)
        {
            vm.LoadFavoriteDoctors();
        }
    }

    public ObservableCollection<Doctor> FavoriteDoctors { get; set; } = new();

    private void LoadFavoriteDoctors()
    {
        var updatedFavorites = DoctorRepository.Doctors
                .Where(d => SessionManager.LoggedInUser.FavoriteDoctorNames.Contains(d.Name))
                .ToList();

        FavoriteDoctors.Clear();
        foreach (var doc in updatedFavorites)
            FavoriteDoctors.Add(doc);

        OnPropertyChanged(nameof(FavoriteDoctors));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UserDashboardPage());
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserDashboardPage());
    }

    

    private void OnDoctorsClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DoctorsPage());
    }

}

public class ZeroToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (int)value == 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
