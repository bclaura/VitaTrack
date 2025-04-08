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
        new Doctor { Name = "Dr. Alexander Bennett, Ph.D.", FirstName = "Bennet", Specialization = "Dermato-Genetics", Gender = "M", ImagePath = "doctor1.jpeg", IsFavorite = false },
        new Doctor { Name = "Dr. Michael Davidson, M.D.", FirstName = "Davidson", Specialization = "Solar Dermatology", Gender = "M", ImagePath = "doctor2.jpeg", IsFavorite = false },
        new Doctor { Name = "Dr. Olivia Turner, M.D.", FirstName = "Turner", Specialization = "Dermato-Endocrinology", Gender = "F", ImagePath = "doctor3.jpeg", IsFavorite = false },
        new Doctor { Name = "Dr. Sophia Martinez, Ph.D.", FirstName = "Martinez", Specialization = "Cosmetic Bioengineering", Gender = "F", ImagePath = "doctor4.jpeg", IsFavorite = false },
        new Doctor { Name = "Dr. Ethan White, M.D.", FirstName = "White", Specialization = "Medical Aesthetics", Gender = "M", ImagePath = "doctor5.jpeg", IsFavorite = false },
        new Doctor { Name = "Dr. Amelia Stone, M.D.", FirstName = "Stone", Specialization = "Pediatric Dermatology", Gender = "F", ImagePath = "doctor6.jpeg", IsFavorite = false }
        };

        Doctors = new ObservableCollection<Doctor>(AllDoctors);
        DoctorsCollectionView.ItemsSource = Doctors;

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




}