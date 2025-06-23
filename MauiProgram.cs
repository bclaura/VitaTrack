using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using System.Net.Http;
using VitaTrack.Platforms.Android;

namespace VitaTrack
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiMaps()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("LeagueSpartan-Regular.ttf", "LeagueSpartan");
                })
                .UseMicrocharts();




#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton(sp =>
            {
#if ANDROID
    var handler = new Xamarin.Android.Net.AndroidMessageHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    };
#else
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };
#endif

                return new HttpClient(handler)
                {
                    //BaseAddress = new Uri("http://192.168.100.16:5000/")
                    BaseAddress = new Uri("https://vitatrackapi.onrender.com/")

                };
            });

#if ANDROID
            
            builder.Services.AddSingleton<IBluetoothClassicService, BluetoothClassicService>();
#endif

            return builder.Build();
        }

    }
}
