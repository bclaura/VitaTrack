using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitaTrack
{
    public interface IBluetoothClassicService
    {
        event EventHandler<string> DataReceived;
        event EventHandler<string> StatusChanged;

        Task ConnectAsync(string macAddress);
        Task<bool> StartListeningAsync();
        Task EnsurePermissionsAsync();
        void StopListening();

        bool IsConnected { get; }


    }
}
