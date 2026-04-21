using System.Diagnostics;
using RikikiApp.Infrastructure.Persistance;
using RikikiApp.Features.Games.Domain.Entities;

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

        public async Task<List<Game>> GetByPlayerIdAsync(int playerId)
        {
            try
            {
                Debug.WriteLine($"SqliteGameRepository.GetByPlayerIdAsync START, playerId={playerId}");

                var db = await _localDb.GetAsync();
                Debug.WriteLine("Database connection acquired");

                var gamePlayers = await db.Table<GamePlayer>()
                    .Where(gp => gp.PlayerId == playerId)
                    .ToListAsync();

                Debug.WriteLine($"GamePlayers found: {gamePlayers.Count}");

                foreach (var gp in gamePlayers)
                {
                    Debug.WriteLine($"GamePlayer -> Id: {gp.Id}, GameId: {gp.GameId}, PlayerId: {gp.PlayerId}");
                }

                var gameIds = gamePlayers
                    .Select(gp => gp.GameId)
                    .Distinct()
                    .ToList();

                Debug.WriteLine($"Distinct gameIds count: {gameIds.Count}");

                var allGames = await db.Table<Game>().ToListAsync();
                Debug.WriteLine($"All games in DB: {allGames.Count}");

                var result = allGames
                    .Where(g => gameIds.Contains(g.Id))
                    .ToList();

                Debug.WriteLine($"Filtered games count: {result.Count}");
                Debug.WriteLine("SqliteGameRepository.GetByPlayerIdAsync END");

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SqliteGameRepository.GetByPlayerIdAsync ERROR: {ex}");
                throw;
            }
        }
    }
}
