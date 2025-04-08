namespace VitaTrack;

public static class SessionManager
{
    public static User LoggedInUser { get; private set; }

    public static void Login(User user)
    {
        LoggedInUser = user;
    }

    public static void Logout()
    {
        LoggedInUser = null;
    }

    public static bool IsLoggedIn => LoggedInUser != null;
    public static string GetProfileImage() => LoggedInUser?.ProfileImagePath ?? "default_user_icon.png";

}
