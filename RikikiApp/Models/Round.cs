using SQLite;

namespace RikikiApp.Models;

public class Round
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int GameId { get; set; }

    public int RoundIndex { get; set; }
    public int HandSize { get; set; }
    public bool isCompleted { get; set; }
    public RoundState State { get; set; } = RoundState.Calling;
}
public enum RoundState
{
    Calling,
    Playing,
    Finished
}