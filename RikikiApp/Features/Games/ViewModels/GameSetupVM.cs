using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Core.Session;
using RikikiApp.Core.Abstractions;
using RikikiApp.Features.Games.Domain;
using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Features.Games.Views;
using RikikiApp.Features.Games.Views.Popups;
using RikikiApp.Features.Games.ViewModels.UiWrappers;
using RikikiApp.Features.Games.ViewModels.Popups;
using RikikiApp.Repositories.Interfaces;


namespace RikikiApp.Features.Games.ViewModels;

public partial class GameSetupVM : ObservableObject, IInitializable
{
    private readonly IGameRepository _games;
    private readonly IGamePlayerRepository _gamePlayers;
    private readonly IPlayerRepository _players;
    private readonly RikikiGameEngine _engine;
    private readonly NavigationService _nav;
    private readonly UserSessionService _session;

    private bool _orderChanged = false;

    public string GameId { get; set; } = string.Empty;

    private Game? _game;

    public ObservableCollection<GamePlayerItemVM> Players { get; } = new();

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string playersTitle = "Players:";

    [ObservableProperty]
    private bool isPlayersExpanded;

    [ObservableProperty]
    private bool isDragging;

    public bool IsEmpty => Players.Count == 0;
    public bool IsNotEmpty => !IsEmpty;

    public GameSetupVM(
        IGameRepository games,
        IGamePlayerRepository gamePlayers,
        IPlayerRepository players,
        RikikiGameEngine engine,
        NavigationService nav,
        UserSessionService session)
    {
        _games = games;
        _gamePlayers = gamePlayers;
        _players = players;
        _engine = engine;
        _nav = nav;
        _session = session;

        Players.CollectionChanged += Players_CollectionChanged;
    }

    public async Task InitAsync()
    {
        if (!int.TryParse(GameId, out var id))
            return;

        _game = await _games.GetByIdAsync(id);

        if (_game == null)
            return;

        Title = _game.Name;

        await EnsureLocalPlayerInGameAsync();
        await LoadPlayers();
    }

    private async Task EnsureLocalPlayerInGameAsync()
    {
        if (_game == null)
            return;

        var currentUser = _session.CurrentUser;
        if (currentUser == null)
            return;

        var localPlayer = await _players.GetByUserIdAsync(currentUser.Id);
        if (localPlayer == null)
            return;

        var gamePlayers = await _gamePlayers.GetByGameIdAsync(_game.Id);

        var alreadyInGame = gamePlayers.Any(gp => gp.PlayerId == localPlayer.Id);
        if (alreadyInGame)
            return;

        var nextSeat = gamePlayers
            .Select(gp => gp.SeatOrder)
            .DefaultIfEmpty(0)
            .Max() + 1;

        var gp = new GamePlayer
        {
            GameId = _game.Id,
            PlayerId = localPlayer.Id,
            GuestName = localPlayer.Name,
            SeatOrder = nextSeat
        };

        await _gamePlayers.AddAsync(gp);
    }

    private async Task LoadPlayers()
    {
        if (_game == null)
            return;

        Players.Clear();

        int? localPlayerId = null;
        var currentUser = _session.CurrentUser;

        if (currentUser != null)
        {
            var localPlayer = await _players.GetByUserIdAsync(currentUser.Id);
            localPlayerId = localPlayer?.Id;
        }

        var players = await _gamePlayers.GetByGameIdAsync(_game.Id);

        foreach (var p in players.OrderBy(x => x.SeatOrder))
            Players.Add(new GamePlayerItemVM(p, localPlayerId));

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

    public void MovePlayerToIndex(GamePlayerItemVM player, int newIndex)
    {
        var oldIndex = Players.IndexOf(player);

        if (oldIndex == -1)
            return;

        if (newIndex > Players.Count)
            newIndex = Players.Count;

        if (oldIndex < newIndex)
            newIndex--;

        if (oldIndex == newIndex)
            return;

        Players.Move(oldIndex, newIndex);

        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].SeatOrder = i + 1;
        }

        _orderChanged = true;
    }

    [RelayCommand]
    private async Task AddPlayer()
    {
        if (_game == null)
            return;

        var names = await _nav.ShowPopupAsync<AddPlayerPopup, AddPlayerPopupVM, List<string>>(
            async vm => await vm.InitAsync());

        if (names == null || !names.Any())
            return;

        var players = await _gamePlayers.GetByGameIdAsync(_game.Id);

        foreach (var name in names)
        {
            var trimmedName = name?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedName))
                continue;

            var nextSeat = players
                .Select(p => p.SeatOrder)
                .DefaultIfEmpty(0)
                .Max() + 1;

            var gp = new GamePlayer
            {
                GameId = _game.Id,
                SeatOrder = nextSeat,
                GuestName = trimmedName,
                PlayerId = null
            };

            players.Add(gp);
            await _gamePlayers.AddAsync(gp);
        }

        await LoadPlayers();
    }

    [RelayCommand]
    private async Task DeletePlayer(GamePlayerItemVM player)
    {
        if (_game == null || player == null)
            return;

        if (player.IsLocalPlayer)
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
    private async Task ShowStats()
    {
        if (_game == null)
            return;

        await _nav.ShowPopupAsync<ShowStatsPopup, ShowStatsPopupVM, object?>(
            vm => vm.InitAsync(_game.Id));
    }

    [RelayCommand]
    private async Task StartGame()
    {
        if (_game == null)
            return;

        if (_orderChanged)
        {
            foreach (var p in Players)
            {
                await _gamePlayers.UpdateAsync(p.Model);
            }

            _orderChanged = false;
        }

        await _games.UpsertAsync(_game);
        await _engine.StartGame(_game.Id);

        if (_game.Status != GameStatus.Setup)
        {
            await _nav.PushWithLoading<GamePlayView, GamePlayVM>(async vm =>
            {
                vm.GameId = _game.Id.ToString();
                await vm.InitAsync();
            });
        }
    }
    [RelayCommand]
    private async Task EndGame()
    {
        if (_game == null)
            return;
        await _engine.EndGame(_game.Id);

        await _nav.ShowPopupWithLoadingAsync<ShowStatsPopup, ShowStatsPopupVM, object?>(
            vm => vm.InitAsync(_game.Id));

        await _nav.Pop();
    }

    [RelayCommand]
    private async Task DeleteGame()
    {
        if (_game == null)
        {
            Debug.WriteLine("no game found");
            return;
        }

        await _games.DeleteAsync(_game.Id);
        await _nav.Pop();
    }

    [RelayCommand]
    private async Task Back()
    {
        if (_orderChanged)
        {
            foreach (var p in Players)
            {
                await _gamePlayers.UpdateAsync(p.Model);
            }
        }

        await _nav.Pop();
    }


}