using RikikiApp.Data;
using RikikiApp.Models;

namespace RikikiApp.Repositories
{
    internal class SQLiteUserRepository : IUserRepository
    {
        private readonly LocalDB _db;

        public SQLiteUserRepository(LocalDB db)
        {
            _db = db;
        }

        public async Task<User?> GetLocalUserAsync()
        {
            var conn = await _db.GetAsync();

            return await conn.Table<User>()
                .FirstOrDefaultAsync(u => u.IsLocalUser);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            var conn = await _db.GetAsync();

            return await conn.Table<User>()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<int> AddAsync(User user)
        {
            var conn = await _db.GetAsync();

            return await conn.InsertAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            var conn = await _db.GetAsync();

            await conn.UpdateAsync(user);
        }

        public async Task DeleteAsync(int id)
        {
            var conn = await _db.GetAsync();

            await conn.DeleteAsync<User>(id);
        }
    }
}