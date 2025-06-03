using Android.Bluetooth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitaTrack.Platforms.Android;
using Android;
using Android.Content.PM;
using Android.OS;
using Android.App;
using Microsoft.Maui.ApplicationModel;

[assembly: Dependency(typeof(BluetoothClassicService))]
namespace VitaTrack.Platforms.Android
{
    public class BluetoothClassicService : Java.Lang.Thread, IBluetoothClassicService
    {
        public event EventHandler<string> DataReceived;
        public event EventHandler<string> StatusChanged;

        private bool isAttemptingReconnect = false;
        private System.Timers.Timer reconnectTimer;

        private BluetoothSocket _socket;
        private StreamReader _reader;
        private bool _isListening;

        public bool IsConnected => _socket?.IsConnected == true;


        public async Task ConnectAsync(string macAddress)
        {
            var adapter = BluetoothAdapter.DefaultAdapter;

            if (adapter == null || !adapter.IsEnabled)
            {
                StatusChanged?.Invoke(this, "Bluetooth indisponibil.");
                return;
            }

            await EnsurePermissionsAsync(); // așteptăm acordul

            var device = adapter.BondedDevices.FirstOrDefault(d => d.Name == "HC-05");

            if (device == null)
            {
                StatusChanged?.Invoke(this, "HC-05 nu este paired. Asociază-l manual din Bluetooth Settings.");
                return;
            }

            if (_socket != null)
            {
                try
                {
                    _socket.Close();
                }
                catch { }

                _socket = null;
            }

            try
            {
                _socket = device.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));

                adapter.CancelDiscovery(); // trebuie făcut *după* pairing dar *înainte* connect

                await Task.Run(() => _socket.Connect()); // poate necesita extensie dacă nu există metoda async

                StatusChanged?.Invoke(this, "Connected to HC-05");

                await StartListeningAsync(); // refactorizăm și această metodă
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Eroare la conectare: {ex.Message}");
            }
        }

        /*public void StartListening()
        {
            _ = EnsurePermissionsAsync();
            var activity = Platform.CurrentActivity;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                if (activity.CheckSelfPermission(Manifest.Permission.BluetoothScan) != Permission.Granted ||
                    activity.CheckSelfPermission(Manifest.Permission.BluetoothConnect) != Permission.Granted)
                {
                    activity.RequestPermissions(new string[]
                    {
                Manifest.Permission.BluetoothScan,
                Manifest.Permission.BluetoothConnect,
                Manifest.Permission.AccessFineLocation // uneori necesar pt. pairing/discovery
                    }, 1001);

                    return;
                }
            }
            else
            {
                if (activity.CheckSelfPermission(Manifest.Permission.Bluetooth) != Permission.Granted ||
                    activity.CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    activity.RequestPermissions(new string[]
                    {
                Manifest.Permission.Bluetooth,
                Manifest.Permission.AccessFineLocation
                    }, 1002);

                    return;
                }
            }

            // Dacă avem permisiuni, continuăm
            _isListening = true;
            _reader = new StreamReader(_socket.InputStream, Encoding.ASCII);

            new Thread(() =>
            {
                while (_isListening)
                {
                    try
                    {
                        var line = _reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            DataReceived?.Invoke(this, line);
                        }
                    }
                    catch (Exception ex)
                    {
                        StatusChanged?.Invoke(this, $"Eroare la citire: {ex.Message}");
                        _isListening = false;
                    }
                }
            }).Start();
        }*/

        public async Task<bool> StartListeningAsync()
        {
            await EnsurePermissionsAsync();

            if (_socket == null || !_socket.IsConnected)
            {
                StatusChanged?.Invoke(this, "Socket invalid sau deconectat");
                return false;
            }

            _isListening = true;
            _reader = new StreamReader(_socket.InputStream, Encoding.ASCII);

            new Thread(() =>
            {
                while (_isListening)
                {
                    try
                    {
                        var line = _reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            DataReceived?.Invoke(this, line);
                        }
                    }
                    catch (Exception ex)
                    {
                        _isListening = false;
                        StatusChanged?.Invoke(this, $"Conexiune pierdută: {ex.Message}");
                    }
                }
            }).Start();

            return true;
        }





        public async Task EnsurePermissionsAsync()
        {
            var activity = Platform.CurrentActivity;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                var permissionsToRequest = new List<string>();

                if (activity.CheckSelfPermission(Manifest.Permission.BluetoothScan) != Permission.Granted)
                    permissionsToRequest.Add(Manifest.Permission.BluetoothScan);

                if (activity.CheckSelfPermission(Manifest.Permission.BluetoothConnect) != Permission.Granted)
                    permissionsToRequest.Add(Manifest.Permission.BluetoothConnect);

                if (activity.CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
                    permissionsToRequest.Add(Manifest.Permission.AccessFineLocation);

                if (permissionsToRequest.Count > 0)
                {
                    activity.RequestPermissions(permissionsToRequest.ToArray(), 1001);
                    await Task.Delay(1000); // Aștepți un pic până utilizatorul răspunde
                }


            }
            else
            {
                // Pentru Android sub 12
                if (activity.CheckSelfPermission(Manifest.Permission.Bluetooth) != Permission.Granted ||
                    activity.CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    activity.RequestPermissions(new string[]
                    {
                Manifest.Permission.Bluetooth,
                Manifest.Permission.AccessFineLocation
                    }, 1002);

                }

            }
        }


        public void StopListening()
        {
            _isListening = false;
            _reader?.Dispose();
            _socket?.Close();

            reconnectTimer?.Stop();
            reconnectTimer?.Dispose();
            reconnectTimer = null;
        }
    }
}