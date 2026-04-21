using SQLite;

namespace RikikiApp.Features.Games.Domain.Entities;

public class Player
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string Name { get; set; } = "";

    [Indexed]
    public int? UserId { get; set; }
}