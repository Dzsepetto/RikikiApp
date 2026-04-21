using RikikiApp.Features.Games.Domain.Entities;

namespace RikikiApp.Repositories;

public interface IRoundRepository
{
    Task AddAsync(Round round);

    Task UpdateAsync(Round round);

    Task<Round?> GetByIdAsync(int id);

    Task<List<Round>> GetByGameIdAsync(int gameId);
}