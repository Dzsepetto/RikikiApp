using Microsoft.Extensions.Logging;
using RikikiApp.Data;
using RikikiApp.Repositories;
using RikikiApp.Views;

namespace RikikiApp
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
                });
            builder.Services.AddSingleton<App>();
            builder.Services.AddSingleton<LocalDB>();
            builder.Services.AddSingleton<IGameRepository, SqliteGameRepository>();

            builder.Services.AddTransient<GamePage>();
            builder.Services.AddTransient<GameSetupPage>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
