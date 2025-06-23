using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Timers;


namespace VitaTrack;

public partial class LocationPage : ContentPage
{
    private System.Timers.Timer locationTimer;
    private bool isRefreshing = false;
    private readonly HttpClient _httpClient;
    public LocationPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var location = await GetInitialLocationFromServerAsync();

        if (location != null)
        {
            ShowMapOnLocation(location.Latitude, location.Longitude);
        }
        else
        {
            //fallback pe timisoara
            ShowMapOnLocation(45.7489, 21.2087, "Default: Timișoara");
        }
    }

    private async Task<LocationMap> GetInitialLocationFromServerAsync()
    {
        try
        {
            var patientId = await SessionManager.GetLoggedInUserIdAsync();
            var response = await _httpClient.GetAsync($"api/locationmap/{patientId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<LocationMap>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to fetch initial location: {ex.Message}", "OK");
        }
        return null;
    }

    private async void OnEmergencyCallClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirm Emergency", "Are you sure you want to call 112?", "Yes", "Cancel");

        if (confirm)
        {
            try
            {
                PhoneDialer.Open("112");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "This device cannot make calls.", "OK");
            }
        }
    }


    private async Task RefreshLocationAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }
        try
        {
            if(status == PermissionStatus.Granted)
            {
                var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                });

                if (location != null)
                {
                    LocationLabel.Text = $"Last known location: {location.Latitude:F6}, {location.Longitude:F6}";

                    var position = new Location(location.Latitude, location.Longitude);
                    UserMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(0.5)));
                    UserMap.Pins.Clear();
                    UserMap.Pins.Add(new Pin
                    {
                        Label = "You are here",
                        Location = position
                    });

                   
                    await _httpClient.PostAsJsonAsync("api/locationmap", new
                    {
                        PatientId = await SessionManager.GetLoggedInUserIdAsync(),
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        RecordedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    LocationLabel.Text = "Location not available.";
                } 
            }
            else
            {
                LocationLabel.Text = "Location not available.";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to get location: {ex.Message}", "OK");
        }
    }

    private async void OnRefreshLocationClicked(object sender, EventArgs e)
    {
        await RefreshLocationAsync();
    }



    private void ShowMapOnLocation(double latitude, double longitude, string label = "Last Known Location")
    {
        var position = new Location(latitude, longitude);
        UserMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(0.5)));

        UserMap.Pins.Clear();
        UserMap.Pins.Add(new Pin
        {
            Label = label,
            Location = position
        });

        LocationLabel.Text = $"Last known location: {latitude:F6}, {longitude:F6}";
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserDashboardPage());
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