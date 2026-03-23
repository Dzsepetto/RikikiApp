
using System;
using System.Collections.Generic;
using System.Text;

namespace RikikiApp.Models
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
    public enum ScoringType
    {
        Basic = 1,
        Hardcore = 2,
    }
    public interface IScoringStrategy
    {
        int CalculateScore(Call call);
    }
    public class HardcoreScoring : IScoringStrategy
    {
        public int CalculateScore(Call call)
        {
            if (!call.Called.HasValue || !call.Won.HasValue)
                return 0;

            if (call.Called == call.Won) { 
                if (call.Called == 0) return 2;

                return 0 + call.Called.Value * 2;
            }

            return -2 * Math.Abs(call.Called.Value - call.Won.Value);
        }
    }
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
