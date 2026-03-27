using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Services;
using System.Collections.ObjectModel;

public partial class GamePlayVM : ObservableObject, IInitializable
{
    private readonly IGameRepository _games;
    private readonly IGamePlayerRepository _players;
    private readonly IRoundRepository _rounds;
    private readonly ICallRepository _calls;
    private readonly RikikiGameEngine _engine;
    private readonly NavigationService _nav;

    public string GameId { get; set; }

    private Game? _game;
    private Round? _round;

    public ObservableCollection<CallView> Calls { get; } = new();
    public ObservableCollection<ScoreView> Results { get; } = new();

    [ObservableProperty]
    private string roundText;

    [ObservableProperty]
    private bool isGameVisible;

    [ObservableProperty]
    private bool isResultVisible;

    [ObservableProperty]
    private bool isFixCallsVisible;

    [ObservableProperty]
    private bool isEndRoundVisible;

    public GamePlayVM(
        IGameRepository games,
        IGamePlayerRepository players,
        IRoundRepository rounds,
        ICallRepository calls,
        RikikiGameEngine engine,
        NavigationService nav)
    {
        _games = games;
        _players = players;
        _rounds = rounds;
        _calls = calls;
        _engine = engine;
        _nav = nav;
    }

    public async Task InitAsync()
    {
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

        RoundText = $"Round {_round.RoundIndex} ({_round.HandSize} cards)";

        await LoadCalls();
        UpdateUI();
    }

    private async Task LoadCalls()
    {
        if (_round == null)
            return;

        Calls.Clear();

        var calls = await _calls.GetByRoundIdAsync(_round.Id);
        var players = await _players.GetByGameIdAsync(_round.GameId);

        var dict = calls.ToDictionary(c => c.GamePlayerId);

        foreach (var p in players)
        {
            dict.TryGetValue(p.Id, out var call);

            Calls.Add(new CallView
            {
                CallId = call?.Id ?? 0,
                GamePlayerId = p.Id,
                PlayerName = p.GuestName,
                Called = call?.Called ?? 0,
                Won = call?.Won ?? 0,
                MaxValue = _round.HandSize,

                IsCallEnabled = _round.State == RoundState.Calling,
                IsWonEnabled = _round.State == RoundState.Playing,
                IsWonVisible = _round.State == RoundState.Playing
            });
        }
    }

    private void UpdateUI()
    {
        if (_round == null)
            return;

        IsGameVisible = true;
        IsResultVisible = false;

        IsFixCallsVisible = _round.State == RoundState.Calling;
        IsEndRoundVisible = _round.State == RoundState.Playing;
    }

    private void ShowResultsUI()
    {
        IsGameVisible = false;
        IsResultVisible = true;
    }

    // 🔥 COMMANDOK

    [RelayCommand]
    private async Task FixCalls()
    {
        if (_round == null)
            return;

        var calls = Calls.Select(c => new Call
        {
            Id = c.CallId,
            RoundId = _round.Id,
            GamePlayerId = c.GamePlayerId,
            Called = (int)c.Called
        }).ToList();

        await _engine.StartRound(calls);

        _round.State = RoundState.Playing;

        await LoadCalls();
        UpdateUI();
    }

    [RelayCommand]
    private async Task EndRound()
    {
        if (_round == null)
            return;

        var calls = Calls.Select(c => new Call
        {
            Id = c.CallId,
            RoundId = _round.Id,
            GamePlayerId = c.GamePlayerId,
            Called = (int)c.Called,
            Won = (int)c.Won
        }).ToList();

        await _engine.EndRound(calls);

        await LoadResults(calls);
    }

    private async Task LoadResults(List<Call> calls)
    {
        Results.Clear();

        var players = await _players.GetByGameIdAsync(_round!.GameId);
        var dict = players.ToDictionary(p => p.Id);

        foreach (var c in calls)
        {
            if (!dict.TryGetValue(c.GamePlayerId, out var player))
                continue;

            Results.Add(new ScoreView
            {
                PlayerName = player.GuestName,
                Called = c.Called ?? 0,
                Won = c.Won ?? 0,
                Score = _engine.CalculateScore(c)
            });
        }

        ShowResultsUI();
    }

    [RelayCommand]
    private async Task Less() => await CreateNextRound(_round!.HandSize - 1);

    [RelayCommand]
    private async Task Same() => await CreateNextRound(_round!.HandSize);

    [RelayCommand]
    private async Task More() => await CreateNextRound(_round!.HandSize + 1);

    private async Task CreateNextRound(int size)
    {
        if (size <= 0)
        {
            await _nav.Pop();
            return;
        }

        _round = await _engine.CreateNextRound(_round!.GameId, size);

        RoundText = $"Round {_round.RoundIndex} ({_round.HandSize} cards)";

        await LoadCalls();
        UpdateUI();
    }

    [RelayCommand]
    private async Task Back()
    {
        await _nav.Pop();
    }

    [RelayCommand]
    private async Task EndGame()
    {
        await _nav.Pop();
    }
}