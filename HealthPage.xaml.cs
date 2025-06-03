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

            if (status.Contains("Conexiune pierdută") || status.Contains("Socket"))
            {
                _shouldRetry = true;
            }
        });

        Debug.WriteLine("Bluetooth Status: " + status);
    }


    private void OnDataReceived(object sender, string data)
    {

        Debug.WriteLine("Data received: " + data);


        var lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int ecgValue) &&
                int.TryParse(parts[1], out int bpmValue))
            {
                int? filteredBpm = FilterAndSmoothBpm(bpmValue);

                if (filteredBpm.HasValue)
                {
                    var reading = new HeartbeatReading
                    {
                        EcgValue = ecgValue,
                        Bpm = filteredBpm.Value
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
                        //DebugLogLabel.Text = displayLog;
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

            }

            if (line.Contains("Stopped", StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine("Mesaj de oprire detectat din Arduino: " + line);

                if (!_awaitingReset)
                {
                    _awaitingReset = true;

                    // Oprește animația dacă e pornită
                    _isBlinking = false;
                    _blinkTokenSource?.Cancel();

                    AfiseazaMediaBpm();

                    _ = MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        LastUploadLabel.Text = "ECG stopped. Sending data to cloud...";
                    });

                    _ = Task.Run(async () =>
                    {
                        await TrimiteDateLaApiAsync();

                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            LastUploadLabel.Text = $"Last data sent to cloud: {DateTime.Now:HH:mm:ss}";
                        });

                        Debug.WriteLine("Aștept 30 secunde pentru o posibilă reconectare...");
                        await Task.Delay(30000);

                        _shouldRetry = true;
                        Debug.WriteLine("Pregătit pentru reconectare dacă Arduino e resetat.");
                    });

                    return; // ieșim, nu mai procesăm nimic în acest pachet
                }
            }
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
                // Scale animat pe inimă
                await HeartIcon.ScaleTo(1.1, 250, Easing.CubicInOut);
                await HeartIcon.ScaleTo(1.0, 250, Easing.CubicInOut);
            });

            await Task.Delay(500, token); // sincron cu efectul
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

        // Rejecrez valori imposibile
        if (rawBpm < 40 || rawBpm > 180)
            return null;

        // Poți adăuga un filtru de tip "media ultimelor 3-5 valori"
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
            filteredReadings = heartbeatReadings
                .Where(r => r.Bpm >= 40 && r.Bpm <= 180 && r.EcgValue >= 600 && r.EcgValue <= 1023)
                .ToList();
        }

        string compressed = string.Join("|", filteredReadings
            .Select(r => $"BPM={r.Bpm};ECG={r.EcgValue}"));

        // Dacă vrei limită de 1000 caractere pentru SQL:
        if (compressed.Length > 1000)
        {
            compressed = compressed.Substring(0, 1000);
        }

        // Trimite la API
        int? userId = await SessionManager.GetLoggedInUserIdAsync();
        if (userId == null) return;

        var patient = await _httpClient.GetFromJsonAsync<Patient>($"api/patients/byUserId/{userId}");
        if (patient == null) return;

        var payload = new
        {
            patientId = patient.Id, // înlocuiește cu ID real
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

    private async void OnTrimiteDateClicked(object sender, EventArgs e)
    {
        await TrimiteDateLaApiAsync();
    }

    /*
    private async Task ListenContinuouslyAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _bluetoothService.EnsurePermissionsAsync(); // dacă e async

                _bluetoothService.ConnectAsync("98:D3:31:F7:44:C0"); // înlocuiește cu MAC-ul corect

                await _bluetoothService.StartListeningAsync(); // încearcă să citească

                // Dacă ai ieșit din listening înseamnă că s-a pierdut conexiunea
                _bluetoothService.StatusChanged += OnStatusChanged;
            }
            catch (Exception ex)
            {
                _bluetoothService.StatusChanged += OnStatusChanged;
            }

            await Task.Delay(5000, token); // Așteaptă 5 secunde între încercări
        }
    }
    */
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
                    _shouldRetry = !success; // doar dacă a mers, oprim retry-ul
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