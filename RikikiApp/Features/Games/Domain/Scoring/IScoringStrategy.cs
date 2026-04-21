using System;
using System.Collections.Generic;
using System.Text;
using RikikiApp.Features.Games.Domain.Entities;

namespace RikikiApp.Features.Games.Domain.Scoring
{
    public interface IScoringStrategy
    {
        int CalculateScore(Call call);
    }
}
