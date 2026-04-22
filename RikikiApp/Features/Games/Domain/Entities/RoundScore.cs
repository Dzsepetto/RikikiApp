using SQLite;

namespace RikikiApp.Features.Games.Domain.Entities
{
    public class RoundScore
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int RoundId { get; set; }

        [Indexed]
        public int GamePlayerId { get; set; }

        public int Score { get; set; }
    }
}
