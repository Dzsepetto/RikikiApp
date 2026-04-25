using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Core.Abstractions;
using RikikiApp.Features.Games.Domain;
using RikikiApp.Features.Games.ViewModels.DTOs;
using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Repositories.Interfaces;
using RikikiApp.Features.Games.Domain.Scoring.Service;

namespace RikikiApp.Features.Games.ViewModels;

public partial class GamePlayVM : ObservableObject, IInitializable
{
    private readonly IGameRepository _games;
    private readonly IGamePlayerRepository _players;
    private readonly IRoundRepository _rounds;
    private readonly ICallRepository _calls;
    private readonly IScoringService _scoringService;
    private readonly RikikiGameEngine _engine;
    private readonly NavigationService _nav;

    public string GameId { get; set; }

    private Game? _game;
    private Round? _round;

    public ObservableCollection<CallViewVM> Calls { get; } = new();
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
        IScoringService scoringService,
        RikikiGameEngine engine,
        NavigationService nav)
    {
        _games = games;
        _players = players;
        _rounds = rounds;
        _calls = calls;
        _scoringService = scoringService;
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
            .Where(r => r.State != RoundState.Finished)
            .OrderByDescending(r => r.RoundIndex)
            .FirstOrDefault();

        if (_round == null)
            return;

        RoundText = $"Round {_round.RoundIndex} ({_round.HandSize} cards)";

        await LoadCalls();

        if (_round.State == RoundState.WaitingForNextRound)
        {
            var calls = await _calls.GetByRoundIdAsync(_round.Id);
            await LoadResults(calls.ToList());
        }

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

            Calls.Add(new CallViewVM
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

        switch (_round.State)
        {
            case RoundState.Calling:
                IsGameVisible = true;
                IsResultVisible = false;

                IsFixCallsVisible = true;
                IsEndRoundVisible = false;
                break;

            case RoundState.Playing:
                IsGameVisible = true;
                IsResultVisible = false;

                IsFixCallsVisible = false;
                IsEndRoundVisible = true;
                break;

            case RoundState.WaitingForNextRound:
                IsGameVisible = false;
                IsEndRoundVisible = false;
                IsResultVisible = true;
                break;
        }
    }
    private void ShowResultsUI()
    {
        IsGameVisible = false;
        IsResultVisible = true;
    }

    // COMMANDOK

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

        _round.State = RoundState.WaitingForNextRound;

        await LoadResults(calls);

        UpdateUI();
    }

    private async Task LoadResults(List<Call> calls)
    {
        Results.Clear();

        var players = await _players.GetByGameIdAsync(_round!.GameId);
        var dict = players.ToDictionary(p => p.Id);

        var game = await _games.GetByIdAsync(_round.GameId);
        if (game == null)
            throw new Exception("Game not found");

        foreach (var c in calls)
        {
            if (!dict.TryGetValue(c.GamePlayerId, out var player))
                continue;

            Results.Add(new ScoreView
            {
                PlayerName = player.GuestName,
                Called = c.Called ?? 0,
                Won = c.Won ?? 0,
                Score = _scoringService.CalculateScore(c, game.ScoringType)
            });
        }

        ShowResultsUI();
    }

    [RelayCommand]
    private async Task Less()
    {
        if (_round!.HandSize <= 1)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Hiba",
                "A hand size nem lehet 1-nél kevesebb.",
                "OK");
            return;
        }

        await CreateNextRound(_round.HandSize - 1);
    }

    [RelayCommand]
    private async Task Same() => await CreateNextRound(_round!.HandSize);

    [RelayCommand]
    private async Task More() => await CreateNextRound(_round!.HandSize + 1);

    private async Task CreateNextRound(int size)
    {
        if (_round == null)
            return;

        if (size <= 0)
        {
            await _nav.Pop();
            return;
        }

        var next = await _engine.FinishRoundAndCreateNext(_round.GameId, size);

        if (next == null)
        {
            await _nav.Pop();
            return;
        }

        _round = next;

        RoundText = $"Round {_round.RoundIndex} ({_round.HandSize} cards)";

        await LoadCalls();
        UpdateUI();
    }

    [RelayCommand]
    private async Task Back()
    {
        await _nav.Pop();
    }
}