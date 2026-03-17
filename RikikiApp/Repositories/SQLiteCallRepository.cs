using RikikiApp.Data;
using RikikiApp.Models;

namespace RikikiApp.Repositories;

public class SQLiteCallRepository : ICallRepository
{
    private readonly LocalDB _db;

    public SQLiteCallRepository(LocalDB db)
    {
        _db = db;
    }

    public async Task AddAsync(Call call)
    {
        var conn = await _db.GetAsync();
        await conn.InsertAsync(call);
    }

    public async Task UpdateAsync(Call call)
    {
        var conn = await _db.GetAsync();
        await conn.UpdateAsync(call);
    }

    public async Task<Call?> GetByIdAsync(int id)
    {
        var conn = await _db.GetAsync();

        return await conn.Table<Call>()
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Call>> GetByRoundIdAsync(int roundId)
    {
        var conn = await _db.GetAsync();

        return await conn.Table<Call>()
            .Where(c => c.RoundId == roundId)
            .ToListAsync();
    }
}