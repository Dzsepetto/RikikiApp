using SQLite;
using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Core.Entities;

namespace RikikiApp.Infrastructure.Persistance;

public class LocalDB
{
    private SQLiteAsyncConnection? _db;

    public async Task InitAsync()
    {
        await GetAsync();
    }

    public async Task<SQLiteAsyncConnection> GetAsync()
    {
        if (_db != null) return _db;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "rikiki.db3");
        _db = new SQLiteAsyncConnection(dbPath);

        await _db.CreateTableAsync<User>();
        await _db.CreateTableAsync<Player>();
        await _db.CreateTableAsync<Game>();
        await _db.CreateTableAsync<GamePlayer>();
        await _db.CreateTableAsync<Round>();
        await _db.CreateTableAsync<Call>();
        await _db.CreateTableAsync<RoundScore>();
        await _db.CreateTableAsync<GameResult>();

        return _db;
    }
}