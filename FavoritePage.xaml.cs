using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;

namespace VitaTrack;

public partial class FavoritePage : ContentPage
{

    private readonly HttpClient _httpClient;

    private List<Doctor> allDoctors = new List<Doctor>();
    private ObservableCollection<Doctor> Doctors = new();

    public FavoritePage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
        BindingContext = new FavoriteDoctorsViewModel();
        //LoadFavoriteDoctors();
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

    /*private async void LoadFavoriteDoctors()
    {
        try
        {
            int? userId = await SessionManager.GetLoggedInUserIdAsync();
            if (userId == null)
            {
                await DisplayAlert("Error", "User not logged in. Please log in again.", "OK");
                return;
            }

            var docsResponse = await _httpClient.GetFromJsonAsync<List<Doctor>>("/api/Doctors");
            var response = await _httpClient.GetFromJsonAsync<List<Doctor>>($"/api/users/{userId}/favorites");
            if (response != null)
            {
                allDoctors = docsResponse;
                var favoriteDoctorIds = await GetFavoriteDoctorIds(userId.Value);

                var filteredDoctors = allDoctors.Where(d => favoriteDoctorIds.Contains(d.Id)).ToList();

                Doctors.Clear();
                foreach (var doc in filteredDoctors)
                {
                    Doctors.Add(doc);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading doctors: {ex.Message}");
            await DisplayAlert("Error", "Failed to load doctors. Please try again.", "OK");
        }
    }*/

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

    private async Task<List<int>> GetFavoriteDoctorIds(int userId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Doctor>>($"/api/users/{userId}/favorites");
            if (response != null)
            {
                return response.Select(d => d.Id).ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading favorites: {ex.Message}");
        }

        return new List<int>();
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
