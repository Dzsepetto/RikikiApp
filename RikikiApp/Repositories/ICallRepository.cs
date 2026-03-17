using RikikiApp.Models;

namespace RikikiApp.Repositories;

public interface ICallRepository
{
    Task AddAsync(Call call);

    Task UpdateAsync(Call call);

    Task<Call?> GetByIdAsync(int id);

    Task<List<Call>> GetByRoundIdAsync(int roundId);
}