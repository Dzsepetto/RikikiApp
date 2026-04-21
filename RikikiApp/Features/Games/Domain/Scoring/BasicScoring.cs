using System;
using System.Collections.Generic;
using System.Text;
using RikikiApp.Features.Games.Domain.Entities;

namespace RikikiApp.Features.Games.Domain.Scoring
{
    public class BasicScoring : IScoringStrategy
    {
        public int CalculateScore(Call call)
        {
            if (!call.Called.HasValue || !call.Won.HasValue)
                return 0;

            if (call.Called == call.Won)
                return 10 + call.Called.Value * 2;

            return -Math.Abs(call.Called.Value - call.Won.Value);
        }
    }
}
