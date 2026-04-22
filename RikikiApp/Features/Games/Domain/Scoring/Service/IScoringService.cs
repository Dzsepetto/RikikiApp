using RikikiApp.Features.Games.Domain.Entities;

namespace RikikiApp.Features.Games.Domain.Scoring.Service
{
    public interface IScoringService
    {
        int CalculateScore(Call call, ScoringType scoringType);
    }
}
