using System.Text.Json;

namespace VitaTrack;

public static class MockDatabase
{
    private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "users.json");

    public static List<User> Users { get; private set; } = new();

    static MockDatabase()
    {
        LoadUsers();
    }

    public static void LoadUsers()
    {
        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            Users = JsonSerializer.Deserialize<List<User>>(json) ?? new();

        }
    }

    public static void SaveUsers()
    {
        string json = JsonSerializer.Serialize(Users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    public static bool EmailExists(string email)
    {
        return Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public static void AddUser(User user)
    {
        Users.Add(user);
        SaveUsers();
    }

    public static User GetUserByEmail(string email)
    {
        return Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public static void UpdatePassword(string email, string newPassword)
    {
        var user = GetUserByEmail(email);
        if (user != null)
        {
            user.Password = newPassword;
            SaveUsers();
        }
    }
}
