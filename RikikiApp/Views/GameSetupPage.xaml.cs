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

    private async Task LoadPlayers()
    {
        if (_game == null)
            return;

        var players = await _gamePlayers.GetByGameIdAsync(_game.Id);

        PlayersList.ItemsSource = players
            .OrderBy(x => x.SeatOrder)
            .ToList();
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnAddPlayerClicked(object sender, EventArgs e)
    {
        if (_game == null)
            return;

        var popup = new AddPlayerPopup();

        await this.ShowPopupAsync(popup);

        var name = popup.Result;

        if (string.IsNullOrWhiteSpace(name))
            return;

        var players = await _gamePlayers.GetByGameIdAsync(_game.Id);

        var nextSeat = players.Any()
        ? players.Max(p => p.SeatOrder) + 1
        : 1;

        var gp = new GamePlayer
        {
            GameId = _game.Id,
            SeatOrder = nextSeat,
            GuestName = name
        };

        await _gamePlayers.AddAsync(gp);

        await LoadPlayers();
    }

    private async void OnDeletePlayerClicked(object sender, EventArgs e)
    {
        if (sender is Button button &&
            button.CommandParameter is GamePlayer player)
        {
            var ok = await DisplayAlert(
                "Delete player?",
                $"Remove '{player.GuestName}'?",
                "Delete",
                "Cancel");

            if (!ok)
                return;

            var players = await _gamePlayers.GetByGameIdAsync(_game!.Id);

            await _gamePlayers.DeleteAsync(player.Id);

            foreach (var p in players.Where(x => x.SeatOrder > player.SeatOrder))
            {
                p.SeatOrder--;
                await _gamePlayers.UpdateAsync(p);
            }

            await LoadPlayers();
        }
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

    private async void OnStartGameClicked(object sender, EventArgs e)
    {
        if (_game == null)
            return;

        var selected = ScoringPicker.SelectedItem as string;

        if (string.IsNullOrWhiteSpace(selected))
            selected = "classic-v1";

        _game.ScoringVersion = selected;

        await _games.UpsertAsync(_game);

        await DisplayAlert("Start",
            $"Game '{_game.Name}' started with '{_game.ScoringVersion}'.",
            "OK");
    }

    private async void OnEndGameClicked(object sender, EventArgs e)
    {
        if (_game == null)
            return;

        await DisplayAlert("End", "Game ended.", "OK");
    }

    void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        if (sender is Label label && label.BindingContext is GamePlayer player)
        {
            e.Data.Properties.Add("player", player);
        }
    }

    async void OnDrop(object sender, DropEventArgs e)
    {
        if (_game == null)
            return;

        if (!e.Data.Properties.ContainsKey("player"))
            return;

        var draggedPlayer = e.Data.Properties["player"] as GamePlayer;

        if (draggedPlayer == null)
            return;

        if (sender is not Grid grid || grid.BindingContext is not GamePlayer targetPlayer)
            return;

        var players = await _gamePlayers.GetByGameIdAsync(_game.Id);

        int oldSeat = draggedPlayer.SeatOrder;
        int newSeat = targetPlayer.SeatOrder;

        if (oldSeat == newSeat)
            return;

        if (oldSeat < newSeat)
        {
            foreach (var p in players.Where(p =>
                     p.SeatOrder > oldSeat &&
                     p.SeatOrder <= newSeat))
            {
                p.SeatOrder--;
                await _gamePlayers.UpdateAsync(p);
            }
        }
        else
        {
            foreach (var p in players.Where(p =>
                     p.SeatOrder < oldSeat &&
                     p.SeatOrder >= newSeat))
            {
                p.SeatOrder++;
                await _gamePlayers.UpdateAsync(p);
            }
        }

        draggedPlayer.SeatOrder = newSeat;
        await _gamePlayers.UpdateAsync(draggedPlayer);

        await LoadPlayers();
    }
}