using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace VitaTrack;

public static class SessionManager
{
    private static HttpClient _httpClient => Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
    public static User LoggedInUser { get; private set; }

    public static List<int> FavoriteDoctorIds { get; set; } = new List<int>();

    private static readonly string UserFilePath = Path.Combine(FileSystem.AppDataDirectory, "user.json");

    public static void Login(User user)
    {
        LoggedInUser = user;
        SaveUserData();
    }

    public static void Logout()
    {
        LoggedInUser = null;
        File.Delete(UserFilePath);
    }

    public static bool IsLoggedIn => LoggedInUser != null;
    public static string GetProfileImage() => LoggedInUser?.ProfileImagePath ?? "default_user_icon.png";

    public static void UpdateProfileImage(string imagePath)
    {
        if (LoggedInUser != null)
        {
            LoggedInUser.ProfileImagePath = imagePath;
            SaveUserData();
        }
    }

    private static void SaveUserData()
    {
        if (LoggedInUser != null)
        {
            var json = JsonSerializer.Serialize(LoggedInUser);
            File.WriteAllText(UserFilePath, json);
        }
    }

    public static void LoadUserData()
    {
        if (File.Exists(UserFilePath))
        {
            var json = File.ReadAllText(UserFilePath);
            var loadedUser = JsonSerializer.Deserialize<User>(json);

            
            if (loadedUser != null && loadedUser.Email == LoggedInUser?.Email)
            {
                LoggedInUser = loadedUser;
            }
        }
    }

    public static void UpdateUser(User user)
    {
        if (LoggedInUser != null)
        {
            
            LoggedInUser.Id = user.Id;
            LoggedInUser.FirstName = user.FirstName;
            LoggedInUser.LastName = user.LastName;
            LoggedInUser.Email = user.Email;
            LoggedInUser.Phone = user.Phone;
            LoggedInUser.DateOfBirth = user.DateOfBirth;
            SaveUserData();
        }
    }

    
    public static async Task<bool> IsUserLoggedInAsync()
    {
        string userId = await SecureStorage.GetAsync("UserId");
        return !string.IsNullOrEmpty(userId);
    }

    public static async Task<User> GetUserByEmailAsync(string email)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<User>($"/api/users/by-email?email={email}");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading user: {ex.Message}");
            return null;
        }
    }


    
    public static async Task LogoutAsync()
    {
        try
        {
            
            SecureStorage.Remove("UserId");
            SecureStorage.Remove("UserFirstName");
            SecureStorage.Remove("UserLastName");
            SecureStorage.Remove("UserEmail");
            SecureStorage.Remove("UserRole");
            SecureStorage.Remove("ProfilePictureBase64");

            await Application.Current.MainPage.DisplayAlert("Logout", "Successfully logged out.", "OK");

            
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logout failed: {ex.Message}");
        }
    }

    public static async Task<int?> GetLoggedInUserIdAsync()
    {
        try
        {
            string userIdString = await SecureStorage.GetAsync("UserId");
            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get UserId from SecureStorage: {ex.Message}");
        }

        return null;
    }
}
