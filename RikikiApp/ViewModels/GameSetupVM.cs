using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Services;
using RikikiApp.ViewModels.UiWrappers;
using RikikiApp.ViewModels.Popups;
using RikikiApp.Views;
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
    private bool _orderChanged = false;

    public string GameId { get; set; }

    private Game? _game;

    public ObservableCollection<GamePlayerItemVM   > Players { get; } = new();

    [ObservableProperty]
    private string title;

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
        RikikiGameEngine engine,
        NavigationService nav)
    {
        _games = games;
        _gamePlayers = gamePlayers;
        _engine = engine;
        _nav = nav;

        Players.CollectionChanged += Players_CollectionChanged;
    }

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
            Players.Add(new GamePlayerItemVM(p));

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

            players.Add(gp);
            await _gamePlayers.AddAsync(gp);
        }

        await LoadPlayers();
    }

    [RelayCommand]
    private async Task DeletePlayer(GamePlayerItemVM player)
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
            return;
        }
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

    [RelayCommand]
    private Task EndGame()
    {
        return Task.CompletedTask;
    }
}