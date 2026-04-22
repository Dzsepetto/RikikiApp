using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;


namespace RikikiApp.Features.Games.Domain.Entities;

public partial class GamePlayer : ObservableObject
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int GameId { get; set; }

    [Indexed]
    public int? PlayerId { get; set; }

    [ObservableProperty]
    private int seatOrder;

    [ObservableProperty]
    private string guestName;

}