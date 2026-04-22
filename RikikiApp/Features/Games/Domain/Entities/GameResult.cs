using SQLite;

namespace RikikiApp.Features.Games.Domain.Entities
{
    public class GameResult
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int GameId { get; set; }

        [Indexed]
        public int GamePlayerId { get; set; }

        public int FinalScore { get; set; }

        public int Placement { get; set; } // 1 = winner
        public bool IsWinner { get; set; }
    }
}
