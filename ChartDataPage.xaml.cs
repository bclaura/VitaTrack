using Android.OS;
using AndroidX.CardView.Widget;
using Microcharts;
using Newtonsoft.Json;
using SkiaSharp;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VitaTrack;

public partial class ChartDataPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private List<EcgSignalEntry> _ecgSignals = new();



    public ChartDataPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();


        int? userId = await SessionManager.GetLoggedInUserIdAsync();
        if (userId == null) return;


        var patient = await _httpClient.GetFromJsonAsync<Patient>($"api/patients/byUserId/{userId}");
        if (patient == null) return;

        int patientId = patient.Id;

        GetEcgSignalSummariesAsync(patientId);

    }

    private List<EcgDataPoint> ParseSignalString(string signalRaw)
    {
        var dataPoints = new List<EcgDataPoint>();
        var readings = signalRaw.Split('|', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < readings.Length; i++)
        {
            var parts = readings[i].Split(';');
            if (parts.Length != 2) continue;

            var bpmPart = parts[0].Replace("BPM=", "");
            var ecgPart = parts[1].Replace("ECG=", "");

            if (int.TryParse(bpmPart, out int bpm) && int.TryParse(ecgPart, out int ecg))
            {
                dataPoints.Add(new EcgDataPoint
                {
                    Index = i,
                    Bpm = bpm,
                    Ecg = ecg
                });
            }
        }

        return dataPoints;
    }

    private List<EcgDataPoint> CalculateGroupedAverages(List<EcgDataPoint> rawData, int groupSize = 5)
    {
        var grouped = new List<EcgDataPoint>();

        for (int i = 0; i < rawData.Count; i += groupSize)
        {
            var group = rawData.Skip(i).Take(groupSize).ToList();
            if (group.Count == 0) continue;

            grouped.Add(new EcgDataPoint
            {
                Index = grouped.Count,
                Bpm = (int)group.Average(p => p.Bpm),
                Ecg = (int)group.Average(p => p.Ecg)
            });
        }

        return grouped;
    }





    private List<EcgReading> ParseSignal(string signal)
    {
        var readings = new List<EcgReading>();
        var entries = signal.Split('|', StringSplitOptions.RemoveEmptyEntries);

        foreach (var entry in entries)
        {
            var parts = entry.Split(';');
            int bpm = 0, ecg = 0;

            foreach (var part in parts)
            {
                if (part.StartsWith("BPM="))
                    int.TryParse(part.Substring(4), out bpm);
                else if (part.StartsWith("ECG="))
                    int.TryParse(part.Substring(4), out ecg);
            }

            if (bpm > 0 && ecg > 0)
                readings.Add(new EcgReading { Bpm = bpm, Ecg = ecg });
        }

        return readings;
    }


    private List<EcgSignalSummary> _signalSummaries = new();

    private async Task<List<EcgSignalSummary>> GetEcgSignalSummariesAsync(int patientId)
    {
        var response = await _httpClient.GetAsync($"api/EcgSignal/patient/{patientId}/summaries");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var summaries = JsonConvert.DeserializeObject<List<EcgSignalSummary>>(json);

            _signalSummaries = summaries.OrderByDescending(s => s.Timestamp).ToList();
            SignalPicker.ItemsSource = _signalSummaries;
            return _signalSummaries;
        }

        return new List<EcgSignalSummary>();
    }


    /*private void OnSignalSelected(object sender, EventArgs e)
    {
        if (SignalPicker.SelectedIndex == -1)
            return;

        var selectedSignal = (EcgSignalEntry)SignalPicker.SelectedItem;
        Debug.WriteLine($"Ai selectat: {selectedSignal.Timestamp}");

        // aici urmeaz? parsingul ?i generarea graficului
        //ParseSignalAndDisplayChart(selectedSignal.Signal);
    }*/



    private void DisplayChart(List<EcgDataPoint> data)
    {
        var entries = data.Select(p =>
            new ChartEntry(p.Bpm)
            {
                ValueLabel = $"BPM:{p.Bpm}",
                Color = SKColor.Parse("#2260FF")

            }).ToList();

        ChartView.Chart = new LineChart
        {
            Entries = entries,
            LineMode = LineMode.Straight,
            LineSize = 4,
            PointSize = 6,
            BackgroundColor = SKColors.White,
            LabelTextSize = 30
        };
    }

    private void DisplayChart2(List<EcgDataPoint> data)
    {
        var entries2 = data.Select(p =>
            new ChartEntry(p.Ecg)
            {
                ValueLabel = $"ECG:{p.Ecg}",
                Color = SKColor.Parse("#ff2259")

            }).ToList();

        ChartView2.Chart = new LineChart
        {
            Entries = entries2,
            LineMode = LineMode.Straight,
            LineSize = 4,
            PointSize = 6,
            BackgroundColor = SKColors.White,
            LabelTextSize = 30
        };
    }

    private async void OnSignalSelected(object sender, EventArgs e)
    {
        if (SignalPicker.SelectedIndex == -1) return;

        var selectedSummary = (EcgSignalSummary)SignalPicker.SelectedItem;
        if (selectedSummary == null) return;

        int signalId = selectedSummary.Id;

        var fullSignal = await GetEcgSignalById(signalId);
        if (fullSignal == null) return;

        var rawData = ParseSignalString(fullSignal.Signal);
        var groupedData = CalculateGroupedAverages(rawData);
        DisplayChart(groupedData);
        DisplayChart2(groupedData);

        int avgBpm = (int)rawData.Average(p => p.Bpm);
        int maxBpm = rawData.Max(p => p.Bpm);
        int minBpm = rawData.Min(p => p.Bpm);

        string evaluation;
        if(avgBpm < 60)
        {
            evaluation = "Low heart rate (possible bradycardia)";
        }
        else if(avgBpm > 100)
        {
            evaluation = "Elevated heart rate (possible tachycardia)";
        }
        else
        {
            evaluation = "Normal heart rate";
        }

        StatsLabelAvg.Text = avgBpm.ToString();
        StatsLabelMax.Text = maxBpm.ToString();
        StatsLabelMin.Text = minBpm.ToString();
        StatsLabelEvaluation.Text = evaluation;


    }

    private async Task<EcgSignal> GetEcgSignalById(int id)
    {
        var response = await _httpClient.GetAsync($"api/EcgSignal/{id}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<EcgSignal>(json);
        }

        return null;
    }






    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserDashboardPage());
    }

    private void OnProfileClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserProfilePage());
    }

    private void OnMessagesClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MessageDoctorPage());
    }

    private void OnCalendaryClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CalendarPage());
    }
}

public class EcgSignal
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Signal { get; set; }  // Ex: BPM=144;ECG=835|BPM=148;ECG=836...
    public DateTime Timestamp { get; set; }
}

public class EcgReading
{
    public int Bpm { get; set; }
    public int Ecg { get; set; }
}

public class EcgSignalEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Signal { get; set; }

    public override string ToString()
    {
        return $"{Id}. {Timestamp:yyyy-MM-dd HH:mm:ss}";
    }
}

public class EcgDataPoint
{
    public int Index { get; set; } // Pentru axa X
    public int Bpm { get; set; }
    public int Ecg { get; set; }
}

public class EcgSignalSummary
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime Timestamp { get; set; }

    public override string ToString() => Timestamp.ToString("g"); // pentru afi?are în Picker
}


