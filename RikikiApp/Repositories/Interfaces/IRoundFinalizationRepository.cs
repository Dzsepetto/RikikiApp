using RikikiApp.Features.Games.Domain.Entities;

namespace RikikiApp.Repositories.Interfaces
{
    public interface IRoundFinalizationRepository
    {
        Task SaveAsync(
            Round round,
            List<Call> calls,
            List<RoundScore> scores);
    }
}
