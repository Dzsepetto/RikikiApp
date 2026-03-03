using Microsoft.Extensions.DependencyInjection;
using RikikiApp.Models;
using RikikiApp.Repositories;
using System.Diagnostics;

namespace RikikiApp.Views;

public partial class GamePage : ContentPage
{
    private readonly IGameRepository _games;

    public GamePage()
    {
        InitializeComponent();

        var services = Application.Current!.Handler!.MauiContext!.Services;
        _games = services.GetRequiredService<IGameRepository>();
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnNewGameClicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync(
            title: "New game",
            message: "Add game name",
            accept: "Next",
            cancel: "Cancel",
            placeholder: "e.g. Friday night");

        if (string.IsNullOrWhiteSpace(name))
            return;

        var game = new Game
        {
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow,
            ScoringVersion = "classic-v1"
        };

        game = await _games.UpsertAsync(game);

        await Shell.Current.GoToAsync($"{nameof(GameSetupPage)}?gameId={game.Id}");
    }
    private async void OnGameClicked(object sender, EventArgs e)
    {
      Debug.WriteLine("Game clicked");
    }
}