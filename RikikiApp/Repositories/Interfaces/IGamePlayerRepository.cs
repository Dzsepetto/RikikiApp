using RikikiApp.Features.Games.Domain.Entities;


namespace RikikiApp.Repositories.Interfaces
{
    public interface IGamePlayerRepository
    {
        Task AddAsync(GamePlayer gp);
        Task<List<GamePlayer>> GetByGameIdAsync(int gameId);
        Task UpdateAsync(GamePlayer gp);
        Task DeleteAsync(int id);
    }
}
