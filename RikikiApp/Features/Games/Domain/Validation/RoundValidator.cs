using RikikiApp.Features.Games.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikikiApp.Features.Games.Domain.Validation
{
    public class RoundValidator
    {
        public void ValidateStart(
            Round round,
            List<Call> calls,
            List<GamePlayer> players,
            List<Call> savedCalls,
            bool forbidEqualBidSum = false)
        {
            ValidatePlayersAndCalls(round, calls, players, savedCalls);

            if (round.State != RoundState.Calling)
                throw new InvalidOperationException(
                    "Round is not in calling state.");

            foreach (var call in calls)
            {
                if (!call.Called.HasValue)
                    throw new InvalidOperationException(
                        "Every player must make a call.");

                if (call.Called.Value < 0 ||
                    call.Called.Value > round.HandSize)
                {
                    throw new InvalidOperationException(
                        "Call must be between 0 and hand size.");
                }
            }

            if (forbidEqualBidSum &&
                calls.Sum(c => c.Called!.Value) == round.HandSize)
            {
                throw new InvalidOperationException(
                    "Total calls cannot equal hand size.");
            }
        }

        public void ValidateEnd(
            Round round,
            List<Call> calls,
            List<GamePlayer> players,
            List<Call> savedCalls)
        {
            ValidatePlayersAndCalls(round, calls, players, savedCalls);

            if (round.State != RoundState.Playing)
                throw new InvalidOperationException(
                    "Round is not in playing state.");

            foreach (var call in calls)
            {
                if (!call.Called.HasValue || !call.Won.HasValue)
                    throw new InvalidOperationException(
                        "Call not completed.");

                if (call.Called.Value < 0 ||
                    call.Called.Value > round.HandSize)
                {
                    throw new InvalidOperationException(
                        "Invalid call value.");
                }

                if (call.Won.Value < 0 ||
                    call.Won.Value > round.HandSize)
                {
                    throw new InvalidOperationException(
                        "Invalid won value.");
                }

                var saved = savedCalls.Single(c => c.Id == call.Id);

                if (saved.Called != call.Called)
                    throw new InvalidOperationException(
                        "Original call cannot be changed.");
            }

            if (calls.Sum(c => c.Won!.Value) != round.HandSize)
            {
                throw new InvalidOperationException(
                    "Total won tricks must equal hand size.");
            }
        }

        private void ValidatePlayersAndCalls(
            Round round,
            List<Call> calls,
            List<GamePlayer> players,
            List<Call> savedCalls)
        {
            if (round.HandSize < 1)
                throw new InvalidOperationException(
                    "Invalid hand size.");

            if (players.Count < 2)
                throw new InvalidOperationException(
                    "Minimum 2 players required.");

            if (calls.Count != players.Count)
                throw new InvalidOperationException(
                    "Every player must have exactly one call.");

            if (calls.Any(c => c.RoundId != round.Id))
                throw new InvalidOperationException(
                    "Calls belong to different rounds.");

            if (calls.Select(c => c.GamePlayerId)
                     .Distinct().Count() != calls.Count)
            {
                throw new InvalidOperationException(
                    "Duplicate player calls.");
            }

            var playerIds = players.Select(p => p.Id).ToHashSet();
            var callPlayerIds = calls.Select(c => c.GamePlayerId).ToHashSet();

            if (!playerIds.SetEquals(callPlayerIds))
                throw new InvalidOperationException(
                    "Calls do not match game players.");

            if (calls.Any(c => c.Id <= 0))
                throw new InvalidOperationException(
                    "Call must already exist in database.");

            if (savedCalls.Count != players.Count ||
                savedCalls.Select(c => c.GamePlayerId)
                          .Distinct().Count() != savedCalls.Count)
            {
                throw new InvalidOperationException(
                    "Invalid saved calls for round.");
            }

            var savedById = savedCalls.ToDictionary(c => c.Id);

            foreach (var call in calls)
            {
                if (!savedById.TryGetValue(call.Id, out var saved))
                    throw new InvalidOperationException(
                        "Call not found in database.");

                if (saved.RoundId != round.Id ||
                    saved.GamePlayerId != call.GamePlayerId)
                {
                    throw new InvalidOperationException(
                        "Call does not belong to this player and round.");
                }
            }
        }
    }
}
