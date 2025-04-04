using Android.Content.Res;
using Microsoft.Maui.Platform;

namespace VitaTrack
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new SplashPage());

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, View) =>
            {
                handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
            });

        }
    }
}
