using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.Data.SQLite;
using System.Diagnostics;

namespace gawo.Pages;

[Authorize]
public class AddEventModel : PageModel
{
    private readonly ILogger<AddEventModel> _logger;

    [BindProperty]
    public string Name { get; set; } = string.Empty;
    [BindProperty]
    public string Description { get; set; } = string.Empty;
    [BindProperty]
    public byte[]? Picture { get; set; } = null;
    [BindProperty]
    public int Capacity { get; set; } = -1;
    [BindProperty]
    public int Duration { get; set; } = -1;
    [BindProperty]
    public byte Grades { get; set; } = 0;
    [BindProperty]
    public string Notes { get; set; } = string.Empty;
    [BindProperty]
    public string Organiser { get; set; } = string.Empty;
    [BindProperty]
    public string Type { get; set; } = "NULL";
    [BindProperty]
    public int Teacher { get; set; } = -1;
    [BindProperty]
    public bool Grade7 { get; set; } = false;
    [BindProperty]
    public bool Grade8 { get; set; } = false;
    [BindProperty]
    public bool Grade9 { get; set; } = false;
    [BindProperty]
    public bool Grade10 { get; set; } = false;
    [BindProperty]
    public bool Grade11 { get; set; } = false;
    [BindProperty]
    public bool Grade12 { get; set; } = false;

    public string ErrorString { get; set; } = string.Empty;

    public IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

    public AddEventModel(ILogger<AddEventModel> logger)
    
    {
        _logger = logger;
    }

    public IActionResult OnPostAddEvent()
    {
        // Execute Pre-Setup-Steps And Check For Errors
        if (PreSetup() == -1)
            return Error("Unvollständige Angaben (Standardwert Oder NULL)");
        
        using (var connection = new SQLiteConnection(configuration.GetConnectionString("GawoDbContext")))
        {
            
            // Will Become The ID Of The Event
            int id = 0;

            // Get The Organisers ID
            // BEGIN
            connection.Open();
            string query = "SELECT id FROM users WHERE username = @username";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@username", Organiser);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Read The ID Of The User As An Int16 Then Parse It To String
                        Organiser = reader.GetInt16(0).ToString();
                    }
                }
            }
            connection.Close();
            // END
            
            // Get The Amount Of Events + 1 For The New Event
            // BEGIN
            connection.Open();
            query = "SELECT COUNT(*) FROM events";
            using (var command = new SQLiteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Current ID
                        id = reader.GetInt16(0);
                        // 0 = No Entries
                        if (id == 0)
                            id = 1;
                        else
                            id += 1;
                    }
                }
            }
            connection.Close();
            // END

            // Insert Formatted Data Into `events` Table
            // BEGIN
            connection.Open();
            query = "INSERT INTO events (id, name, description, picture, capacity, duration, grades, notes, organiser, type, accepted) VALUES (@id, @name, @description, @picture, @capacity, @duration, @grades, @notes, @organiser, @type, @accepted)";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@name", Name);
                command.Parameters.AddWithValue("@description", Description);

                command.Parameters.AddWithValue("@picture", Picture);
                command.Parameters.AddWithValue("@capacity", Capacity);
                command.Parameters.AddWithValue("@duration", Duration);
                command.Parameters.AddWithValue("@grades", Grades);
                command.Parameters.AddWithValue("@notes", Notes);
                command.Parameters.AddWithValue("@organiser", Organiser);
                command.Parameters.AddWithValue("@type", Type);
                command.Parameters.AddWithValue("@accepted", false);

                command.ExecuteNonQuery();
            }
            connection.Close();
            // END
        }
        // Reset Old ErrorString Value
        ErrorString = string.Empty;
        return RedirectToPage();
    }

    private IActionResult Error(string message)
    {
        ErrorString = message;
        return Page();
    }

    private int PreSetup()
    {
        // Ignore Standard Value
        if (Notes == "Notiz")
            Notes = null!;

        if (Grade7)
            Grades |= 1 << 0;
        if (Grade8)
            Grades |= 1 << 1;
        if (Grade9)
            Grades |= 1 << 2;
        if (Grade10)
            Grades |= 1 << 3;
        if (Grade11)
            Grades |= 1 << 4;
        if (Grade12)
            Grades |= 1 << 5;

        Console.WriteLine($"{Name} {Description} {Organiser} {Capacity} {Duration} {Grades} {Type}");

        if (Name.IsNullOrEmpty() == true || Description.IsNullOrEmpty() == true || Organiser.IsNullOrEmpty() == true || Capacity < 0 || Duration <= 0 || Grades == 0 || Type == "NULL")
        {
            return -1;
        }
        return 0;
    }
}