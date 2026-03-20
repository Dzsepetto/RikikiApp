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

    public GamePage()
    {
        InitializeComponent();

        var services = Application.Current!.Handler!.MauiContext!.Services;
        _games = services.GetRequiredService<IGameRepository>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadGamesAsync();
    }

    private async Task ReloadGamesAsync()
    {
        try
        {
            var list = await _games.GetAllAsync();
            GamesList.ItemsSource = list;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ReloadGamesAsync error: " + ex);
            await DisplayAlertAsync("Error", "Could not load games from DB.", "OK");
        }
    }

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