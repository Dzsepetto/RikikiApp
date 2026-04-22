using RikikiApp.Features.Games.Domain.Entities;

namespace RikikiApp.Repositories.Interfaces
{
    public interface IGameRepository
    {
        Task<List<Game>> GetAllAsync();
        Task<Game?> GetByIdAsync(int id);
        Task<List<Game>> GetByPlayerIdAsync(int playerId);
        Task<Game> UpsertAsync(Game game);
        Task<int> DeleteAsync(int id);
    }
}
