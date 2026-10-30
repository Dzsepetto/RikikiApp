using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Infrastructure.Persistance;
using RikikiApp.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikikiApp.Repositories
{
    public class SQLiteRoundFinalizationRepository : IRoundFinalizationRepository
    {
        private readonly LocalDB _db;

        public SQLiteRoundFinalizationRepository(LocalDB db)
        {
            _db = db;
        }

        public async Task SaveAsync(Round round, List<Call> calls, List<RoundScore> scores)
        {
            var conn = await _db.GetAsync();

            await conn.RunInTransactionAsync(db =>
            {
                var currentRound = db.Find<Round>(round.Id);

                if (currentRound == null ||
                    currentRound.State != RoundState.Playing)
                {
                    throw new Exception("Round not in playing state.");
                }

                foreach (var call in calls)
                {
                    if (db.Update(call) != 1)
                        throw new Exception("Call update failed.");
                }

                db.Execute(
                    "DELETE FROM \"RoundScore\" WHERE \"RoundId\" = ?",
                    round.Id);

                foreach (var score in scores)
                {
                    db.Insert(score);
                }

                currentRound.State = RoundState.WaitingForNextRound;

                if (db.Update(currentRound) != 1)
                    throw new Exception("Round update failed.");
            });
        }
    }
}
