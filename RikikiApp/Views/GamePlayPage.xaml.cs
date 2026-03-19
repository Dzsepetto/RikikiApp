using Microsoft.Extensions.DependencyInjection;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Services;

namespace RikikiApp.Views;

[QueryProperty(nameof(GameId), "gameId")]
public partial class GamePlayPage : ContentPage
{
    private readonly IGameRepository _games;
    private readonly IGamePlayerRepository _players;
    private readonly IRoundRepository _rounds;
    private readonly ICallRepository _calls;
    private readonly RikikiGameEngine _engine;

    public string GameId { get; set; } = "";

    private Game? _game;
    private Round? _round;

    private List<CallView> _callViews = new();
    private List<ScoreView> _results = new();

    public GamePlayPage()
    {
        InitializeComponent();

        var services = Application.Current!.Handler!.MauiContext!.Services;

        _games = services.GetRequiredService<IGameRepository>();
        _players = services.GetRequiredService<IGamePlayerRepository>();
        _rounds = services.GetRequiredService<IRoundRepository>();
        _calls = services.GetRequiredService<ICallRepository>();
        _engine = services.GetRequiredService<RikikiGameEngine>();
    }

    private void ShowGameUI()
    {
        GameSection.IsVisible = true;
        ResultSection.IsVisible = false;

        GameButtons.IsVisible = true;
        ResultButtons.IsVisible = false;
    }

    private void ShowResultUI()
    {
        GameSection.IsVisible = false;
        ResultSection.IsVisible = true;

        GameButtons.IsVisible = false;
        ResultButtons.IsVisible = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!int.TryParse(GameId, out var id))
            return;

        _game = await _games.GetByIdAsync(id);

        if (_game == null)
            return;

        _round = (await _rounds.GetByGameIdAsync(_game.Id))
            .OrderBy(r => r.RoundIndex)
            .FirstOrDefault(r => !r.isCompleted);

        if (_round == null)
            return;

        RoundLabel.Text = $"Round {_round.RoundIndex} ({_round.HandSize} cards)";

        await LoadCalls();
        UpdateUI();

        ShowGameUI();
    }

    private async Task LoadCalls()
    {
        if (_round == null)
            return;

        var calls = await _calls.GetByRoundIdAsync(_round.Id);
        var players = await _players.GetByGameIdAsync(_round.GameId);

        var callDict = calls.ToDictionary(c => c.GamePlayerId);

        _callViews = players
            .Select(p =>
            {
                callDict.TryGetValue(p.Id, out var call);

                return new CallView
                {
                    CallId = call?.Id ?? 0,
                    GamePlayerId = p.Id,
                    PlayerName = p.GuestName,

                    Called = call?.Called,
                    Won = call?.Won,

                    IsCallEnabled = _round.State == RoundState.Calling,
                    IsWonEnabled = _round.State == RoundState.Playing,
                    IsWonVisible = _round.State == RoundState.Playing
                };
            })
            .ToList();

        CallsList.ItemsSource = _callViews;
    }

    private void UpdateUI()
    {
        if (_round == null)
            return;

        if (_round.State == RoundState.Calling)
        {
            FixCallsButton.IsVisible = true;
            EndRoundButton.IsVisible = false;
        }
        else if (_round.State == RoundState.Playing)
        {
            FixCallsButton.IsVisible = false;
            EndRoundButton.IsVisible = true;
        }
    }
    private async Task ShowResults(List<Call> calls)
    {
        var players = await _players.GetByGameIdAsync(_round!.GameId);

        var playerDict = players.ToDictionary(p => p.Id);

        _results = calls
            .Select(c =>
            {
                if (!playerDict.TryGetValue(c.GamePlayerId, out var player))
                {
                    Console.WriteLine($"Missing player for CallId: {c.Id}");
                    return null;
                }

                return new ScoreView
                {
                    PlayerName = player.GuestName,
                    Called = c.Called ?? 0,
                    Won = c.Won ?? 0,
                    Score = _engine.CalculateScore(c)
                };
            })
            .Where(x => x != null)
            .ToList();

        ResultsList.ItemsSource = _results;

        ShowResultUI();
    }

    private async Task CreateNextRound(int newHandSize)
    {
        if (newHandSize <= 0)
        {
            await DisplayAlert("Game finished", "No more rounds.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        var nextRound = await _engine.CreateNextRound(_round!.GameId, newHandSize);

        _round = nextRound;

        RoundLabel.Text = $"Round {_round.RoundIndex} ({_round.HandSize} cards)";

        ShowGameUI();

        await LoadCalls();
        UpdateUI();
    }
    private async void OnLessClicked(object sender, EventArgs e)
    {
        await CreateNextRound(_round!.HandSize - 1);
    }

    private async void OnSameClicked(object sender, EventArgs e)
    {
        await CreateNextRound(_round!.HandSize);
    }

    private async void OnMoreClicked(object sender, EventArgs e)
    {
        await CreateNextRound(_round!.HandSize + 1);
    }
    private async void OnFixCallsClicked(object sender, EventArgs e)
    {
        if (_round == null)
            return;

        if (_callViews.Any(c => !c.Called.HasValue))
        {
            await DisplayAlert("Error", "All players must enter a call.", "OK");
            return;
        }

        var calls = new List<Call>();

        foreach (var cv in _callViews)
        {
            calls.Add(new Call
            {
                Id = cv.CallId,
                RoundId = _round.Id,
                GamePlayerId = cv.GamePlayerId,
                Called = cv.Called
            });
        }

        await _engine.StartRound(calls);

        _round.State = RoundState.Playing;

        await LoadCalls();
        UpdateUI();
    }

    private async void OnEndRoundClicked(object sender, EventArgs e)
    {
        if (_round == null)
            return;

        if (_callViews.Any(c => !c.Won.HasValue))
        {
            await DisplayAlert("Error", "All players must enter won tricks.", "OK");
            return;
        }

        var calls = _callViews.Select(cv => new Call
        {
            Id = cv.CallId,
            RoundId = _round.Id,
            GamePlayerId = cv.GamePlayerId,
            Called = cv.Called,
            Won = cv.Won
        }).ToList();


        await _engine.EndRound(calls);


        await ShowResults(calls);
    }
    private async void OnEndGameClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Game ended", "Game has been manually ended.", "OK");
        await Shell.Current.GoToAsync("..");
    }
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}