using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace VitaTrack
{
    public class FavoriteDoctorsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Doctor> FavoriteDoctors { get; set; } = new();

        public ICommand ToggleFavoriteCommand { get; }


        public void LoadFavoriteDoctors()
        {
            var updatedFavorites = DoctorRepository.Doctors
                .Where(d => SessionManager.LoggedInUser.FavoriteDoctorNames.Contains(d.Name))
                .ToList();

            FavoriteDoctors.Clear();
            foreach (var doc in updatedFavorites)
                FavoriteDoctors.Add(doc);

            OnPropertyChanged(nameof(FavoriteDoctors));
        }

        public FavoriteDoctorsViewModel()
        {
            LoadFavoriteDoctors();
            ToggleFavoriteCommand = new Command<Doctor>(OnFavoriteClicked);
        }

        private void OnFavoriteClicked(Doctor doctor)
        {
            if (doctor == null) return;

            doctor.IsFavorite = !doctor.IsFavorite;

            if (SessionManager.LoggedInUser.FavoriteDoctorNames.Contains(doctor.Name))
                SessionManager.LoggedInUser.FavoriteDoctorNames.Remove(doctor.Name);
            else
                SessionManager.LoggedInUser.FavoriteDoctorNames.Add(doctor.Name);

            MockDatabase.SaveUsers();
            LoadFavoriteDoctors();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
