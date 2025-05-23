using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitaTrack
{
    public class Patient
    {
        public int Id { get; set; }
        public string Cnp { get; set; }
        public int Age { get; set; }
        public string AdressStreet { get; set; }
        public string AdressCity { get; set; }
        public string AdressCounty { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Workplace { get; set; }
        public string Occupation { get; set; }

    }

    public class PatientUpdateDto
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AdressStreet { get; set; }
        public string? AdressCity { get; set; }
        public string? AdressCounty { get; set; }
        public string? Occupation { get; set; }
        public string? Workplace { get; set; }
        public string? Cnp { get; set; }
    }

}
