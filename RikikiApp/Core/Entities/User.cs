using SQLite;

namespace RikikiApp.Core.Entities;

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string Email { get; set; } = "";

    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string ProfilePicture { get; set; } = "";
    public bool IsLocalUser { get; set; }
    public bool IsGuest { get; set; }
}