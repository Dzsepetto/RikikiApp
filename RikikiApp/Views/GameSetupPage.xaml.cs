using Microsoft.Extensions.DependencyInjection;
using RikikiApp.Models;
using RikikiApp.Repositories;

namespace RikikiApp.Views;

[QueryProperty(nameof(GameId), "gameId")]
public partial class GameSetupPage : ContentPage
{
    private readonly IGameRepository _games;

    public string GameId { get; set; } = "";

    private Game? _game;

    public GameSetupPage()
    {
        InitializeComponent();

        var services = Application.Current!.Handler!.MauiContext!.Services;
        _games = services.GetRequiredService<IGameRepository>();

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
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnAddPlayerClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Add player", "Ide jön majd: player választás / létrehozás.", "OK");
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

}