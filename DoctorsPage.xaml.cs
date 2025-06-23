using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;


namespace VitaTrack;

public partial class DoctorsPage : ContentPage
{

    private readonly HttpClient _httpClient;

    private ObservableCollection<Doctor> Doctors = new();
    private bool isFilterApplied = false;
    private bool isSortAscending = true;
    private List<Doctor> allDoctors = new List<Doctor>();


    public DoctorsPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();

        LoadDoctors();

    }

    private async void LoadDoctors()
    {
        try
        {
            int? userId = await SessionManager.GetLoggedInUserIdAsync();
            if (userId == null)
            {
                await DisplayAlert("Error", "User not logged in. Please log in again.", "OK");
                return;
            }

            var favoriteDoctorIds = await GetFavoriteDoctorIds(userId.Value);

            var response = await _httpClient.GetFromJsonAsync<List<Doctor>>("/api/Doctors");
            if (response != null)
            {
                allDoctors = response;

                foreach (var doctor in allDoctors)
                {
                    doctor.IsFavorite = favoriteDoctorIds.Contains(doctor.Id);
                }

                Doctors = new ObservableCollection<Doctor>(allDoctors);
                DoctorsCollectionView.ItemsSource = Doctors;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading doctors: {ex.Message}");
            await DisplayAlert("Error", "Failed to load doctors. Please try again.", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnSortToggleClicked(object sender, EventArgs e)
    {
        if (isFilterApplied)
        {
            ResetFilterIcons();
            isFilterApplied = false;
        }

        isSortAscending = !isSortAscending;
        isFilterApplied = true;

        
        SortToggleButton.Source = isSortAscending ? "filter_az_icon_active.png" : "filter_za_active_icon.png";

        ResetDoctorList();

        
        var sortedDoctors = isSortAscending
            ? allDoctors.OrderBy(d => d.LastName).ToList()
            : allDoctors.OrderByDescending(d => d.LastName).ToList();

        Doctors.Clear();
        foreach (var doc in sortedDoctors)
        Doctors.Add(doc);
    }

    private async void OnFavoriteFilterClicked(object sender, EventArgs e)
    {
        if (isFilterApplied)
        {
            ResetFilterIcons();
            isFilterApplied = false;
        }

        isFilterApplied = true;
        FavoriteFilter.Source = "filter_fav_icon_active.png";

        try
        {
            int? userId = await SessionManager.GetLoggedInUserIdAsync();
            if (userId == null)
            {
                await DisplayAlert("Error", "User not logged in. Please log in again.", "OK");
                return;
            }

            var favoriteDoctorIds = await GetFavoriteDoctorIds(userId.Value);

            var filteredDoctors = allDoctors.Where(d => favoriteDoctorIds.Contains(d.Id)).ToList();

            Doctors.Clear();
            foreach (var doc in filteredDoctors)
            {
                Doctors.Add(doc);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading favorites: {ex.Message}");
            await DisplayAlert("Error", "Failed to load favorite doctors. Please try again.", "OK");
        }
    }


    private void OnMaleFilterClicked(object sender, EventArgs e)
    {
        
        if (isFilterApplied)
        {
            ResetFilterIcons();
            isFilterApplied = false;
        }

        
        isFilterApplied = true;
        MaleFilter.Source = "filter_male_icon_active.png";

        
        var filteredDoctors = allDoctors.Where(d => d.Gender.Equals("M", StringComparison.OrdinalIgnoreCase)).ToList();

        
        Doctors.Clear();
        foreach (var doc in filteredDoctors)
        {
            Doctors.Add(doc);
        }
    }

    private void OnFemaleFilterClicked(object sender, EventArgs e)
    {
        if (isFilterApplied)
        {
            ResetFilterIcons();
            isFilterApplied = false;
        }

        isFilterApplied = true;
        FemaleFilter.Source = "filter_female_icon_active.png";

        var filteredDoctors = allDoctors.Where(d => d.Gender.Equals("F", StringComparison.OrdinalIgnoreCase)).ToList();

        Doctors.Clear();
        foreach (var doc in filteredDoctors)
        {
            Doctors.Add(doc);
        }
    }

    private void ResetFilterIcons()
    {
        SortToggleButton.Source = "filter_az_icon.png";
        FavoriteFilter.Source = "filter_fav_icon.png";
        MaleFilter.Source = "filter_male_icon.png";
        FemaleFilter.Source = "filter_female_icon.png";
    }

    private void ResetDoctorList()
    {
        Doctors.Clear();
        foreach (var doc in allDoctors)
            Doctors.Add(doc);
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserDashboardPage());
    }

    private async void OnInfoClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Doctor selectedDoctor)
        {
            await Navigation.PushAsync(new DoctorDetailPage(selectedDoctor));
        }
    }

    private async void OnInfoIconClicked(object sender, EventArgs e)
    {

    }

    private async void OnHelpIconClicked(object sender, EventArgs e)
    {

    }

    private async void OnFavoriteClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is Doctor selectedDoctor)
        {
            int? userId = await SessionManager.GetLoggedInUserIdAsync();
            try
            {
                if (selectedDoctor.IsFavorite)
                {
                    var response = await _httpClient.DeleteAsync($"/api/users/{userId}/favorites/{selectedDoctor.Id}");
                    if (response.IsSuccessStatusCode)
                    {
                        selectedDoctor.IsFavorite = false;

                        var doctorInAll = allDoctors.FirstOrDefault(d => d.Id == selectedDoctor.Id);
                        if (doctorInAll != null)
                            doctorInAll.IsFavorite = false;

                        if (isFilterApplied)
                        {
                            ApplyFavoriteFilter();
                        }
                    }
                }
                else
                {
                    var response = await _httpClient.PostAsync($"/api/users/{userId}/favorites/{selectedDoctor.Id}", null);
                    if (response.IsSuccessStatusCode)
                    {
                        selectedDoctor.IsFavorite = true;

                        var doctorInAll = allDoctors.FirstOrDefault(d => d.Id == selectedDoctor.Id);
                        if (doctorInAll != null)
                            doctorInAll.IsFavorite = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating favorite: {ex.Message}");
                await DisplayAlert("Error", "Failed to update favorite status. Please try again.", "OK");
            }
        }
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

    private void ApplyFavoriteFilter()
    {
        var filteredDoctors = allDoctors.Where(d => d.IsFavorite).ToList();

        Doctors.Clear();
        foreach (var doc in filteredDoctors)
        {
            Doctors.Add(doc);
        }
    }

    private void OnProfileClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserProfilePage());
    }

    private void OnMessagesClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MessageDoctorPage());
    }

    private void OnCalendaryClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CalendarPage());
    }



}