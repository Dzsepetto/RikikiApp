using System;
using System.Collections.Generic;
using System.Text;
using RikikiApp.Features.Games.Domain.Entities;


namespace RikikiApp.Features.Games.Domain.Scoring
{
    public class HardcoreScoring : IScoringStrategy
    {
        public int CalculateScore(Call call)
        {
            if (!call.Called.HasValue || !call.Won.HasValue)
                return 0;
            
            if (call.Called == call.Won)
            {
                if (call.Called == 0) return 2;

                return 0 + call.Called.Value * 2;
            }

            return -2 * Math.Abs(call.Called.Value - call.Won.Value);
        }
    }
}
