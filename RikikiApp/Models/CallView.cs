namespace RikikiApp.Models;

public class CallView
{
    public int CallId { get; set; }

    public int GamePlayerId { get; set; }

    public string PlayerName { get; set; } = "";

    public int? Called { get; set; }

    public int? Won { get; set; }

    public bool IsCallEnabled { get; set; }

    public bool IsWonEnabled { get; set; }

    public bool IsWonVisible { get; set; }
}