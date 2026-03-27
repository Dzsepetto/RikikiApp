using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using RikikiApp.Data;
using RikikiApp.Repositories;
using RikikiApp.Services;
using RikikiApp.ViewModel;
using RikikiApp.ViewModel.Popups;
using RikikiApp.ViewModels;
using RikikiApp.Views;
using RikikiApp.Views.Popups;

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
            builder.Services.AddSingleton<IGamePlayerRepository, SqliteGamePlayerRepository>();
            builder.Services.AddSingleton<IRoundRepository, SQLiteRoundRepository>();
            builder.Services.AddSingleton<ICallRepository, SQLiteCallRepository>();
            builder.Services.AddSingleton<IPlayerRepository, SQLitePlayerRepository>();

            //services
            builder.Services.AddSingleton<RikikiGameEngine>();
            builder.Services.AddSingleton<NavigationService>();

            //page
            builder.Services.AddSingleton<MainLayoutPage>();

            // views
            builder.Services.AddTransient<GameViewVM>();
            builder.Services.AddTransient<GameView>();

            builder.Services.AddTransient<GameSetupVM>();
            builder.Services.AddTransient<GameSetupView>();

            builder.Services.AddTransient<GamePlayVM>();
            builder.Services.AddTransient<GamePlayView>();

            builder.Services.AddTransient<ProfileViewVM>();
            builder.Services.AddTransient<ProfileView>();

            builder.Services.AddTransient<MainView>();

            builder.Services.AddTransient<StatsView>();

            builder.Services.AddTransient<LoadingPopup>();

            //popus
            builder.Services.AddTransient<AddGamePopup>();
            builder.Services.AddTransient<AddGamePopupVM>();

            builder.Services.AddTransient<AddPlayerPopup>();
            builder.Services.AddTransient<AddPlayerPopupVM>();

            //==== ======= ====


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
