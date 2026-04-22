using RikikiApp.Features.Games.Domain.Entities;

namespace RikikiApp.Repositories.Interfaces;

public interface IGameResultRepository
{
    Task AddAsync(GameResult result);

    Task UpdateAsync(GameResult result);

    Task UpsertAsync(GameResult result);

    Task<GameResult?> GetByIdAsync(int id);

    Task<List<GameResult>> GetByGameIdAsync(int gameId);

    Task<List<GameResult>> GetByGamePlayerIdAsync(int gamePlayerId);

    Task DeleteByGameIdAsync(int gameId);
}