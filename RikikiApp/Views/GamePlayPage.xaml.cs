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
    }

    private async Task LoadCalls()
    {
        if (_round == null)
            return;

        var calls = await _calls.GetByRoundIdAsync(_round.Id);
        var players = await _players.GetByGameIdAsync(_round.GameId);

        _callViews = calls.Select(c =>
        {
            var player = players.First(p => p.Id == c.GamePlayerId);

            return new CallView
            {
                CallId = c.Id,
                GamePlayerId = c.GamePlayerId,
                PlayerName = player.GuestName,
                Called = c.Called,
                Won = c.Won,

                IsCallEnabled = _round.State == RoundState.Calling,
                IsWonEnabled = _round.State == RoundState.Playing,
                IsWonVisible = _round.State == RoundState.Playing
            };

        }).ToList();

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

        var calls = new List<Call>();

        foreach (var cv in _callViews)
        {
            calls.Add(new Call
            {
                Id = cv.CallId,
                RoundId = _round.Id,
                GamePlayerId = cv.GamePlayerId,
                Called = cv.Called,
                Won = cv.Won
            });
        }

        var nextRound = await _engine.EndRound(calls);

        if (nextRound == null)
        {
            await DisplayAlert("Game finished", "No more rounds.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        _round = nextRound;

        RoundLabel.Text = $"Round {_round.RoundIndex} ({_round.HandSize} cards)";

        await LoadCalls();
        UpdateUI();
    }
}