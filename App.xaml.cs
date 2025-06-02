using Android.Content.Res;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Platform;

namespace VitaTrack
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            MainPage = new NavigationPage(new SplashPage());
            ServiceProvider = serviceProvider;

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, View) =>
            {
                handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
            });

        }


    }
}
