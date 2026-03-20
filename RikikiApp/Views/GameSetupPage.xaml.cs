using CommunityToolkit.Maui.Extensions;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Views.Popups;
using RikikiApp.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RikikiApp.Views;

[QueryProperty(nameof(GameId), "gameId")]
public partial class GameSetupPage : ContentPage, INotifyPropertyChanged
{
    private readonly IGameRepository _games;
    private readonly IGamePlayerRepository _gamePlayers;
    private readonly RikikiGameEngine _engine;

    public string GameId { get; set; } = "";

    private Game? _game;

    private ObservableCollection<GamePlayer> _players = new();
    public ObservableCollection<GamePlayer> Players
    {
        get => _players;
        set
        {
            if (_players != null)
                _players.CollectionChanged -= Players_CollectionChanged;

            _players = value;

            if (_players != null)
                _players.CollectionChanged += Players_CollectionChanged;

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(IsNotEmpty));
        }
    }
    public bool IsEmpty => Players == null || Players.Count == 0;
    public bool IsNotEmpty => !IsEmpty;
    private bool _isPlayersExpanded;
    public bool IsPlayersExpanded
    {
        get => _isPlayersExpanded;
        set => SetProperty(ref _isPlayersExpanded, value);
    }

    public GameSetupPage()
    {
        InitializeComponent();

        var services = Application.Current!.Handler!.MauiContext!.Services;

        _games = services.GetRequiredService<IGameRepository>();
        _gamePlayers = services.GetRequiredService<IGamePlayerRepository>();
        _engine = services.GetRequiredService<RikikiGameEngine>();

        ScoringPicker.ItemsSource = new List<string>
        {
            "classic-v1",
            "classic-v2",
            "custom"
        };

        ScoringPicker.SelectedIndex = 0;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!int.TryParse(GameId, out var id))
        {
            await DisplayAlertAsync("Error", "Invalid game id.", "OK");
            return;
        }

        _game = await _games.GetByIdAsync(id);

        if (_game == null)
        {
            await DisplayAlertAsync("Error", "Game not found.", "OK");
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
        var ordered = players.OrderBy(x => x.SeatOrder).ToList();

        Players = new ObservableCollection<GamePlayer>(ordered);

        IsPlayersExpanded = Players.Count == 0;
    }

    // FIGYELI A LISTA VÁLTOZÁST
    private void Players_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(IsNotEmpty));

        if (Players.Count == 0)
            IsPlayersExpanded = true;
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

        var nextSeat = players
            .Select(p => p.SeatOrder)
            .DefaultIfEmpty(0)
            .Max() + 1;

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
            var ok = await DisplayAlertAsync(
                "Delete player?",
                $"Remove '{player.GuestName}'?",
                "Delete",
                "Cancel");

            if (!ok)
                return;

            await _gamePlayers.DeleteAsync(player.Id);

            var players = await _gamePlayers.GetByGameIdAsync(_game!.Id);

            var ordered = players.OrderBy(p => p.SeatOrder).ToList();

            int seat = 1;

            foreach (var p in ordered)
            {
                p.SeatOrder = seat++;
                await _gamePlayers.UpdateAsync(p);
            }

            await LoadPlayers();
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_game == null)
            return;

        var ok = await DisplayAlertAsync(
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

        await _engine.StartGame(_game.Id);

        await Shell.Current.GoToAsync($"{nameof(GamePlayPage)}?gameId={_game.Id}");
    }

    private async void OnEndGameClicked(object sender, EventArgs e)
    {
        if (_game == null)
            return;

        await DisplayAlertAsync("End", "Game ended.", "OK");
    }
    private async void OnShowStatsClicked(object sender, EventArgs e)
    {
        if (_game == null)
            return;

        var popup = new ShowStatsPopup(_game.Id);

        await this.ShowPopupAsync(popup);
    }
    // PropertyChanged helper
    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(name);
        return true;
    }
}