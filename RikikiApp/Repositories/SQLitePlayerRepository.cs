using RikikiApp.Data;
using RikikiApp.Models;

namespace RikikiApp.Repositories
{
    public class SQLitePlayerRepository : IPlayerRepository
    {
        private readonly LocalDB _db;

        public SQLitePlayerRepository(LocalDB db)
        {
            _db = db;
        }

        public async Task<List<Player>> GetAllAsync()
        {
            var conn = await _db.GetAsync();
            return await conn.Table<Player>()
                             .OrderBy(p => p.Name)
                             .ToListAsync();
        }

        public async Task<Player?> GetByIdAsync(int id)
        {
            var conn = await _db.GetAsync();
            return await conn.Table<Player>()
                             .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Player player)
        {
            var conn = await _db.GetAsync();

            var existing = await conn.Table<Player>()
                .FirstOrDefaultAsync(p => p.Name == player.Name);

            if (existing == null)
            {
                await conn.InsertAsync(player);
            }

        }

        public async Task UpdateAsync(Player player)
        {
            var conn = await _db.GetAsync();
            await conn.UpdateAsync(player);
        }

        public async Task DeleteAsync(int id)
        {
            var conn = await _db.GetAsync();
            await conn.DeleteAsync<Player>(id);
        }
    }
}