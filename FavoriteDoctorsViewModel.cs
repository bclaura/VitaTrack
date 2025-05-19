using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Json;
using System.Windows.Input;

namespace VitaTrack
{
    public class FavoriteDoctorsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Doctor> FavoriteDoctors { get; set; } = new();
        public ICommand ToggleFavoriteCommand { get; }

        private readonly HttpClient _httpClient;

        public FavoriteDoctorsViewModel()
        {
            _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
            ToggleFavoriteCommand = new Command<Doctor>(OnFavoriteClicked);
            LoadFavoriteDoctors();
        }

        public async void LoadFavoriteDoctors()
        {
            try
            {
                int? userId = await SessionManager.GetLoggedInUserIdAsync();
                if (userId == null)
                {
                    Console.WriteLine("User not logged in.");
                    return;
                }

                var response = await _httpClient.GetFromJsonAsync<List<Doctor>>($"/api/users/{userId}/favorites");
                if (response != null)
                {
                    FavoriteDoctors.Clear();
                    foreach (var dto in response)
                    {
                        var doctor = new Doctor
                        {
                            Id = dto.Id,
                            LastName = dto.LastName,
                            FullName = dto.FullName,
                            Gender = dto.Gender,
                            ProfilePictureBase64 = dto.ProfilePictureBase64,
                            Bio = dto.Bio,
                            AvailabilityHours = dto.AvailabilityHours,
                            ClinicAddress = dto.ClinicAddress,
                            HonorificTitle = dto.HonorificTitle,
                            Specialization = dto.Specialization,
                            IsFavorite = true
                        };

                        FavoriteDoctors.Add(doctor);
                    }

                    OnPropertyChanged(nameof(FavoriteDoctors));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading favorite doctors: {ex.Message}");
            }
        }

        private async void OnFavoriteClicked(Doctor doctor)
        {
            if (doctor == null) return;

            int? userId = await SessionManager.GetLoggedInUserIdAsync();
            if (userId == null) return;

            try
            {
                if (doctor.IsFavorite)
                {
                    // Elimină din favoriți
                    var response = await _httpClient.DeleteAsync($"/api/users/{userId}/favorites/{doctor.Id}");
                    if (response.IsSuccessStatusCode)
                    {
                        doctor.IsFavorite = false;
                        FavoriteDoctors.Remove(doctor);
                        OnPropertyChanged(nameof(FavoriteDoctors));
                    }
                }
                else
                {
                    // Adaugă la favoriți
                    var response = await _httpClient.PostAsync($"/api/users/{userId}/favorites/{doctor.Id}", null);
                    if (response.IsSuccessStatusCode)
                    {
                        doctor.IsFavorite = true;
                        if (!FavoriteDoctors.Contains(doctor))
                        {
                            FavoriteDoctors.Add(doctor);
                        }
                        OnPropertyChanged(nameof(FavoriteDoctors));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating favorite: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
