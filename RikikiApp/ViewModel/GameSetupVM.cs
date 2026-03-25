using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Services;
using RikikiApp.Views.Popups;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
public partial class GameSetupVM : ObservableObject, IInitializable
{
    private readonly IGameRepository _games;
    private readonly IGamePlayerRepository _gamePlayers;
    private readonly RikikiGameEngine _engine;
    private readonly NavigationService _nav;

    public string GameId { get; set; }

    private Game? _game;

    public ObservableCollection<GamePlayer> Players { get; } = new();

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string playersTitle = "Players:";

    [ObservableProperty]
    private bool isPlayersExpanded;

    public bool IsEmpty => Players.Count == 0;
    public bool IsNotEmpty => !IsEmpty;

    public GameSetupVM(
        IGameRepository games,
        IGamePlayerRepository gamePlayers,
        RikikiGameEngine engine,
        NavigationService nav)
    {
        _games = games;
        _gamePlayers = gamePlayers;
        _engine = engine;
        _nav = nav;

        Players.CollectionChanged += Players_CollectionChanged;
    }

    // ✅ EZ FUT LE NAV UTÁN
    public async Task InitAsync()
    {
        if (!int.TryParse(GameId, out var id))
            return;

        _game = await _games.GetByIdAsync(id);

        Title = _game.Name;

        if (_game == null)
            return;

        await LoadPlayers();
    }

    private async Task LoadPlayers()
    {
        if (_game == null)
            return;

        Players.Clear();

        var players = await _gamePlayers.GetByGameIdAsync(_game.Id);

        foreach (var p in players.OrderBy(x => x.SeatOrder))
            Players.Add(p);

        PlayersTitle = $"Players ({Players.Count})";
        IsPlayersExpanded = Players.Count == 0;

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(IsNotEmpty));
    }

    private void Players_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(IsNotEmpty));

        if (Players.Count == 0)
            IsPlayersExpanded = true;
    }

    [RelayCommand]
    private async Task AddPlayer()
    {
        if (_game == null)
            return;

        var name = await _nav.ShowPopupAsync<string>(new AddPlayerPopup());

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

    [RelayCommand]
    private async Task DeletePlayer(GamePlayer player)
    {
        if (_game == null)
            return;

        await _gamePlayers.DeleteAsync(player.Id);

        var players = await _gamePlayers.GetByGameIdAsync(_game.Id);

        var ordered = players.OrderBy(p => p.SeatOrder).ToList();

        int seat = 1;

        foreach (var p in ordered)
        {
            p.SeatOrder = seat++;
            await _gamePlayers.UpdateAsync(p);
        }

        await LoadPlayers();
    }

    [RelayCommand]
    private async Task StartGame()
    {
        if (_game == null)
            return;

        await _games.UpsertAsync(_game);
        await _engine.StartGame(_game.Id);
    }

    [RelayCommand]
    private async Task DeleteGame()
    {
        if (_game == null) {
            Debug.WriteLine("no game found");
            return;
        }
        await _games.DeleteAsync(_game.Id);

       await _nav.Pop();
    }

    [RelayCommand]
    private async void Back()
    {
       await _nav.Pop();
    }

    [RelayCommand]
    private Task EndGame()
    {
        return Task.CompletedTask;
    }
}