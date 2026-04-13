using RikikiApp.Models;

namespace RikikiApp.Repositories
{
    public interface IPlayerRepository
    {
        Task<List<Player>> GetAllAsync();
        Task<Player?> GetByIdAsync(int id);
        Task<Player?> GetByUserIdAsync(int userId);
        Task<int> AddAsync(Player player);
        Task UpdateAsync(Player player);
        Task DeleteAsync(int id);
    }
}