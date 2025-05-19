using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitaTrack;

namespace VitaTrack;

public class Doctor : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string Gender { get; set; } // "M" sau "F"
    public string ProfilePictureBase64 { get; set; }
    public string Bio {  get; set; }
    public string AvailabilityHours { get; set; }
    public string ClinicAddress { get; set; }
    public bool IsFavorite { get; set; }
    public string HonorificTitle { get; set; }
    public string Specialization {  get; set; }

    public string ImagePath => $"doctor{Id}.jpeg";
    public string NameTitle => "Dr. " + FullName + ", " + HonorificTitle;

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}