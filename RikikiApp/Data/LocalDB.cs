using SQLite;
using RikikiApp.Models;

namespace RikikiApp.Data;

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

        return _db;
    }
}