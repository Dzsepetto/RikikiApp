using CommunityToolkit.Mvvm.ComponentModel;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class GameSetupVM : ObservableObject, IInitializable
{
    private readonly IGameRepository _games;
    private readonly IGamePlayerRepository _gamePlayers;
    private readonly RikikiGameEngine _engine;

    public string GameId { get; set; }

    private Game? _game;

    public ObservableCollection<GamePlayer> Players { get; } = new();

    [ObservableProperty]
    private string _playersTitle = "Players:";

    public bool IsEmpty => Players.Count == 0;
    public bool IsNotEmpty => !IsEmpty;

    [ObservableProperty]
    private bool _isPlayersExpanded;


    public GameSetupVM(
        IGameRepository games,
        IGamePlayerRepository gamePlayers,
        RikikiGameEngine engine)
    {
        _games = games;
        _gamePlayers = gamePlayers;
        _engine = engine;

        Players.CollectionChanged += Players_CollectionChanged;
    }

    // 🔹 INIT
    public async Task InitAsync()
    {
        if (!int.TryParse(GameId, out var id))
            return;

        _game = await _games.GetByIdAsync(id);

        if (_game == null)
            return;

        await LoadPlayers();
    }

    // 🔹 LOAD
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

    // 🔹 COLLECTION CHANGE
    private void Players_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(IsNotEmpty));

        if (Players.Count == 0)
            IsPlayersExpanded = true;
    }

    // 🔹 ADD PLAYER (UI fog popup-ot adni)
    public async Task AddPlayer(string name)
    {
        if (_game == null || string.IsNullOrWhiteSpace(name))
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

    // 🔹 DELETE PLAYER
    public async Task DeletePlayer(GamePlayer player)
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

    // 🔹 DELETE GAME
    public async Task DeleteGame()
    {
        if (_game == null)
            return;

        await _games.DeleteAsync(_game.Id);
    }

    // 🔹 START GAME
    public async Task StartGame()
    {
        if (_game == null)
            return;

        await _games.UpsertAsync(_game);
        await _engine.StartGame(_game.Id);
    }

    // 🔹 END GAME
    public Task EndGame()
    {
        // csak state/logika ide
        return Task.CompletedTask;
    }

    // 🔹 PROPERTY
    public event PropertyChangedEventHandler PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(name);
        return true;
    }

    void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}