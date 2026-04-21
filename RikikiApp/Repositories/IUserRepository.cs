using RikikiApp.Core.Entities;

namespace RikikiApp.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetLocalUserAsync();
        Task<User?> GetByIdAsync(int id);
        Task<int> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
    }
}