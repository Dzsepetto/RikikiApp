using SQLite;
using RikikiApp.Features.Games.Domain.Scoring;

namespace RikikiApp.Features.Games.Domain.Entities;

public class Game
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ScoringType ScoringType { get; set; } = ScoringType.Basic;
    public int? OwnerUserId { get; set; }
    public GameStatus Status { get; set; } = GameStatus.Setup;
}

public enum GameStatus
{
    Setup = 0,
    InProgress = 1,
    Finished = 2
}