using System;
using System.Collections.Generic;
using System.Text;

namespace RikikiApp.Features.Games.Domain.Scoring
{
    public static class ScoringFactory
    {
        public static IScoringStrategy Create(ScoringType type)
        {
            return type switch
            {
                ScoringType.Basic => new BasicScoring(),
                ScoringType.Hardcore => new HardcoreScoring(),
                _ => throw new NotImplementedException()
            };
        }
    }
}
