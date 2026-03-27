using RikikiApp.Data;
using RikikiApp.Models;
using RikikiApp.Services;
using RikikiApp.ViewModel;

namespace RikikiApp.Views;

public partial class MainView : ContentView
{
    private readonly NavigationService _nav;
    private readonly LocalDB _localDb;

    public MainView(NavigationService nav, LocalDB localDb)
    {
        InitializeComponent();

        _nav = nav;
        _localDb = localDb;
    }

    private async void OpenGamesView(object sender, EventArgs e)
    {
       await _nav.PushWithLoading<GameView, GameViewVM>();
    }

    private async void OnDbTestClicked(object sender, EventArgs e)
    {
        var db = await _localDb.GetAsync();

        await db.InsertAsync(new Game
        {
            Name = "Test game",
            CreatedAt = DateTime.UtcNow,
            ScoringType = ScoringType.Basic
        });

        var count = await db.Table<Game>().CountAsync();

        await Application.Current.MainPage.DisplayAlertAsync("DB OK", $"Games in DB: {count}", "OK");
    }
}