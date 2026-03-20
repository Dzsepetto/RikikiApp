using CommunityToolkit.Maui.Views;
using RikikiApp.Models;
using RikikiApp.Repositories;
using System.Diagnostics;

namespace RikikiApp.Views.Popups;

public partial class AddGamePopup : Popup
{
    private readonly IGameRepository _games;
    public TaskCompletionSource<Game?> Result { get; } = new();

    public AddGamePopup(IGameRepository games)
    {
        InitializeComponent();
        _games = games;
    }

    async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GameEntry.Text))
        {
            await Application.Current.MainPage.DisplayAlertAsync("Error", "Name required", "OK");
            return;
        }

        var game = new Game
        {
            Name = GameEntry.Text.Trim(),
            CreatedAt = DateTime.UtcNow,
            ScoringVersion = "classic-v1",
            Status = GameStatus.Setup
        };

        game = await _games.UpsertAsync(game);

        Result.SetResult(game);

        await CloseAsync();
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        Result.SetResult(null);
        await CloseAsync();
    }
}