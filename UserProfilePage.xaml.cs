using Microsoft.Maui.Graphics;
using System.Globalization;
using System.Net.Http.Json;
using System.Xml;

namespace VitaTrack;

public partial class UserProfilePage : ContentPage
{
    private UserProfileViewModel _viewModel;
    private readonly HttpClient _httpClient;
    public UserProfilePage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        //BindingContext = new UserProfileViewModel();

        _viewModel = new UserProfileViewModel();
        BindingContext = _viewModel;

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();


        string profilePictureBase64 = await SecureStorage.GetAsync("ProfilePictureBase64");
        if (!string.IsNullOrEmpty(profilePictureBase64))
        {
            profileImage.Source = ConvertFromBase64(profilePictureBase64);
        }
        else
        {
            profileImage.Source = "default_user_icon.png";
        }

        string firstName = await SecureStorage.GetAsync("UserFirstName");
        string lastName = await SecureStorage.GetAsync("UserLastName");

        if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
        {
            nameLabel.Text = $"{firstName} {lastName}";
        }
        else
        {
            nameLabel.Text = "Guest";
        }

        await LoadProfileAsync();
    }


    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserDashboardPage());
    }

    private void OnMessagesClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MessageDoctorPage());
    }

    private void OnCalendaryClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CalendarPage());
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Choose an option", "Cancel", null, "Take Photo", "Upload Photo");

        if (action == "Take Photo")
        {
            await TakePhotoAsync();
        }
        else if (action == "Upload Photo")
        {
            await PickPhotoAsync();
        }
    }

    private async Task LoadProfileAsync()
    {
        int? userId = await SessionManager.GetLoggedInUserIdAsync();
        if (userId == null) return;

        var user = await _httpClient.GetFromJsonAsync<User>($"api/users/{userId}");
        var patient = await _httpClient.GetFromJsonAsync<Patient>($"api/patients/byUserId/{userId}");

        _viewModel.FirstName = user.FirstName;
        _viewModel.LastName = user.LastName;
        _viewModel.FullName = $"{user.FirstName} {user.LastName}";
        _viewModel.Email = user.Email;
        _viewModel.DateOfBirth = user.DateOfBirth ?? "";
        _viewModel.Phone = patient?.PhoneNumber ?? "";
        _viewModel.AddressStreet = patient?.AdressStreet ?? "";
        _viewModel.AddressCity = patient?.AdressCity ?? "";
        _viewModel.AddressCounty = patient?.AdressCounty ?? "";
        _viewModel.Occupation = patient?.Occupation ?? "";
        _viewModel.Workplace = patient?.Workplace ?? "";
    }

    private async Task TakePhotoAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Camera access is required to take photos.", "OK");
                return;
            }

            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo != null)
            {
                string base64Image = await ConvertToBase64Async(photo);

                await UploadProfilePictureAsync(base64Image);
                await SecureStorage.SetAsync("ProfilePictureBase64", base64Image);

                _viewModel.UpdateProfileImage(photo.FullPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unable to take photo: {ex.Message}", "OK");
        }
    }

    private async Task PickPhotoAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Storage access is required to select photos.", "OK");
                return;
            }

            var photo = await MediaPicker.PickPhotoAsync();
            if (photo != null)
            {
                string base64Image = await ConvertToBase64Async(photo);

                await UploadProfilePictureAsync(base64Image);
                await SecureStorage.SetAsync("ProfilePictureBase64", base64Image);

                _viewModel.UpdateProfileImage(photo.FullPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unable to pick photo: {ex.Message}", "OK");
        }
    }

    private async void OnProfileRowTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UserProfileEditPage());
    }

    private async void OnMedicalHistoryTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MedicalHistoryPage());
    }

    private async void OnPrivacyPolicyTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PrivacyPolicy());
    }

    private async Task<string> ConvertToBase64Async(FileResult photo)
    {
        if (photo == null) return null;

        using (var stream = await photo.OpenReadAsync())
        using (var memoryStream = new MemoryStream())
        {
            await stream.CopyToAsync(memoryStream);
            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }

    private ImageSource ConvertFromBase64(string base64String)
    {
        byte[] imageBytes = Convert.FromBase64String(base64String);
        return ImageSource.FromStream(() => new MemoryStream(imageBytes));
    }

    private async Task UploadProfilePictureAsync(int userId, FileResult photo)
    {
        if (photo == null) return;

        try
        {
            string base64String = await ConvertToBase64Async(photo);

            var content = new StringContent($"\"{base64String}\"", System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/Users/{userId}/ProfilePicture", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Image uploaded successfully");
            }
            else
            {
                Console.WriteLine("Error uploading image: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.Message);
        }
    }

    private async Task UploadProfilePictureAsync(string base64Image)
    {
        try
        {
            string userId = await SecureStorage.GetAsync("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                await DisplayAlert("Error", "User not logged in.", "OK");
                return;
            }

            var content = new StringContent($"\"{base64Image}\"", System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/Users/{userId}/ProfilePicture", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Profile picture updated successfully.", "OK");
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"Failed to upload profile picture: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Exception: {ex.Message}", "OK");
        }
    }

}