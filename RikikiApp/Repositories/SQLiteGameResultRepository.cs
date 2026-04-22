using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Infrastructure.Persistance;
using RikikiApp.Repositories.Interfaces;

namespace RikikiApp.Repositories;

public class SQLiteGameResultRepository : IGameResultRepository
{
    private readonly LocalDB _db;

    public SQLiteGameResultRepository(LocalDB db)
    {
        _db = db;
    }

    public async Task AddAsync(GameResult result)
    {
        var conn = await _db.GetAsync();
        await conn.InsertAsync(result);
    }

    public async Task UpdateAsync(GameResult result)
    {
        var conn = await _db.GetAsync();
        await conn.UpdateAsync(result);
    }

    public async Task UpsertAsync(GameResult result)
    {
        var conn = await _db.GetAsync();

        var existing = await conn.Table<GameResult>()
            .Where(x => x.GameId == result.GameId &&
                        x.GamePlayerId == result.GamePlayerId)
            .FirstOrDefaultAsync();

        if (existing == null)
        {
            await conn.InsertAsync(result);
        }
        else
        {
            existing.FinalScore = result.FinalScore;
            existing.Placement = result.Placement;
            existing.IsWinner = result.IsWinner;

            await conn.UpdateAsync(existing);
        }
    }

    public async Task<GameResult?> GetByIdAsync(int id)
    {
        var conn = await _db.GetAsync();

        return await conn.Table<GameResult>()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<GameResult>> GetByGameIdAsync(int gameId)
    {
        var conn = await _db.GetAsync();

        return await conn.Table<GameResult>()
            .Where(x => x.GameId == gameId)
            .ToListAsync();
    }

    public async Task<List<GameResult>> GetByGamePlayerIdAsync(int gamePlayerId)
    {
        var conn = await _db.GetAsync();

        return await conn.Table<GameResult>()
            .Where(x => x.GamePlayerId == gamePlayerId)
            .ToListAsync();
    }

    public async Task DeleteByGameIdAsync(int gameId)
    {
        var conn = await _db.GetAsync();

        var items = await conn.Table<GameResult>()
            .Where(x => x.GameId == gameId)
            .ToListAsync();

        foreach (var item in items)
        {
            await conn.DeleteAsync(item);
        }
    }
}