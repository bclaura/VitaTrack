using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;


namespace VitaTrack;

public partial class CalendarPage : ContentPage
{
    private DateTime currentMonth;
    private DateTime? selectedDate;
    private readonly HttpClient _httpClient;
    public ObservableCollection<RecommendationDto> Recommendations { get; set; } = new();
    private HashSet<DateTime> activeDays = new();

    private Dictionary<DateTime, List<PhysicalActivityDto>> activityByDay = new();
    private ObservableCollection<PhysicalActivityDto> selectedDayActivities = new();

    public CalendarPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
        currentMonth = DateTime.Now;
        GenerateCalendar(currentMonth);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadRecommendationsAsync();
        await LoadPhysicalActivitiesAsync();

        GenerateCalendar(currentMonth);
        RecommendationsCollectionView.ItemsSource = Recommendations;
        SelectedDayActivitiesCollectionView.ItemsSource = selectedDayActivities;
    }

    private void GenerateCalendar(DateTime month)
    {
        CalendarGrid.Children.Clear();
        CalendarGrid.RowDefinitions.Clear();
        CalendarGrid.ColumnDefinitions.Clear();

        for (int i = 0; i < 7; i++)
            CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        for (int i = 0; i < 6; i++)
            CalendarGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        MonthLabel.Text = FormatMonth(month);

        DateTime firstDay = new DateTime(month.Year, month.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
        int dayOfWeek = (int)firstDay.DayOfWeek;
        if (dayOfWeek == 0) dayOfWeek = 7;

        int row = 0;
        int column = dayOfWeek - 1;

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(month.Year, month.Month, day);
            bool isActiveDay = activeDays.Contains(date);

            var button = new Button
            {
                Text = day.ToString(),
                BackgroundColor = isActiveDay ? Color.FromArgb("#4CAF50") : Colors.White,
                TextColor = Colors.Black,
                CornerRadius = 10,
                FontSize = 14,
                Command = new Command(() => OnDateSelected(date))
            };

            Grid.SetColumn(button, column);
            Grid.SetRow(button, row);
            CalendarGrid.Children.Add(button);

            column++;
            if (column > 6)
            {
                column = 0;
                row++;
            }
        }
    }


    private async Task LoadRecommendationsAsync()
    {
        try
        {
            int? userId = await SessionManager.GetLoggedInUserIdAsync();
            if (userId == null) return;

            var patient = await _httpClient.GetFromJsonAsync<Patient>($"api/patients/byUserId/{userId}");
            if (patient == null) return;

            var response = await _httpClient.GetAsync($"api/Recommendations/{patient.Id}");
            if (!response.IsSuccessStatusCode) return;

            var allRecs = await response.Content.ReadFromJsonAsync<List<RecommendationDto>>();
            var patientRecs = allRecs.Where(r => r.PatientId == patient.Id).ToList();

            Recommendations.Clear();
            foreach (var rec in patientRecs)
                Recommendations.Add(rec);

            NoRecommendationsLabel.IsVisible = !patientRecs.Any();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load recommendations: {ex.Message}", "OK");
        }
    }

    private async Task LoadPhysicalActivitiesAsync()
    {
        try
        {
            int? userId = await SessionManager.GetLoggedInUserIdAsync();
            if (userId == null) return;

            var patient = await _httpClient.GetFromJsonAsync<Patient>($"api/patients/byUserId/{userId}");
            if (patient == null) return;

            var response = await _httpClient.GetAsync($"api/PhysicalActivity/{patient.Id}");
            if (!response.IsSuccessStatusCode) return;

            var activities = await response.Content.ReadFromJsonAsync<List<PhysicalActivityDto>>();

            activeDays.Clear();

            foreach (var activity in activities)
            {
                DateTime current = activity.StartTime.Date;
                DateTime end = activity.EndTime.Date;

                while (current <= end)
                {
                    var key = current.Date;

                    if(!activityByDay.ContainsKey(key))
                    {
                        activityByDay[key] = new List<PhysicalActivityDto>();
                    }

                    activeDays.Add(key);
                    activityByDay[key].Add(activity);
                    current = current.AddDays(1);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load activities: {ex.Message}", "OK");
        }
    }




    private string FormatMonth(DateTime date)
    {
        var text = date.ToString("MMMM yyyy", new CultureInfo("ro-RO"));
        return char.ToUpper(text[0]) + text.Substring(1);
    }

    private void OnDateSelected(DateTime date)
    {
        selectedDate = date;
        var normalizedDate = date.Date;
        var text = date.ToString("dd MMMM yyyy", new CultureInfo("ro-RO"));
        var splitList = text.Split(' ');
        var month = splitList[1];

        SelectedDateLabel.Text = $"You selected " + splitList[0] + " " + char.ToUpper(month[0]) + month.Substring(1) + " " + splitList[2];

        if (activityByDay.TryGetValue(normalizedDate.Date, out var activities))
        {
            SelectedDayActivitiesPanel.IsVisible = true;
            selectedDayActivities.Clear();
            foreach (var activity in activities)
                selectedDayActivities.Add(activity);
        }
        else
        {
            SelectedDayActivitiesPanel.IsVisible = false;
            selectedDayActivities.Clear();
        }
    }

    private void OnPrevMonthClicked(object sender, EventArgs e)
    {
        currentMonth = currentMonth.AddMonths(-1);
        GenerateCalendar(currentMonth);
    }

    private void OnNextMonthClicked(object sender, EventArgs e)
    {
        currentMonth = currentMonth.AddMonths(1);
        GenerateCalendar(currentMonth);
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserDashboardPage());
    }

    private void OnMessagesClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MessageDoctorPage());
    }

    private void OnProfileClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserProfilePage());
    }
}

public class RecommendationDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string RecommendationType { get; set; }
    public int DailyDuration { get; set; }
    public string AdditionalInstructions { get; set; }
}

public class PhysicalActivityDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string ActivityType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Duration { get; set; }
}

