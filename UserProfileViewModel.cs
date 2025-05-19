using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VitaTrack
{
    public class UserProfileViewModel : INotifyPropertyChanged
    {
        private string _profileImage;

        public UserProfileViewModel()
        {
            _profileImage = SessionManager.GetProfileImage();
        }
        public string fullName = SessionManager.LoggedInUser?.FirstName + SessionManager.LoggedInUser?.LastName;
        public string UserName => fullName ?? "Guest";
        public string ProfileImage
        {
            get => _profileImage;
            set
            {
                if (_profileImage != value)
                {
                    _profileImage = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Metod? pentru a actualiza imaginea de profil
        public void UpdateProfileImage(string imagePath)
        {
            SessionManager.UpdateProfileImage(imagePath);
            ProfileImage = imagePath;
        }
    }
}