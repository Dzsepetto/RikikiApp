using Microsoft.Maui.ApplicationModel;
using RikikiApp.Core.Session;
using RikikiApp.Infrastructure.Persistance;

using RikikiApp.Repositories;

using System.Diagnostics;
using System.Globalization;

namespace RikikiApp { 

public partial class App : Application
{
    private readonly LocalDB _localDb;
        private readonly IPlayerRepository _players;
        private readonly IUserRepository _user;
        private readonly UserSessionService _session;
    public App(LocalDB localDb, IPlayerRepository players, IUserRepository user, UserSessionService session)
    {
        InitializeComponent();
        _localDb = localDb;
        _players = players;
        _user = user;
        _session = session;

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Debug.WriteLine($"🔥 UNHANDLED: {e.ExceptionObject}");
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Debug.WriteLine($"🔥 TASK ERROR: {e.Exception}");
        };

    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var mainPage = Handler.MauiContext.Services.GetRequiredService<MainLayoutPage>();

        var window = new Window(mainPage);

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await _localDb.InitAsync();
                Debug.WriteLine("✅ DB init OK");

                await EnsureLocalUserAndPlayerAsync();
                Debug.WriteLine("✅ Local user + player ensured");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ DB init ERROR:\n" + ex);
            }
        });

        return window;
    }
        private async Task EnsureLocalUserAndPlayerAsync()
        {
            var localUser = await _user.GetLocalUserAsync();

            if (localUser == null)
            {
                localUser = new User
                {
                    Name = "Player",
                    Surname = "",
                    Email = null,
                    ProfilePicture = "",
                    IsLocalUser = true,
                    IsGuest = true
                };

                await _user.AddAsync(localUser);
                Debug.WriteLine($"✅ Local user created: {localUser.Name} (Id: {localUser.Id})");
            }
            else
            {
                Debug.WriteLine($"ℹ️ Local user already exists: {localUser.Name} (Id: {localUser.Id})");
            }

            var localPlayer = await _players.GetByUserIdAsync(localUser.Id);

            if (localPlayer == null)
            {
                localPlayer = new Player
                {
                    Name = localUser.Name,
                    UserId = localUser.Id
                };

                await _players.AddAsync(localPlayer);
                Debug.WriteLine($"✅ Local player created: {localPlayer.Name} (UserId: {localPlayer.UserId})");
            }
            else
            {
                Debug.WriteLine($"ℹ️ Local player already exists: {localPlayer.Name} (Id: {localPlayer.Id})");
            }

            _session.SetCurrentUser(localUser);
        }
    }

}