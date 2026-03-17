using RikikiApp.Data;
using RikikiApp.Models;

namespace RikikiApp.Repositories;

public class SQLiteRoundRepository : IRoundRepository
{
    private readonly LocalDB _db;

    public SQLiteRoundRepository(LocalDB db)
    {
        _db = db;
    }

    public async Task AddAsync(Round round)
    {
        var conn = await _db.GetAsync();
        await conn.InsertAsync(round);
    }

    public async Task UpdateAsync(Round round)
    {
        var conn = await _db.GetAsync();
        await conn.UpdateAsync(round);
    }

    public async Task<Round?> GetByIdAsync(int id)
    {
        var conn = await _db.GetAsync();

        return await conn.Table<Round>()
            .Where(r => r.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Round>> GetByGameIdAsync(int gameId)
    {
        var conn = await _db.GetAsync();

        return await conn.Table<Round>()
            .Where(r => r.GameId == gameId)
            .OrderBy(r => r.RoundIndex)
            .ToListAsync();
    }
}