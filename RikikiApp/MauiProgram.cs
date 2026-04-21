using CommunityToolkit.Maui;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;

using RikikiApp.Infrastructure.Persistance;
using RikikiApp.Repositories;

using RikikiApp.Core.Session;
using RikikiApp.Features.Games.Domain;

using RikikiApp.Features.Main.Views;
using RikikiApp.Features.Main.ViewModels;
using RikikiApp.Features.Games.Views;
using RikikiApp.Features.Games.ViewModels;
using RikikiApp.Features.Games.ViewModels.Popups;
using RikikiApp.Features.Games.Views.Popups;
using RikikiApp.Features.Profile.Views;
using RikikiApp.Features.Profile.ViewModels;
using RikikiApp.Features.Profile.Views.Components;
using RikikiApp.Features.Profile.ViewModels.Components;
using RikikiApp.Features.Stats.Views;
using RikikiApp.Features.Stats.ViewModels;

using RikikiApp.Features.PopupsGeneral.Views;

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
                .UseMicrocharts()
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
            builder.Services.AddSingleton<IUserRepository, SQLiteUserRepository>();

            //services
            builder.Services.AddSingleton<RikikiGameEngine>();
            builder.Services.AddSingleton<NavigationService>();
            builder.Services.AddSingleton<UserSessionService>();

            //page
            builder.Services.AddSingleton<MainLayoutPage>();

            // views
            builder.Services.AddTransient<GameViewVM>();
            builder.Services.AddTransient<GameView>();

            builder.Services.AddTransient<GameSetupVM>();
            builder.Services.AddTransient<GameSetupView>();

            builder.Services.AddTransient<GamePlayVM>();
            builder.Services.AddTransient<GamePlayView>();
            builder.Services.AddTransient<CallViewVM>();

            builder.Services.AddTransient<ProfileViewVM>();
            builder.Services.AddTransient<ProfileView>();
            builder.Services.AddTransient<ManagePlayersView>();
            builder.Services.AddTransient<ManagePlayersVM>();

            builder.Services.AddTransient<MainView>();
            builder.Services.AddTransient<MainViewVM>();

            builder.Services.AddTransient<StatsView>();
            builder.Services.AddTransient<StatsViewVM>();

            builder.Services.AddTransient<LoadingPopup>();

            //popus
            builder.Services.AddTransient<AddGamePopup>();
            builder.Services.AddTransient<AddGamePopupVM>();

            builder.Services.AddTransient<AddPlayerPopup>();
            builder.Services.AddTransient<AddPlayerPopupVM>();

            builder.Services.AddTransient<ShowStatsPopup>();
            builder.Services.AddTransient<ShowStatsPopupVM>();

            //==== ======= ====


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
