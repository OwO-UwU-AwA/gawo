
namespace gawo.Models;

public class User
{
    public int id { get; set; }
    public string username { get; set; } = string.Empty;
    public string password_hash { get; set; } = string.Empty;
    public string password_salt { get; set; } = string.Empty;
}