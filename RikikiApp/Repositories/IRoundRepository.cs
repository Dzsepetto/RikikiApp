using RikikiApp.Models;

namespace RikikiApp.Repositories;

public interface IRoundRepository
{
    Task AddAsync(Round round);

    Task UpdateAsync(Round round);

    Task<Round?> GetByIdAsync(int id);

    Task<List<Round>> GetByGameIdAsync(int gameId);
}