using CommunityToolkit.Maui.Extensions;
using Microsoft.Extensions.DependencyInjection;
using RikikiApp.Models;
using RikikiApp.Repositories;
using System.Diagnostics;
using RikikiApp.Views.Popups;

namespace RikikiApp.Views;

public partial class GamePage : ContentPage
{
    private readonly IGameRepository _games;

    // 🔹 Title
    private string _gamesTitle = "Your games:";
    public string GamesTitle
    {
        get => _gamesTitle;
        set
        {
            _gamesTitle = value;
            OnPropertyChanged();
        }
    }

    // 🔹 Összes játék (cache)
    private List<Game> _allGames = new();

    // 🔹 Multi filter (EZ A LÉNYEG)
    private HashSet<GameStatus> _selectedStatuses = new()
    {
        GameStatus.Setup,
        GameStatus.InProgress,
        GameStatus.Finished
    };

    public GamePage()
    {
        InitializeComponent();

        var services = Application.Current!.Handler!.MauiContext!.Services;
        _games = services.GetRequiredService<IGameRepository>();

        BindingContext = this;

        // kezdeti UI állapot
        Highlight(btnSetupFilter);
        Highlight(btnInProgressFilter);
        Highlight(btnFinishedFilter);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadGamesAsync();
    }

    // 🔹 Betöltés
    private async Task ReloadGamesAsync()
    {
        try
        {
            _allGames = (await _games.GetAllAsync()).ToList();

            GamesTitle = $"Your games: {_allGames.Count}";

            ApplyFilter();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ReloadGamesAsync error: " + ex);
            await DisplayAlertAsync("Error", "Could not load games from DB.", "OK");
        }
    }

    // 🔹 SZŰRÉS (multi!)
    private void ApplyFilter()
    {
        if (_selectedStatuses.Count == 0)
        {
            GamesList.ItemsSource = new List<Game>();
            return;
        }

        var filtered = _allGames
            .Where(g => _selectedStatuses.Contains(g.Status))
            .ToList();

        GamesList.ItemsSource = filtered;
    }

    // 🔹 TOGGLE LOGIKA
    private void ToggleStatus(GameStatus status, Button button)
    {
        if (_selectedStatuses.Contains(status))
        {
            _selectedStatuses.Remove(status);
            ResetButton(button);
        }
        else
        {
            _selectedStatuses.Add(status);
            Highlight(button);
        }

        ApplyFilter();
    }

    // 🔹 BUTTON CLICK-ek
    private void OnSetupFilterClicked(object sender, EventArgs e)
    {
        ToggleStatus(GameStatus.Setup, (Button)sender);
    }

    private void OnInProgressFilterClicked(object sender, EventArgs e)
    {
        ToggleStatus(GameStatus.InProgress, (Button)sender);
    }

    private void OnFinishedFilterClicked(object sender, EventArgs e)
    {
        ToggleStatus(GameStatus.Finished, (Button)sender);
    }

    // 🔹 UI helper
    private void Highlight(Button btn)
    {
        btn.BackgroundColor = Colors.Green;
        btn.TextColor = Colors.White;
    }

    private void ResetButton(Button btn)
    {
        btn.BackgroundColor = Colors.LightGray;
        btn.TextColor = Colors.Black;
    }

    // 🔹 NAV
    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnNewGameClicked(object sender, EventArgs e)
    {
        var popup = new AddGamePopup(_games);

        this.ShowPopup(popup);

        var result = await popup.Result.Task;

        if (result == null)
            return;

        await ReloadGamesAsync();

        await Shell.Current.GoToAsync($"{nameof(GameSetupPage)}?gameId={result.Id}");
    }

    private async void OnGameClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is null)
            return;

        var gameId = btn.CommandParameter.ToString();
        Debug.WriteLine($"Game clicked: {gameId}");

        await Shell.Current.GoToAsync($"{nameof(GameSetupPage)}?gameId={gameId}");
    }
}