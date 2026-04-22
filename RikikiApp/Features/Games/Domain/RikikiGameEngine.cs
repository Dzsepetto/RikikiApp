using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Features.Games.Domain.Scoring;
using RikikiApp.Features.Games.Domain.Scoring.Service;
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
    private readonly IGameResultRepository _gameResult;

    public RikikiGameEngine(
        IGameRepository games,
        IGamePlayerRepository players,
        IRoundRepository rounds,
        ICallRepository calls,
        IScoringService scoring,
        IRoundScoreRepository roundScore,
        IGameResultRepository result)
    {
        _gamesrepo = games;
        _players = players;
        _rounds = rounds;
        _calls = calls;
        _scoring = scoring;
        _roundScore = roundScore;
        _gameResult = result;
    }

    public async Task StartGame(int gameId)
    {
        var game = await _gamesrepo.GetByIdAsync(gameId);
        if (game == null)
            return;

        if (game.Status != GameStatus.Setup)
            return;

        if (game.Status == GameStatus.Finished)
            return;

        var players = await _players.GetByGameIdAsync(gameId);

        if (players.Count < 2)
            throw new Exception("Minimum 2 players required.");

        game.Status = GameStatus.InProgress;
        await _gamesrepo.UpsertAsync(game);

        await CreateNextRound(gameId, 1);
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
        foreach (var call in calls)
        {
            if (call.Id == 0)
            {
                await _calls.AddAsync(call);
            }
            else
            {
                await _calls.UpdateAsync(call);
            }
        }

        var round = await _rounds.GetByIdAsync(calls.First().RoundId);

        if (round == null || round.State != RoundState.Calling)
            throw new Exception("Round not in calling state.");

        foreach (var call in calls)
        {
            await _calls.UpdateAsync(call);
        }

        round.State = RoundState.Playing;

        await _rounds.UpdateAsync(round);
    }

    public async Task EndRound(List<Call> calls)
    {
        if (calls.Count == 0)
            return;

        var round = await _rounds.GetByIdAsync(calls.First().RoundId);

        if (round == null || round.State != RoundState.Playing)
            throw new Exception("Round not in playing state.");

        var game = await _gamesrepo.GetByIdAsync(round.GameId);
        if (game == null)
            throw new Exception("Game not found.");

        foreach (var call in calls)
        {
            await _calls.UpdateAsync(call);
        }

        await _roundScore.DeleteByRoundIdAsync(round.Id);

        foreach (var call in calls)
        {
            if (!call.Called.HasValue || !call.Won.HasValue)
                throw new Exception("Call not completed.");

            var score = _scoring.CalculateScore(call, game.ScoringType);

            await _roundScore.UpsertAsync(new RoundScore
            {
                RoundId = round.Id,
                GamePlayerId = call.GamePlayerId,
                Score = score
            });
        }

        round.State = RoundState.WaitingForNextRound;
        await _rounds.UpdateAsync(round);
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
