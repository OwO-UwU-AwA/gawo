using System.Text.Json.Serialization;
using SurrealDb.Net.Models;

namespace GaWo;

public class VerificationLink : Record
{
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("user")] public Thing User { get; set; } = null!;
    [JsonPropertyName("secret")] public string Secret { get; set; } = string.Empty;
}

public class GawoUser : Record
{
    [JsonPropertyName("firstname")] public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastname")] public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("masterpassword")] public string MasterPassword { get; set; } = string.Empty;

    [JsonPropertyName("password")] public string Password { get; set; } = string.Empty;

    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;

    [JsonPropertyName("class")] public string? Class { get; set; } = string.Empty;

    /*
        Teacher:        0000 0001
        Admin:          0000 0010
        Guest:          0000 0100

        Teacher+Admin:  0000 0011
    */
    [JsonPropertyName("permissions")] public byte Permissions { get; set; }

    /*
        Monday:     0000 0001
        Tuesday:    0000 0010
        Wednesday:  0000 0100
        Thursday:   0000 1000
        Friday:     0001 0000
     */
    [JsonPropertyName("absence")] public byte Absence { get; set; }
}

public class Event : Record
{
    [JsonPropertyName("accepted")] public bool Accepted { get; set; }

    [JsonPropertyName("capacity")] public int Capacity { get; set; }

    [JsonPropertyName("date")] public DateTime Date { get; set; }

    [JsonPropertyName("description")] public string? Description { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("duration")] public int Duration { get; set; }

    [JsonPropertyName("grades")] public int Grades { get; set; }

    [JsonPropertyName("notes")] public string? Notes { get; set; }

    [JsonPropertyName("organiser")] public GawoUser? Organiser { get; set; }

    [JsonPropertyName("picture")] public string? Picture { get; set; }

    [JsonPropertyName("room")] public string? Room { get; set; }

    [JsonPropertyName("teacher")] public GawoUser? Teacher { get; set; }

    [JsonPropertyName("type")] public string? Type { get; set; }
};