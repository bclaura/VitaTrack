using Android.AdServices.Common;
using Android.Gms.Common.Apis;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using VitaTrack.Platforms.Android;

namespace VitaTrack;

public partial class HealthPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private IBluetoothClassicService _bluetoothService;
    private Queue<int> bpmHistory = new Queue<int>();
    private List<HeartbeatReading> heartbeatReadings = new List<HeartbeatReading>();
    private readonly object _readingLock = new object();
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isActiveOnPage = false;
    private bool _shouldRetry = true;
    private bool _awaitingReset = false;
    private DateTime _lastDataReceivedTime = DateTime.MinValue;
    private bool _isBlinking = false;
    private CancellationTokenSource _blinkTokenSource;
    private bool _dataAlreadySent = false;
    private int zeroStreak = 0;
    private bool allowZeros = true;
    private CancellationTokenSource _failsafeCts;
    private DateTime _firstDataReceivedAt;
    private bool _failsafeTriggered = false;


    public HealthPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        BindingContext = this;
        _httpClient = Application.Current.Handler.MauiContext.Services.GetService<HttpClient>();

        _bluetoothService = Application.Current.Handler.MauiContext.Services.GetService<IBluetoothClassicService>();
        //_bluetoothService = Application.Current.Handler.MauiContext.Services.GetRequiredService<IBluetoothClassicService>();


        if (_bluetoothService == null)
        {
            Debug.WriteLine("Bluetooth service is null. Ensure platform-specific implementation is registered.");
        }
        else
        {
            _bluetoothService.StatusChanged += OnStatusChanged;
            _bluetoothService.DataReceived += OnDataReceived;

        }
    }



    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_bluetoothService != null)
        {
            await _bluetoothService.ConnectAsync("98:D3:31:F7:44:C0");
        }

        _isActiveOnPage = true;
        _shouldRetry = true;

        await LoadPatientAndHighlightAsync();

        _cancellationTokenSource = new CancellationTokenSource();
        _ = MonitorConnectionLoop(_cancellationTokenSource.Token);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isActiveOnPage = false;
        _shouldRetry = false;
        _cancellationTokenSource?.Cancel();
        _bluetoothService?.StopListening();
        _blinkTokenSource?.Cancel();
        _isBlinking = false;
    }

    private string GetAgeCategory(int age)
    {
        if (age <= 1) return "0-1 ani";
        if (age <= 10) return "1–10 ani";
        if (age <= 17) return "11–17 ani";
        if (age <= 45) return "18–45 ani";
        return "45+ ani";
    }

    private async Task LoadPatientAndHighlightAsync()
    {
        int? userId = await SessionManager.GetLoggedInUserIdAsync();
        if (userId == null) return;

        try
        {
            var user = await _httpClient.GetFromJsonAsync<User>($"api/users/{userId}");
            var patient = await _httpClient.GetFromJsonAsync<Patient>($"api/patients/byUserId/{userId}");

            int age = patient.Age;
            string category = GetAgeCategory(age);

            HighlightAgeCategory(category);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", $"Nu s-a putut înc?rca datele: {ex.Message}", "OK");
        }
    }

    private void HighlightAgeCategory(string category)
    {
        Color highlight = Color.FromArgb("#D0E8FF");

        switch (category)
        {
            case "0-1 ani":
                AgeLabel_0_1.BackgroundColor = highlight;
                MaleLabel_0_1.BackgroundColor = highlight;
                FemaleLabel_0_1.BackgroundColor = highlight;
                break;

            case "1–10 ani":
                AgeLabel_1_10.BackgroundColor = highlight;
                MaleLabel_1_10.BackgroundColor = highlight;
                FemaleLabel_1_10.BackgroundColor = highlight;
                break;

            case "11–17 ani":
                AgeLabel_11_17.BackgroundColor = highlight;
                MaleLabel_11_17.BackgroundColor = highlight;
                FemaleLabel_11_17.BackgroundColor = highlight;
                break;

            case "18–45 ani":
                AgeLabel_18_45.BackgroundColor = highlight;
                MaleLabel_18_45.BackgroundColor = highlight;
                FemaleLabel_18_45.BackgroundColor = highlight;
                break;

            case "45+ ani":
                AgeLabel_45Plus.BackgroundColor = highlight;
                MaleLabel_45Plus.BackgroundColor = highlight;
                FemaleLabel_45Plus.BackgroundColor = highlight;
                break;
        }
    }


    private void OnStatusChanged(object sender, string status)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            //StatusLabel.Text = status;

            /*if (status.Contains("Conexiune pierdută") || status.Contains("Socket"))
            {
                _shouldRetry = true;
            }*/

            if (status.Contains("Connected", StringComparison.OrdinalIgnoreCase))
            {
                BluetoothStatusLabel.Text = "Connected to HC-05";
                BluetoothStatusLabel.TextColor = Colors.Green;
            }
            else if (status.Contains("Connecting", StringComparison.OrdinalIgnoreCase))
            {
                BluetoothStatusLabel.Text = "Connecting...";
                BluetoothStatusLabel.TextColor = Colors.Orange;
            }
            else if (status.Contains("Conexiune pierdută") || status.Contains("Socket") || status.Contains("Failed"))
            {
                BluetoothStatusLabel.Text = "Not connected";
                BluetoothStatusLabel.TextColor = Colors.Red;
                _shouldRetry = true;
            }
        });

        Debug.WriteLine("Bluetooth Status: " + status);
    }




    private void OnDataReceived(object sender, string data)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (BluetoothStatusLabel.Text != "Connected to HC-05")
            {
                BluetoothStatusLabel.Text = "Connected to HC-05";
                BluetoothStatusLabel.TextColor = Colors.Green;
            }
        });

        Debug.WriteLine("Data received: " + data);
        Debug.WriteLine("Failsafe pornit");

        if (_firstDataReceivedAt == default)
        {
            _firstDataReceivedAt = DateTime.Now;
            _failsafeTriggered = false;

            _failsafeCts?.Cancel();
            _failsafeCts = new CancellationTokenSource();

            _ = MonitorFailsafeAsync(_failsafeCts.Token);
        }

        var lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int ecgValue) &&
                int.TryParse(parts[1], out int bpmValue))
            {
                
                if (!ShouldAcceptEcg(ecgValue))
                    continue;

               
                var reading = new HeartbeatReading
                {
                    EcgValue = ecgValue,
                    Bpm = bpmValue
                };

                lock (_readingLock)
                {
                    heartbeatReadings.Add(reading);
                }

                Debug.WriteLine($"Heartbeat adăugat: BPM={reading.Bpm}, ECG={reading.EcgValue}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    string displayLog;
                    lock (_readingLock)
                    {
                        displayLog = string.Join("\n", heartbeatReadings
                            .TakeLast(5)
                            .Select(r => r.ToString()));
                    }
                    
                });

                _lastDataReceivedTime = DateTime.Now;
                _awaitingReset = false;

                if (!_isBlinking)
                {
                    _isBlinking = true;
                    _blinkTokenSource = new CancellationTokenSource();
                    _ = BlinkBpmAsync(_blinkTokenSource.Token);
                }
            }
        

            
            if (line.Contains("Stopped", StringComparison.OrdinalIgnoreCase))
            {
                _failsafeCts?.Cancel();
                _firstDataReceivedAt = default;
                _failsafeTriggered = false;

                Debug.WriteLine("Mesaj de oprire detectat din Arduino: " + line);

                
                if (!_awaitingReset && !_dataAlreadySent)
                {
                    _awaitingReset = true;
                    _dataAlreadySent = true; 

                    
                    _isBlinking = false;
                    _blinkTokenSource?.Cancel();
                    AfiseazaMediaBpm();

                    _ = MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        LastUploadLabel.Text = "ECG stopped. Sending data to cloud...";
                    });

                    _ = Task.Run(async () =>
                    {
                        Debug.WriteLine("==== Încep trimiterea datelor către API ====");
                        await TrimiteDateLaApiAsync();
                        Debug.WriteLine("==== Trimiterea s-a încheiat ====");

                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            LastUploadLabel.Text = $"Last data sent to cloud: {DateTime.Now:HH:mm:ss}";
                        });

                        Debug.WriteLine("Aștept 10 secunde pentru o posibilă reconectare...");
                        await Task.Delay(10000);

                        _shouldRetry = true;
                        _dataAlreadySent = false; 
                        Debug.WriteLine("Pregătit pentru reconectare dacă Arduino e resetat.");
                    });

                    return; 
                }
                else
                {
                    Debug.WriteLine("Trimiterea deja a fost inițiată. Ignor acest mesaj duplicat.");
                }
            }
        }
    }

    private async Task MonitorFailsafeAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(70), token);

            if (!token.IsCancellationRequested && !_dataAlreadySent && !_failsafeTriggered)
            {
                _failsafeTriggered = true;
                Debug.WriteLine("Failsafe activat – nu a venit mesajul 'Stopped'");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LastUploadLabel.Text = "Timeout: Sending data to cloud...";
                });

                _isBlinking = false;
                _blinkTokenSource?.Cancel();
                AfiseazaMediaBpm();

                lock (_readingLock)
                {
                    if (heartbeatReadings.Count < 30)
                    {
                        Debug.WriteLine($"Failsafe: prea puține date ({heartbeatReadings.Count}), nu trimit nimic.");
                        return;
                    }
                }

                await TrimiteDateLaApiAsync();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LastUploadLabel.Text = $"Data send at: {DateTime.Now:HH:mm:ss}";
                });

                _dataAlreadySent = true;
                _awaitingReset = true;

                await Task.Delay(10000);
                _dataAlreadySent = false;
            }
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("Failsafe timer anulat – mesaj 'Stopped' primit între timp.");
        }
    }

    private async Task BlinkBpmAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if ((DateTime.Now - _lastDataReceivedTime).TotalSeconds > 5)
            {
                _isBlinking = false;
                _blinkTokenSource.Cancel();
                AfiseazaMediaBpm();

                break;
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                
                await HeartIcon.ScaleTo(1.1, 250, Easing.CubicInOut);
                await HeartIcon.ScaleTo(1.0, 250, Easing.CubicInOut);
            });

            await Task.Delay(500, token); 
        }
    }


    private void AfiseazaMediaBpm()
    {
        List<HeartbeatReading> filteredReadings;

        lock (_readingLock)
        {
            filteredReadings = heartbeatReadings
                .Where(r => r.Bpm >= 40 && r.Bpm <= 180 && r.EcgValue >= 600 && r.EcgValue <= 1023)
                .ToList();
        }

        if (filteredReadings.Count == 0)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BpmLabel.Text = "--";
            });
            return;
        }

        int media = (int)filteredReadings.Average(r => r.Bpm);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            BpmLabel.Text = media.ToString();
        });
    }




    private int? FilterAndSmoothBpm(int rawBpm)
    {

        
        if (rawBpm < 40 || rawBpm > 180)
            return null;

        
        bpmHistory.Enqueue(rawBpm);
        if (bpmHistory.Count > 5)
            bpmHistory.Dequeue();

        Debug.WriteLine($"Valoare BPM adăugată în istoric: {rawBpm}, medie: {(int)bpmHistory.Average()}");


        return (int)bpmHistory.Average();
    }

    private void OnShowAllReadingsClicked(object sender, EventArgs e)
    {
        string all;
        lock (_readingLock)
        {
            all = string.Join("\n", heartbeatReadings.Select(r => r.ToString()));
        }
        DisplayAlert("Istoric", all, "OK");
    }

    private async Task TrimiteDateLaApiAsync()
    {
        List<HeartbeatReading> filteredReadings;

        lock (_readingLock)
        {
            filteredReadings = new List<HeartbeatReading>(heartbeatReadings);
        }

        var statsForBpm = filteredReadings.Where(r => r.Bpm >= 40 && r.Bpm <= 180).ToList();

        if (filteredReadings.Any())
        {
            
            int avgBpm = (int)statsForBpm.Average(r => r.Bpm);
            int maxBpm = statsForBpm.Max(r => r.Bpm);
            int minBpm = statsForBpm.Min(r => r.Bpm);

            
            string evaluation = GetHeartRateEvaluation(avgBpm);

            
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                StatsLabelAvg.Text = avgBpm.ToString();
                StatsLabelMax.Text = maxBpm.ToString();
                StatsLabelMin.Text = minBpm.ToString();
                StatsLabelEvaluation.Text = evaluation;
            });

            string compressed = string.Join("|", filteredReadings
            .Select(r => $"BPM={r.Bpm};ECG={r.EcgValue}"));

            
            if (compressed.Length > 3800)
            {
                compressed = compressed.Substring(0, 3800);
            }

            
            int? userId = await SessionManager.GetLoggedInUserIdAsync();
            if (userId == null) return;

            var patient = await _httpClient.GetFromJsonAsync<Patient>($"api/patients/byUserId/{userId}");
            if (patient == null) return;

            var payload = new
            {
                patientId = patient.Id, 
                signal = compressed
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/EcgSignal", content);

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Date trimise cu succes.");

                    lock (_readingLock)
                    {
                        heartbeatReadings.Clear();
                    }

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        LastUploadLabel.Text = $"Last data sent to cloud: {DateTime.Now:HH:mm:ss}";
                    });
                }
                else
                {
                    Debug.WriteLine($"Eroare la trimitere: {response.StatusCode}");

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        LastUploadLabel.Text = "Failed to send data to cloud.";
                    });
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare rețea/API: {ex.Message}");
            }
        }
    }

    private bool ShouldAcceptEcg(int ecgValue)
    {
        if (ecgValue == 0)
        {
            if (!allowZeros) return false;

            zeroStreak++;
            if (zeroStreak >= 10)
            {
                allowZeros = false;
            }

            return true;
        }
        else
        {
            zeroStreak = 0;
            allowZeros = true;
            return true;
        }
    }

    private string GetHeartRateEvaluation(int bpm)
    {
        return bpm switch
        {
            < 60 => "Low heart rate (possible bradycardia)",
            > 100 => "Elevated heart rate (possible tachycardia)",
            _ => "Normal heart rate"
        };
    }


    private async Task MonitorConnectionLoop(CancellationToken token)
    {
        while (_isActiveOnPage && !token.IsCancellationRequested)
        {
            try
            {
                if (_shouldRetry)
                {
                    await _bluetoothService.EnsurePermissionsAsync();

                    if (!_bluetoothService.IsConnected)
                    {
                        Debug.WriteLine("Reconectare...");
                        await _bluetoothService.ConnectAsync("98:D3:31:F7:44:C0");
                    }

                    bool success = await _bluetoothService.StartListeningAsync();
                    _shouldRetry = !success; 
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare reconectare: {ex.Message}");
                _shouldRetry = true;
            }

            await Task.Delay(5000, token);
        }
    }





    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserDashboardPage());
    }

    private void OnMessagesClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MessageDoctorPage());
    }

    private void OnCalendaryClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CalendarPage());
    }

    private void OnProfileClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UserProfilePage());
    }
}

public class HeartbeatReading
{
    public int EcgValue { get; set; }
    public int Bpm { get; set; }

    public override string ToString()
    {
        return $"ECG: {EcgValue}, BPM: {Bpm}";
    }
}