using Android.Graphics;
using System.Collections.ObjectModel;

namespace VitaTrack;

public partial class DoctorsPage : ContentPage
{
    private List<Doctor> AllDoctors = new();
    private ObservableCollection<Doctor> Doctors = new();
    private bool isSortAscending = true;
    private bool isFilterApplied = false;
    public DoctorsPage()
	{
		InitializeComponent();


        NavigationPage.SetHasNavigationBar(this, false);

        AllDoctors = new List<Doctor>
        {
        new Doctor {
            Name = "Dr. Alexander Bennett, Ph.D.",
            Specialization = "Dermato-Genetics",
            Gender = "M",
            ImagePath = "doctor1.jpeg",
            IsFavorite = false,
            Bio = "Dr. Bennett is a leading expert in skin genetics and personalized dermatology. He has published over 50 research papers on rare dermato-genetic conditions and is a passionate advocate for early genetic screening in dermatology.",
            InfoDetails = "Graduated from Oxford Medical School.\n20+ years in clinical research.\nLead author of the Dermatogenetics Handbook (3rd Edition).",
            HelpDetails = "Specializes in rare genetic skin disorders. Tap Info for background or schedule a consultation to explore treatment options."
        },
        new Doctor {
            Name = "Dr. Michael Davidson, M.D.",
            Specialization = "Solar Dermatology",
            Gender = "M",
            ImagePath = "doctor2.jpeg",
            IsFavorite = false,
            Bio = "Dr. Davidson specializes in the effects of sun exposure on skin health. With over 15 years of experience, he is renowned for his work on skin cancer prevention and advanced treatment of sun-related skin disorders.",
            InfoDetails = "Board-certified in Solar Dermatology.\n15+ years of patient care in UV-related skin damage.\nResearch collaborator with NASA on skin exposure in space.",
            HelpDetails = "Focuses on prevention and treatment of sun-induced skin conditions. Use the Info tab to read more, or schedule a personalized UV evaluation."
        },
        new Doctor {
            Name = "Dr. Olivia Turner, M.D.",
            Specialization = "Dermato-Endocrinology",
            Gender = "F",
            ImagePath = "doctor3.jpeg",
            IsFavorite = false,
            Bio = "Dr. Turner blends dermatology with hormonal science to treat conditions like acne, hair loss, and pigmentation disorders. She's highly appreciated for her holistic approach and dedication to patient care.",
            InfoDetails = "Trained in Endocrine Dermatology at Harvard.\nExpert in hormonal skin disorders and acne.\nFeatured speaker at the Global Skin Congress 2023.",
            HelpDetails = "Handles cases where skin issues are linked to hormone imbalances. Tap Info for credentials or book a consultation to begin assessment."
        },
        new Doctor {
            Name = "Dr. Sophia Martinez, Ph.D.",
            Specialization = "Cosmetic Bioengineering",
            Gender = "F",
            ImagePath = "doctor4.jpeg",
            IsFavorite = false,
            Bio = "Dr. Martinez is a pioneer in cosmetic bioengineering, focusing on skin rejuvenation through biocompatible treatments. She collaborates with top biotech labs to develop innovative solutions for aging skin.",
            InfoDetails = "PhD in Cosmetic Bioengineering from UCLA.\nInnovator in dermal nanotechnology.\nOver 50 procedures developed for scar reduction and anti-aging.",
            HelpDetails = "Offers advanced cosmetic solutions using biomedical research. Read more under Info or request an appointment for custom skin care plans."
        },
        new Doctor {
            Name = "Dr. Ethan White, M.D.",
            Specialization = "Medical Aesthetics",
            Gender = "M",
            ImagePath = "doctor5.jpeg",
            IsFavorite = false,
            Bio = "With a refined eye for facial harmony, Dr. White is a sought-after name in non-invasive aesthetics. His patient-centric techniques emphasize natural beauty and long-term skin health.",
            InfoDetails = "Certified in Medical Aesthetics and Regenerative Dermatology.\nCo-founder of SkinForma Clinics.\n10+ years experience in minimally invasive procedures.",
            HelpDetails = "Specializes in facial symmetry, rejuvenation, and non-surgical interventions. Learn more in Info or reach out to schedule a consult."

        },
        new Doctor {
            Name = "Dr. Amelia Stone, M.D.",
            Specialization = "Pediatric Dermatology",
            Gender = "F",
            ImagePath = "doctor6.jpeg",
            IsFavorite = false,
            Bio = "Dr. Stone is known for her gentle and effective treatments for children with skin conditions. She has helped thousands of families navigate complex pediatric dermatological needs with compassion and clarity.",
            InfoDetails = "Pediatric dermatologist trained at Johns Hopkins.\nKnown for gentle, child-centered skin care.\nPublished author on eczema in early childhood.",
            HelpDetails = "Focuses exclusively on children's dermatology. Tap Info to read more or schedule a session for your child’s evaluation."

        }
        };

        Doctors = new ObservableCollection<Doctor>(AllDoctors);
        DoctorsCollectionView.ItemsSource = Doctors;

        //marcheaza doctorii favoriti cand porneste aplicatia
        foreach (var doc in Doctors)
        {
            doc.IsFavorite = SessionManager.LoggedInUser.FavoriteDoctorNames.Contains(doc.Name);
        }

        BindingContext = this;
    }


    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnSortToggleClicked(object sender, EventArgs e)
    {
        if (isFilterApplied)
        {
            ResetFilterIcons();
            isFilterApplied = false;
        }

        isSortAscending = !isSortAscending;
        isFilterApplied = true;

        // Schimbă imaginea
        SortToggleButton.Source = isSortAscending ? "filter_az_icon_active.png" : "filter_za_active_icon.png";

        ResetDoctorList();

        // Sortează doctorii
        var sortedDoctors = isSortAscending
            ? Doctors.OrderBy(d => d.FirstName).ToList()
            : Doctors.OrderByDescending(d => d.FirstName).ToList();

        Doctors.Clear();
        foreach (var doc in sortedDoctors)
            Doctors.Add(doc);
    }

    private void OnFavoriteFilterClicked(object sender, EventArgs e)
    {
        if (isFilterApplied)
        {
            ResetFilterIcons();
            isFilterApplied = false;
        }

        isFilterApplied = true;
        FavoriteFilter.Source = "filter_fav_icon_active.png";

        ResetDoctorList();

        var filtered = Doctors.Where(d => d.IsFavorite).ToList();
        Doctors.Clear();
        foreach (var doc in filtered)
            Doctors.Add(doc);
    }

    private void OnMaleFilterClicked(object sender, EventArgs e)
    {
        if (isFilterApplied)
        {
            ResetFilterIcons();
            isFilterApplied = false;
        }

        isFilterApplied = true;
        MaleFilter.Source = "filter_male_icon_active.png";

        ResetDoctorList();

        var filtered = Doctors.Where(d => d.Gender.Equals("M")).ToList();
        Doctors.Clear();
        foreach(var doc in filtered)
        {
            Doctors.Add(doc);
        }
    }

    private void OnFemaleFilterClicked(object sender, EventArgs e)
    {
        if (isFilterApplied)
        {
            ResetFilterIcons();
            isFilterApplied = false;
        }

        isFilterApplied = true;
        FemaleFilter.Source = "filter_female_icon_active.png";

        ResetDoctorList();

        var filtered = Doctors.Where(d => d.Gender.Equals("F")).ToList();
        Doctors.Clear();
        foreach (var doc in filtered)
        {
            Doctors.Add(doc);
        }
    }

    private void ResetFilterIcons()
    {
        SortToggleButton.Source = "filter_az_icon.png";
        FavoriteFilter.Source = "filter_fav_icon.png";
        MaleFilter.Source = "filter_male_icon.png";
        FemaleFilter.Source = "filter_female_icon.png";
    }

    private void ResetDoctorList()
    {
        Doctors.Clear();
        foreach (var doc in AllDoctors)
            Doctors.Add(doc);
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserDashboardPage());
    }

    private void OnFavoriteClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is Doctor selectedDoctor)
        {
            if (SessionManager.LoggedInUser.FavoriteDoctorNames.Contains(selectedDoctor.Name))
            {
                SessionManager.LoggedInUser.FavoriteDoctorNames.Remove(selectedDoctor.Name);
                selectedDoctor.IsFavorite = false;
            }
            else
            {
                SessionManager.LoggedInUser.FavoriteDoctorNames.Add(selectedDoctor.Name);
                selectedDoctor.IsFavorite = true;
            }

            // Salvează în MockDatabase
            MockDatabase.SaveUsers();
        }
    }

    private async void OnInfoClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Doctor selectedDoctor)
        {
            await Navigation.PushAsync(new DoctorDetailPage(selectedDoctor));
        }
    }

    private async void OnInfoIconClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is Doctor doctor)
        {
            await DisplayAlert("More Info", doctor.InfoDetails ?? "No additional info available.", "OK");
        }
    }

    private async void OnHelpIconClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is Doctor doctor)
        {
            await DisplayAlert("Help", doctor.HelpDetails ?? "This doctor specializes in dermatology. You can tap Info for more or favorite to save.", "OK");
        }
    }






}