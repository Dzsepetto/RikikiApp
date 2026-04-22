using RikikiApp.Features.Games.Domain.Entities;

namespace RikikiApp.Repositories.Interfaces;

public interface ICallRepository
{
    Task AddAsync(Call call);

    Task UpdateAsync(Call call);

    Task<Call?> GetByIdAsync(int id);

    Task<List<Call>> GetByRoundIdAsync(int roundId);
}