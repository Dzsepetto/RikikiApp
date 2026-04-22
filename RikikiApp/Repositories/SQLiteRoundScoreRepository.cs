using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Infrastructure.Persistance;
using RikikiApp.Repositories.Interfaces;

namespace RikikiApp.Repositories;

public class SQLiteRoundScoreRepository : IRoundScoreRepository
{
    private readonly LocalDB _db;

    public SQLiteRoundScoreRepository(LocalDB db)
    {
        _db = db;
    }

    public async Task AddAsync(RoundScore score)
    {
        var conn = await _db.GetAsync();
        await conn.InsertAsync(score);
    }

    public async Task UpdateAsync(RoundScore score)
    {
        var conn = await _db.GetAsync();
        await conn.UpdateAsync(score);
    }

    public async Task UpsertAsync(RoundScore score)
    {
        var conn = await _db.GetAsync();

        var existing = await conn.Table<RoundScore>()
            .Where(x => x.RoundId == score.RoundId &&
                        x.GamePlayerId == score.GamePlayerId)
            .FirstOrDefaultAsync();

        if (existing == null)
        {
            await conn.InsertAsync(score);
        }
        else
        {
            existing.Score = score.Score;
            await conn.UpdateAsync(existing);
        }
    }

    public async Task<RoundScore?> GetByIdAsync(int id)
    {
        var conn = await _db.GetAsync();

        return await conn.Table<RoundScore>()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<RoundScore>> GetByRoundIdAsync(int roundId)
    {
        var conn = await _db.GetAsync();

        return await conn.Table<RoundScore>()
            .Where(x => x.RoundId == roundId)
            .ToListAsync();
    }

    public async Task<List<RoundScore>> GetByGamePlayerIdAsync(int gamePlayerId)
    {
        var conn = await _db.GetAsync();

        return await conn.Table<RoundScore>()
            .Where(x => x.GamePlayerId == gamePlayerId)
            .ToListAsync();
    }

    public async Task DeleteByRoundIdAsync(int roundId)
    {
        var conn = await _db.GetAsync();

        var items = await conn.Table<RoundScore>()
            .Where(x => x.RoundId == roundId)
            .ToListAsync();

        foreach (var item in items)
        {
            await conn.DeleteAsync(item);
        }
    }
}