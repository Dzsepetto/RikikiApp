using RikikiApp.Data;
using RikikiApp.Models;


namespace RikikiApp.Repositories
{
    public class SqliteGamePlayerRepository : IGamePlayerRepository
    {
        private readonly LocalDB _db;

        public SqliteGamePlayerRepository(LocalDB db)
        {
            _db = db;
        }

        public async Task AddAsync(GamePlayer gp)
        {
            var conn = await _db.GetAsync();
            await conn.InsertAsync(gp);
        }
        public async Task UpdateAsync(GamePlayer gp)
        {
            var db = await _db.GetAsync();
            await db.UpdateAsync(gp);
        }

        public async Task<List<GamePlayer>> GetByGameIdAsync(int gameId)
        {
            var conn = await _db.GetAsync();
            return await conn.Table<GamePlayer>()
                .Where(x => x.GameId == gameId)
                .OrderBy(x => x.SeatOrder)
                .ToListAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var conn = await _db.GetAsync();
            await conn.DeleteAsync<GamePlayer>(id);
        }
    }
}
