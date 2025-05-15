namespace VitaTrack;

public partial class UserProfilePage : ContentPage
{
    private UserProfileViewModel _viewModel;
    public UserProfilePage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        //BindingContext = new UserProfileViewModel();

        _viewModel = new UserProfileViewModel();
        BindingContext = _viewModel;

    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserDashboardPage());
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

    private async Task TakePhotoAsync()
    {
        try
        {
            // Verific?m permisiunile pentru camer?
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Camera access is required to take photos.", "OK");
                return;
            }

            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo != null)
            {
                string localFilePath = Path.Combine(FileSystem.AppDataDirectory, $"{Guid.NewGuid()}.jpg");
                using (var stream = await photo.OpenReadAsync())
                using (var newStream = File.OpenWrite(localFilePath))
                    await stream.CopyToAsync(newStream);

                // Set?m noua imagine de profil
                _viewModel.UpdateProfileImage(localFilePath);
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
            // Verific?m permisiunile pentru galerie
            var status = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Storage access is required to select photos.", "OK");
                return;
            }

            var photo = await MediaPicker.PickPhotoAsync();
            if (photo != null)
            {
                string localFilePath = Path.Combine(FileSystem.AppDataDirectory, $"{Guid.NewGuid()}.jpg");
                using (var stream = await photo.OpenReadAsync())
                using (var newStream = File.OpenWrite(localFilePath))
                    await stream.CopyToAsync(newStream);

                // Set?m noua imagine de profil
                _viewModel.UpdateProfileImage(localFilePath);
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
}