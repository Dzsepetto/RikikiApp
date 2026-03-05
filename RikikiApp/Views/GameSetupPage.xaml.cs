using CommunityToolkit.Maui.Extensions;
using Microsoft.Extensions.DependencyInjection;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Views.Popups;

namespace RikikiApp.Views;

[QueryProperty(nameof(GameId), "gameId")]
public partial class GameSetupPage : ContentPage
{
    private readonly IGameRepository _games;
    private readonly IGamePlayerRepository _gamePlayers;

    public string GameId { get; set; } = "";

    private Game? _game;

    public GameSetupPage()
    {
        InitializeComponent();

        var services = Application.Current!.Handler!.MauiContext!.Services;

        _games = services.GetRequiredService<IGameRepository>();
        _gamePlayers = services.GetRequiredService<IGamePlayerRepository>();

        ScoringPicker.ItemsSource = new List<string>
        {
            "classic-v1",
            "classic-v2",
            "custom"
        };
        ScoringPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!int.TryParse(GameId, out var id))
        {
            await DisplayAlert("Error", "Invalid game id.", "OK");
            return;
        }

        _game = await _games.GetByIdAsync(id);
        if (_game == null)
        {
            await DisplayAlert("Error", "Game not found.", "OK");
            return;
        }

        TitleLabel.Text = _game.Name;

        var idx = ScoringPicker.ItemsSource
            .OfType<string>()
            .ToList()
            .FindIndex(x => x == _game.ScoringVersion);

        ScoringPicker.SelectedIndex = idx >= 0 ? idx : 0;

        await LoadPlayers();
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async Task LoadPlayers()
    {
        if (_game == null)
            return;

        var players = await _gamePlayers.GetByGameIdAsync(_game.Id);

        PlayersList.ItemsSource = players
            .OrderBy(x => x.SeatOrder)
            .ToList();
    }

    private async void OnAddPlayerClicked(object sender, EventArgs e)
    {
        if (_game == null)
            return;

        var players = await _gamePlayers.GetByGameIdAsync(_game.Id);

        var popup = new AddPlayerPopup(1);

        await this.ShowPopupAsync(popup);

        var result = popup.Result;

        if (result == null)
            return;

        var gp = new GamePlayer
        {
            GameId = _game.Id,
            SeatOrder = result.Seat,
            GuestName = result.GuestName
        };

        await _gamePlayers.AddAsync(gp);

        await LoadPlayers();
    }
    private async void OnStartGameClicked(object sender, EventArgs e)
    {
        if (_game == null)
            return;

        var selected = ScoringPicker.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(selected))
            selected = "classic-v1";

        _game.ScoringVersion = selected;
        await _games.UpsertAsync(_game);

        await DisplayAlert("Start", $"Game '{_game.Name}' started with '{_game.ScoringVersion}'.", "OK");

        // Következõ lépés: GamePlayPage-re nav (majd megcsináljuk)
        // await Shell.Current.GoToAsync($"{nameof(GamePlayPage)}?gameId={_game.Id}");
    }
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_game == null)
            return;

        var ok = await DisplayAlert(
            "Delete game?",
            $"Are you sure you want to delete '{_game.Name}'?",
            "Delete",
            "Cancel");

        if (!ok)
            return;

        await _games.DeleteAsync(_game.Id);

        await Shell.Current.GoToAsync("..");
    }

    private async void OnDeletePlayerClicked(object sender, EventArgs e)
    {
        if (sender is Button button &&
            button.CommandParameter is GamePlayer player)
        {
            var ok = await DisplayAlert(
                "Delete player?",
                $"Remove '{player.GuestName}' from the game?",
                "Delete",
                "Cancel");

            if (!ok)
                return;

            await _gamePlayers.DeleteAsync(player.Id);

            await LoadPlayers();
        }
    }
    private async void OnEndGameClicked(object sender, EventArgs e)
    {
        if (_game == null)
            return;
        var ok = await DisplayAlert(
            "End game?",
            $"Are you sure you want to end '{_game.Name}'? This cannot be undone.",
            "End",
            "Cancel");
    }

}