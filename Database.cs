using SurrealDb.Net.Models;
using System.Text.Json;

namespace Database
{

  public class User : Record
  {
      public int Absence { get; set; }
      public int Permissions { get; set; }
      public string? Firstname { get; set; }
      public string? Lastname { get; set; }
      public string? Username { get; set; }
      public string? Email { get; set; }
      public string? Password { get; set; }
      public string? Class { get; set; }
      public string? Masterpassword { get; set; }
  };

  public class Event : Record
  {
    public bool Accepted { get; set; }
    public int Capacity { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? Name { get; set; }
    public int Duration { get; set; }
    public int Grades { get; set; }
    public string? Notes { get; set; }
    public User? Organiser { get; set; }
    public byte[]? Picture { get; set; }
    public string? Room { get; set; }
    public User? Teacher { get; set; }
    public string? Type { get; set; }
  };

  public class Surreal
  {
    public static string ToJsonString(object? o)
    {
      return JsonSerializer.Serialize(o, new JsonSerializerOptions
      {
        WriteIndented = true,
      });
    }
  };

  public class LowerCaseNamingPolicy : JsonNamingPolicy
  {
    public override string ConvertName(string name)
    {
      return name.ToLower();
    }
  }
}