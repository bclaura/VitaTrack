using Microsoft.Extensions.Logging;

namespace VitaTrack
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("LeagueSpartan-Regular.ttf", "LeagueSpartan");
                });
#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton(sp =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };

                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri("https://10.0.2.2:7203/")
                };

                return client;
            });

            return builder.Build();
        }
    }
}
