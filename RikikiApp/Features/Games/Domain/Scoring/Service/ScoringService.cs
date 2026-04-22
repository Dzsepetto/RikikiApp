using RikikiApp.Features.Games.Domain.Entities;

namespace RikikiApp.Features.Games.Domain.Scoring.Service
{
    public class ScoringService : IScoringService
    {
        public int CalculateScore(Call call, ScoringType scoringType)
        {
            var strategy = ScoringFactory.Create(scoringType);
            return strategy.CalculateScore(call);
        }
    }
}
