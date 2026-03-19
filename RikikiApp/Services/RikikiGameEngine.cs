using RikikiApp.Models;
using RikikiApp.Repositories;

namespace RikikiApp.Services;

public class RikikiGameEngine
{
    private readonly IGameRepository _games;
    private readonly IGamePlayerRepository _players;
    private readonly IRoundRepository _rounds;
    private readonly ICallRepository _calls;

    public RikikiGameEngine(
        IGameRepository games,
        IGamePlayerRepository players,
        IRoundRepository rounds,
        ICallRepository calls)
    {
        _games = games;
        _players = players;
        _rounds = rounds;
        _calls = calls;
    }

    public async Task StartGame(int gameId)
    {
        var game = await _games.GetByIdAsync(gameId);
        if (game == null)
            return;

        var players = await _players.GetByGameIdAsync(gameId);

        if (players.Count < 2)
            throw new Exception("Minimum 2 players required.");

        game.Status = GameStatus.InProgress;
        await _games.UpsertAsync(game);

       // await GenerateRounds(gameId, players.Count);
        await CreateNextRound(gameId, 1);
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

    // CALL mentése
    public async Task StartRound(List<Call> calls)
    {
        if (calls.Count == 0)
            return;

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

    // ROUND vége
    public async Task<Round?> EndRound(List<Call> calls)
    {
        if (calls.Count == 0)
            return null;

        var round = await _rounds.GetByIdAsync(calls.First().RoundId);

        if (round == null || round.State != RoundState.Playing)
            throw new Exception("Round not in playing state.");

        foreach (var call in calls)
        {
            await _calls.UpdateAsync(call);
        }

        round.State = RoundState.Finished;
        round.isCompleted = true;

        await _rounds.UpdateAsync(round);

        return await NextRound(round.GameId);
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
            var game = await _games.GetByIdAsync(gameId);

            if (game != null)
            {
                game.Status = GameStatus.Finished;
                await _games.UpsertAsync(game);
            }

            return null;
        }

        return next;
    }

    public int CalculateScore(Call call)
    {
        if (!call.Called.HasValue || !call.Won.HasValue)
            return 0;

        if (call.Called == call.Won)
            return 10 + call.Called.Value;

        return -Math.Abs(call.Called.Value - call.Won.Value);
    }
}
