using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Services;
using RikikiApp.Views;
using RikikiApp.Views.Popups;
using System.Collections.ObjectModel;

namespace RikikiApp.ViewModel;

public partial class GameViewVM : ObservableObject, IInitializable
{
    private readonly IGameRepository _games;
    private readonly NavigationService _nav;

    public GameViewVM(IGameRepository games, NavigationService nav)
    {
        _games = games;
        _nav = nav;
    }

    // 🔹 UI
    [ObservableProperty]
    private string gamesTitle = "Your games:";

    [ObservableProperty]
    private bool isSetupSelected = true;

    [ObservableProperty]
    private bool isInProgressSelected = true;

    [ObservableProperty]
    private bool isFinishedSelected = true;

    public ObservableCollection<Game> Games { get; } = new();

    private List<Game> _allGames = new();

    private HashSet<GameStatus> _selectedStatuses = new()
    {
        GameStatus.Setup,
        GameStatus.InProgress,
        GameStatus.Finished
    };

    // 🔹 INIT
    public async Task InitAsync()
    {
        await ReloadGamesAsync();
    }

    // 🔹 LOAD
    private async Task ReloadGamesAsync()
    {
        _allGames = (await _games.GetAllAsync()).ToList();

        GamesTitle = $"Your games: {_allGames.Count}";

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Games.Clear();

        var filtered = _allGames
            .Where(g => _selectedStatuses.Contains(g.Status));

        foreach (var g in filtered)
            Games.Add(g);
    }

    // 🔹 FILTER
    [RelayCommand]
    private void ToggleSetup()
    {
        Toggle(GameStatus.Setup);
    }

    [RelayCommand]
    private void ToggleInProgress()
    {
        Toggle(GameStatus.InProgress);
    }

    [RelayCommand]
    private void ToggleFinished()
    {
        Toggle(GameStatus.Finished);
    }

    private void Toggle(GameStatus status)
    {
        if (_selectedStatuses.Contains(status))
            _selectedStatuses.Remove(status);
        else
            _selectedStatuses.Add(status);

        IsSetupSelected = _selectedStatuses.Contains(GameStatus.Setup);
        IsInProgressSelected = _selectedStatuses.Contains(GameStatus.InProgress);
        IsFinishedSelected = _selectedStatuses.Contains(GameStatus.Finished);

        ApplyFilter();
    }

    // 🔹 NAV
    [RelayCommand]
    private async Task NewGame()
    {
        var game = await _nav.ShowPopupAsync<Game>(new AddGamePopup(_games));

        if (game is null)
            return;

        await ReloadGamesAsync();

        await _nav.Push<GameSetupView, GameSetupVM>(vm =>
        {
            vm.GameId = game.Id.ToString();
        });
    }

    [RelayCommand]
    private async Task OpenGame(int gameId)
    {
        await _nav.Push<GameSetupView, GameSetupVM>(vm =>
        {
            vm.GameId = gameId.ToString();
        });
    }
    [RelayCommand]
    private void Back()
    {
        _nav.Pop();
    }
}