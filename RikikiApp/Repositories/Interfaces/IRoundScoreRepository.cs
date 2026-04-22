using RikikiApp.Features.Games.Domain.Entities;

namespace RikikiApp.Repositories.Interfaces;

public interface IRoundScoreRepository
{
    Task AddAsync(RoundScore score);

    Task UpdateAsync(RoundScore score);

    Task UpsertAsync(RoundScore score);

    Task<RoundScore?> GetByIdAsync(int id);

    Task<List<RoundScore>> GetByRoundIdAsync(int roundId);

    Task<List<RoundScore>> GetByGamePlayerIdAsync(int gamePlayerId);

    Task DeleteByRoundIdAsync(int roundId);
}