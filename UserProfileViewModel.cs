using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VitaTrack
{
    public class UserProfileViewModel : INotifyPropertyChanged
    {
        private string _profileImage;

        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }


        private string _phone;
        public string Phone
        {
            get => _phone;
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _cnp;
        public string Cnp
        {
            get => _cnp;
            set
            {
                if (_cnp != value)
                {
                    _cnp = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _dateOfBirth;
        public string DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                if (_dateOfBirth != value)
                {
                    _dateOfBirth = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _addressStreet;
        public string AddressStreet
        {
            get => _addressStreet;
            set
            {
                if (_addressStreet != value)
                {
                    _addressStreet = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _addressCity;
        public string AddressCity
        {
            get => _addressCity;
            set
            {
                if (_addressCity != value)
                {
                    _addressCity = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _addressCounty;
        public string AddressCounty
        {
            get => _addressCounty;
            set
            {
                if (_addressCounty != value)
                {
                    _addressCounty = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _occupation;
        public string Occupation
        {
            get => _occupation;
            set
            {
                if (_occupation != value)
                {
                    _occupation = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _workplace;
        public string Workplace
        {
            get => _workplace;
            set
            {
                if (_workplace != value)
                {
                    _workplace = value;
                    OnPropertyChanged();
                }
            }
        }



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

        public void UpdateProfileImage(string imagePath)
        {
            SessionManager.UpdateProfileImage(imagePath);
            ProfileImage = imagePath;
        }
    }
}