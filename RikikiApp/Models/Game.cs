using SQLite;

namespace RikikiApp.Models;

public class Game
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ScoringVersion { get; set; } = "classic-v1";
    public int? OwnerUserId { get; set; }
    public GameStatus Status { get; set; } = GameStatus.Setup;
}

public enum GameStatus
{
    Setup = 0,
    InProgress = 1,
    Finished = 2
}