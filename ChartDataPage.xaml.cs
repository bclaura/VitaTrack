using Android.OS;
using AndroidX.CardView.Widget;
using Microcharts;
using Microcharts.Maui;
using Newtonsoft.Json;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VitaTrack;

public partial class ChartDataPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private List<int> ecgValues = new(); 
    private List<EcgSignalEntry> _ecgSignals = new();
    public int EcgCanvasWidth { get; set; } = 1000;
    private List<int> ecgValuesFull = new();     
    private List<int> ecgValuesVisible = new();  
    private CancellationTokenSource _animationCts;
    private bool _userScrolled = false;
    float stepX = 6f;



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

    private void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        _userScrolled = true;
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

    private List<EcgDataPoint> CalculateGroupedAverages(List<EcgDataPoint> rawData, int groupSize = 10)
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

    /*private void DisplayChart3FilteredECG(List<EcgDataPoint> rawData)
    {
        var normalizedEntries = NormalizeEcg(rawData); 

        ChartView3.Chart = new LineChart
        {
            Entries = normalizedEntries,
            LineMode = LineMode.Straight,
            LineSize = 3,
            PointMode = PointMode.None,
            MinValue = -100, 
            MaxValue = 300,
            LabelTextSize = 20,
            BackgroundColor = SKColors.White
        };
    }*/

    private List<ChartEntry> NormalizeEcg(List<EcgDataPoint> data)
    {
        if (data.Count == 0) return new();

        double avg = data.Average(p => p.Ecg);

        return data.Select(p =>
        {
            float shifted = p.Ecg - (float)avg;

            return new ChartEntry(shifted)
            {
                Label = "",
                ValueLabel = "",
                Color = SKColor.Parse("#FF4C4C")
            };
        }).ToList();
    }

    private void OnCanvasPaint(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);


        float baseY = e.Info.Height / 2f;
        float stepX = 6;

        var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Blue,
            StrokeWidth = 2,
            IsAntialias = true
        };

        /*canvas.DrawLine(0, baseY, e.Info.Width, baseY, new SKPaint
        {
            Color = SKColors.Gray,
            StrokeWidth = 1,
            PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0)
        });*/

        for (int i = 1; i < ecgValuesVisible.Count; i++)
        {
            float x1 = (i - 1) * stepX;
            float y1 = baseY - Normalize(ecgValuesVisible[i - 1]);

            float x2 = i * stepX;
            float y2 = baseY - Normalize(ecgValuesVisible[i]);

            canvas.DrawLine(x1, y1, x2, y2, paint);
        }
    }


    private float Normalize(int ecgValue)
    {
        float center = 600f; 
        float scale = 0.6f;  
        return (ecgValue - center) * scale;
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
        //DisplayChart3FilteredECG(groupedData);

        /*ecgValues = rawData.Select(p => p.Ecg).ToList();
        int spacing = 6; // testează cu 6, 8, 10
        EcgCanvasWidth = ecgValues.Count * spacing;
        OnPropertyChanged(nameof(EcgCanvasWidth)); 
        EcgCanvas.InvalidateSurface();*/

        
        ecgValuesFull = rawData.Select(p => p.Ecg).ToList();
        ecgValuesVisible = new List<int>(); 

        
        EcgCanvasWidth = ecgValuesFull.Count * 2;
        OnPropertyChanged(nameof(EcgCanvasWidth));

        
        await StartEcgAnimationAsync();

        ecgValuesFull = rawData.Select(p => p.Ecg).ToList();
        ecgValuesVisible.Clear();

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

    private async Task StartEcgAnimationAsync()
    {
        _animationCts?.Cancel();
        _animationCts = new CancellationTokenSource();
        var token = _animationCts.Token;

        for (int i = 0; i < ecgValuesFull.Count; i++)
        {
            if (token.IsCancellationRequested) break;

            ecgValuesVisible.Add(ecgValuesFull[i]);
            EcgCanvas.InvalidateSurface();

            await Task.Delay(30); 

            
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (!_userScrolled)
                {
                    double scrollX = i * stepX; 
                    await ScrollViewRef.ScrollToAsync(scrollX, 0, animated: true);
                }
            });
        }
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
    public string Signal { get; set; } 
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
    public int Index { get; set; } 
    public int Bpm { get; set; }
    public int Ecg { get; set; }
}

public class EcgSignalSummary
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime Timestamp { get; set; }

    public override string ToString() => Timestamp.ToString("g");
}


