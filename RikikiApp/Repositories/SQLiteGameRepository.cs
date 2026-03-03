using RikikiApp.Data;
using RikikiApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RikikiApp.Repositories
{
    public class SqliteGameRepository : IGameRepository
    {
        private readonly LocalDB _localDb;

        public SqliteGameRepository(LocalDB localDb)
        {
            _localDb = localDb;
        }

        public async Task<List<Game>> GetAllAsync()
        {
            var db = await _localDb.GetAsync();
            return await db.Table<Game>()
                           .OrderByDescending(g => g.CreatedAt)
                           .ToListAsync();
        }

        public async Task<Game?> GetByIdAsync(int id)
        {
            var db = await _localDb.GetAsync();
            return await db.Table<Game>().Where(g => g.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Game> UpsertAsync(Game game)
        {
            var db = await _localDb.GetAsync();

            if (game.Id == 0)
            {
                await db.InsertAsync(game);
                // Insert után sqlite-net-pcl kitölti a game.Id-t
                return game;
            }

            await db.UpdateAsync(game);
            return game;
        }

        public async Task<int> DeleteAsync(int id)
        {
            var db = await _localDb.GetAsync();
            return await db.DeleteAsync<Game>(id);
        }
    }
}
