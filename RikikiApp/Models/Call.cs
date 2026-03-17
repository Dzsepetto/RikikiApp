using SQLite;

namespace RikikiApp.Models;

public class Call
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int RoundId { get; set; }

    [Indexed]
    public int GamePlayerId { get; set; }

    public int? Called { get; set; }

    public int? Won { get; set; }
}