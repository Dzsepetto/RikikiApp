using CommunityToolkit.Maui;
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
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
                

//==== services ====

            builder.Services.AddSingleton<App>();
            builder.Services.AddSingleton<LocalDB>();
            builder.Services.AddSingleton<IGameRepository, SqliteGameRepository>();

            builder.Services.AddTransient<GamePage>();
            builder.Services.AddTransient<GameSetupPage>();

            builder.Services.AddSingleton<IGamePlayerRepository, SqliteGamePlayerRepository>();
//==== ======= ====



#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
