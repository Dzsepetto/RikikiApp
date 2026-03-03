using SQLite;

namespace RikikiApp.Models;

public class GamePlayer
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int GameId { get; set; }

    [Indexed]
    public int PlayerId { get; set; }

    public int SeatOrder { get; set; }
}