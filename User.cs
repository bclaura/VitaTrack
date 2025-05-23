namespace VitaTrack;

public class User
{
    public int Id { get; set; }
    public string ProfileImagePath { get; set; } = "default_user_icon.png";
    public string FirstName {  get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string DateOfBirth { get; set; }

    public List<string> FavoriteDoctorNames { get; set; } = new();

}
