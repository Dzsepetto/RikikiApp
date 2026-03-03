using SQLite;

namespace RikikiApp.Models;

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed(Unique = true)]
    public string Email { get; set; } = "";

    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
}