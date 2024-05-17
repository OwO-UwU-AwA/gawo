using System.Text.Json.Serialization;
using SurrealDb.Net.Models;

namespace GaWo;

// Class Representation Of Database Tables
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

    [JsonPropertyName("class")] public string Class { get; set; } = string.Empty;

    /*
        Add Other Roles As Required
        Teacher:        0000 0001
        Admin:          0000 0010
        Guest:          0000 0100

        Teacher+Admin:  0000 0011
    */
    [JsonPropertyName("permissions")] public byte Permissions { get; set; } = 0;

    /*
        Monday:     0000 0001
        Tuesday:    0000 0010
        Wednesday:  0000 0100
        Thursday:   0000 1000
        Friday:     0001 0000
     */
    [JsonPropertyName("absence")] public byte Absence { get; set; } = 0;
}

// No Use Right Now
public enum Subjects
{
    Art,
    English,
    French,
    German,
    Latin,
    Russian,
    Astronomy,
    Biology,
    Computerscience,
    Maths,
    Physics,
    Geography,
    History,
    Politics,
    Misc,
    Sport,
    Studiesconsulting
}

// No Use Right Now
public enum Type

{
    Presentation,
    GPresentation,
    FlangPresentation,
    ThesisDef,
    Competition,
    Workshop,
    Qf,
    Sport,
    Elmos,
}

public class Event : Record
{
    [JsonPropertyName("subject")] public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("accepted")] public bool Accepted { get; set; } = false;

    [JsonPropertyName("capacity")] public int Capacity { get; set; } = -1;

    [JsonPropertyName("date")] public DateTime? Date { get; set; } = DateTime.MinValue;

    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("duration")] public int Duration { get; set; } = -1;

    [JsonPropertyName("grades")] public int Grades { get; set; } = -1;

    [JsonPropertyName("notes")] public string? Notes { get; set; } = null;

    [JsonPropertyName("organiser")] public Thing Organiser { get; set; } = null!;

    [JsonPropertyName("picture")] public string? Picture { get; set; } = string.Empty;

    [JsonPropertyName("room")] public string? Room { get; set; } = string.Empty;

    [JsonPropertyName("teacher")] public Thing Teacher { get; set; } = null!;

    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
};