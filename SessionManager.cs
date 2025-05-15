using System.Text.Json;

namespace VitaTrack;

public static class SessionManager
{
    public static User LoggedInUser { get; private set; }

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

            // Verific?m dac? e acela?i utilizator (ex. dup? Email)
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
            // Actualizeaz? datele utilizatorului
            LoggedInUser.FullName = user.FullName;
            LoggedInUser.Email = user.Email;
            LoggedInUser.Phone = user.Phone;
            LoggedInUser.DateOfBirth = user.DateOfBirth;
            SaveUserData();
        }
    }
}
