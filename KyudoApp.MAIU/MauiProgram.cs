using KyudoApp.MAIU.Views;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Syncfusion.Maui.Core.Hosting;
namespace KyudoApp.MAIU
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                /*.UseLocalNotification()*/
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<SqliConnection>();
            
#if DEBUG
    		
#endif

            return builder.Build();
        }
    }
}
