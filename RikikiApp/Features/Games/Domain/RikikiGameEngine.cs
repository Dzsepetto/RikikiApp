using CommunityToolkit.Mvvm.Input;
using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Features.Games.Domain.Scoring;
using RikikiApp.Features.Games.Domain.Scoring.Service;
using RikikiApp.Features.Games.Domain.Validation;
using RikikiApp.Repositories.Interfaces;

namespace RikikiApp.Features.Games.Domain;

public class RikikiGameEngine
{
    private readonly IGameRepository _gamesrepo;
    private readonly IGamePlayerRepository _players;
    private readonly IRoundRepository _rounds;
    private readonly ICallRepository _calls;
    private readonly IScoringService _scoring;
    private readonly IRoundScoreRepository _roundScore;
    private readonly IRoundFinalizationRepository _roundFinalization;
    private readonly IGameResultRepository _gameResult;
    private readonly RoundValidator _validator;

    public RikikiGameEngine(
        IGameRepository games,
        IGamePlayerRepository players,
        IRoundRepository rounds,
        ICallRepository calls,
        IScoringService scoring,
        IRoundScoreRepository roundScore,
        IGameResultRepository result,
        IRoundFinalizationRepository roundFinalization, 
        RoundValidator validator)
    {
        _gamesrepo = games;
        _players = players;
        _rounds = rounds;
        _calls = calls;
        _scoring = scoring;
        _roundScore = roundScore;
        _gameResult = result;
        _roundFinalization = roundFinalization;
        _validator = validator;
    }

    public async Task<Game?> StartGame(int gameId)
    {
        var game = await _gamesrepo.GetByIdAsync(gameId);

        if (game == null)
            return null;

        if (game.Status == GameStatus.Finished)
            return game;

        if (game.Status == GameStatus.InProgress)
            return game;

        var players = await _players.GetByGameIdAsync(gameId);

        if (players.Count < 2)
            throw new Exception("Minimum 2 players required.");

        game.Status = GameStatus.InProgress;
        await _gamesrepo.UpsertAsync(game);

        await CreateNextRound(gameId, 1);

        return game;
    }
    public async Task<Round?> ContinueGame(int gameId)
    {
        var game = await _gamesrepo.GetByIdAsync(gameId);

        if (game == null || game.Status != GameStatus.InProgress)
            return null;

        var rounds = await _rounds.GetByGameIdAsync(gameId);

        var currentRound = rounds
            .Where(r =>
                r.State == RoundState.Calling ||
                r.State == RoundState.Playing ||
                r.State == RoundState.WaitingForNextRound)
            .OrderByDescending(r => r.RoundIndex)
            .FirstOrDefault();

        return currentRound;
    }

    public async Task EndGame(int gameId)
    {
        var game = await _gamesrepo.GetByIdAsync(gameId);
        if (game == null)
            return;

        var rounds = await _rounds.GetByGameIdAsync(gameId);

        if (rounds.Count == 0)
            throw new Exception("Game has no rounds.");

        var allRoundScores = new List<RoundScore>();

        foreach (var round in rounds)
        {
            var scores = await _roundScore.GetByRoundIdAsync(round.Id);
            allRoundScores.AddRange(scores);
        }

        if (allRoundScores.Count == 0)
            throw new Exception("Game has no round scores.");

        await _gameResult.DeleteByGameIdAsync(gameId);

        var grouped = allRoundScores
            .GroupBy(x => x.GamePlayerId)
            .Select(g => new
            {
                GamePlayerId = g.Key,
                FinalScore = g.Sum(x => x.Score)
            })
            .OrderByDescending(x => x.FinalScore)
            .ToList();

        int rank = 0;
        int placement = 0;
        int? lastScore = null;

        foreach (var item in grouped)
        {
            rank++;

            if (lastScore != item.FinalScore)
            {
                placement = rank;
                lastScore = item.FinalScore;
            }

            await _gameResult.UpsertAsync(new GameResult
            {
                GameId = gameId,
                GamePlayerId = item.GamePlayerId,
                FinalScore = item.FinalScore,
                Placement = placement,
                IsWinner = placement == 1
            });
        }

        game.Status = GameStatus.Finished;
        await _gamesrepo.UpsertAsync(game);
    }

    public async Task<Round> CreateNextRound(int gameId, int handSize)
    {
        var rounds = await _rounds.GetByGameIdAsync(gameId);

        int nextIndex = rounds.Any() ? rounds.Max(r => r.RoundIndex) + 1 : 1;

        var round = new Round
        {
            GameId = gameId,
            RoundIndex = nextIndex,
            HandSize = handSize,
            State = RoundState.Calling
        };

        await _rounds.AddAsync(round);

        var savedRound = await _rounds.GetByIdAsync(round.Id);

        await CreateCallsForRound(savedRound!);

        return round;
    }
    private async Task CreateCallsForRound(Round round)
    {
        var players = await _players.GetByGameIdAsync(round.GameId);

        foreach (var p in players)
        {
            var call = new Call
            {
                RoundId = round.Id,
                GamePlayerId = p.Id
            };

            await _calls.AddAsync(call);
        }
    }

    public async Task StartRound(List<Call> calls)
    {
        if (calls == null || calls.Count == 0)
            throw new InvalidOperationException("No calls provided.");

        var round = await _rounds.GetByIdAsync(calls[0].RoundId);

        if (round == null)
            throw new InvalidOperationException("Round not found.");

        var players = await _players.GetByGameIdAsync(round.GameId);
        var savedCalls = await _calls.GetByRoundIdAsync(round.Id);

        _validator.ValidateStart(
            round,
            calls,
            players,
            savedCalls);

        foreach (var call in calls)
        {
            await _calls.UpdateAsync(call);
        }

        round.State = RoundState.Playing;
        await _rounds.UpdateAsync(round);
    }

    public async Task EndRound(List<Call> calls)
    {
        if (calls == null || calls.Count == 0)
            throw new InvalidOperationException("No calls provided.");

        var round = await _rounds.GetByIdAsync(calls[0].RoundId);

        if (round == null)
            throw new InvalidOperationException("Round not found.");

        var game = await _gamesrepo.GetByIdAsync(round.GameId);

        if (game == null)
            throw new InvalidOperationException("Game not found.");

        var players = await _players.GetByGameIdAsync(game.Id);
        var savedCalls = await _calls.GetByRoundIdAsync(round.Id);

        _validator.ValidateEnd(
            round,
            calls,
            players,
            savedCalls);

        var scores = calls.Select(call => new RoundScore
        {
            RoundId = round.Id,
            GamePlayerId = call.GamePlayerId,
            Score = _scoring.CalculateScore(call, game.ScoringType)
        }).ToList();

        await _roundFinalization.SaveAsync(round, calls, scores);
    }

    public async Task<Round?> FinishRoundAndCreateNext(int gameId, int nextHandSize)
    {
        var rounds = await _rounds.GetByGameIdAsync(gameId);

        var current = rounds
            .Where(r => r.State == RoundState.WaitingForNextRound)
            .OrderByDescending(r => r.RoundIndex)
            .FirstOrDefault();

        if (current == null)
            return null;

        current.State = RoundState.Finished;
        current.isCompleted = true;

        await _rounds.UpdateAsync(current);

        return await CreateNextRound(gameId, nextHandSize);
    }
    public async Task<Round?> NextRound(int gameId)
    {
        var rounds = await _rounds.GetByGameIdAsync(gameId);

        var next = rounds
            .Where(r => r.State == RoundState.Calling)
            .OrderBy(r => r.RoundIndex)
            .FirstOrDefault();

        if (next == null)
        {
            var game = await _gamesrepo.GetByIdAsync(gameId);

            if (game != null)
            {
                game.Status = GameStatus.Finished;
                await _gamesrepo.UpsertAsync(game);
            }

            return null;
        }

        return next;
    }

    public int CalculateScore(Call call)
    {
        var scoring = ScoringFactory.Create(ScoringType.Basic);

        int points = scoring.CalculateScore(call);

        return points;
    }
}
