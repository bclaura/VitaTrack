using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitaTrack;

namespace VitaTrack;

public class Doctor
{
    public string Name { get; set; }
    public string FirstName {  get; set; }
    public string Specialization { get; set; }
    public string Gender { get; set; } // "M" sau "F"
    public string ImagePath { get; set; }
    public bool IsFavorite { get; set; }
}

public static class DoctorRepository
{
    public static List<Doctor> Doctors = new()
    {
        new Doctor {
            Name = "Dr. Alexander Bennett, Ph.D.",
            Specialization = "Dermato-Genetics",
            Gender = "M",
            ImagePath = "doctor1.jpeg",
            IsFavorite = false
        },
        new Doctor {
            Name = "Dr. Michael Davidson, M.D.",
            Specialization = "Solar Dermatology",
            Gender = "M",
            ImagePath = "doctor2.jpeg",
            IsFavorite = false
        },
        new Doctor {
            Name = "Dr. Olivia Turner, M.D.",
            Specialization = "Dermato-Endocrinology",
            Gender = "F",
            ImagePath = "doctor3.jpeg",
            IsFavorite = false
        },
        new Doctor {
            Name = "Dr. Sophia Martinez, Ph.D.",
            Specialization = "Cosmetic Bioengineering",
            Gender = "F",
            ImagePath = "doctor4.jpeg",
            IsFavorite = false
        },
        new Doctor {
            Name = "Dr. Ethan White, M.D.",
            Specialization = "Medical Aesthetics",
            Gender = "M",
            ImagePath = "doctor5.jpeg",
            IsFavorite = false
        },
        new Doctor {
            Name = "Dr. Amelia Stone, M.D.",
            Specialization = "Pediatric Dermatology",
            Gender = "F",
            ImagePath = "doctor6.jpeg",
            IsFavorite = false
        }
    };
}
