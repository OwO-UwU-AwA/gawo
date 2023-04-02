using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;

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
    public string Date { get; set; } = string.Empty;

    public enum Rooms {
        NULL,
        SPORT,
        UG13,
        UG14,
        UG15,
        UG16,
        UG17,
        UG18,
        UG19,
        UG20,
        UG21,
        UG22,
        UG23,
        UG24,
        UG25,
        UG26,
        UG27,
        UG28,
        UG29,
        UG30,
        UG31,
        UG32,
        UG33,
        UG34,
        UG35,
    };
    [BindProperty]
    public Rooms Room { get; set; } = Rooms.NULL;
    [BindProperty]
    public byte[]? Picture { get; set; } = (byte[])null!;
    [BindProperty]
    public int Capacity { get; set; } = -1;
    [BindProperty]
    public int Duration { get; set; } = -1;
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
    [BindProperty]
    public string Notes { get; set; } = string.Empty;
    [BindProperty]
    public string Organiser { get; set; } = string.Empty;
    [BindProperty]
    public string CoOrganisers { get; set; } = string.Empty;
    [BindProperty]
    public string Type { get; set; } = string.Empty;
    [BindProperty]
    public int Teacher { get; set; } = -1;
    public string ErrorString { get; set; } = string.Empty;

    public IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

    // Really Ugly Way To Store Grades
    private string Grades = string.Empty;
    public AddEventModel(ILogger<AddEventModel> logger)
    
    {
        _logger = logger;
    }

    public void OnGet()
    {        

    }

    public IActionResult OnPostAddEvent()
    {
        // Execute Pre-Setup-Steps And Check For Errors
        if (PreSetup() == -1)
            return Error("Unvollständige Angaben (Standardwert Oder NULL)");
        
        // Verify All CoOrganisers
        var tmp = CoOrganisersCheck();
        if (tmp.Item1 == -1)
            return Error($"{tmp.Item2} Ist Kein Benutzername.");


        using (var connection = new SQLiteConnection(configuration.GetConnectionString("GawoDbContext")))
        {
            
            // Remove Trailing Comma After We Assure That We Have Valid Input (Grades Could Be NULL)
            Grades = Grades.Substring(0, Grades.Length - 1);

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
            
            try
            {
                // SQL Returns The Username If The User Doesnt Exist
                // Check If We Got An ID Or A Username
                int.Parse(Organiser);
            }
            catch
            {
                // If We Got A FormatException Return Error
                return Error("Organisator/Benutzer Mit Diesem Namen Existiert Nicht.");
            }

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
            query = "INSERT INTO events (id, name, description, date, room, picture, capacity, duration, grades, notes, organiser, type) VALUES (@id, @name, @description, @date, @room, @picture, @capacity, @duration, @grades, @notes, @organiser, @type)";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@name", Name);
                command.Parameters.AddWithValue("@description", Description);
                command.Parameters.AddWithValue("@date", Date);

                // Get The Actual Name Not The Index Of The Enum Item (Actually Pretty Cool That This Works)
                command.Parameters.AddWithValue("@room", Room.ToString());
                command.Parameters.AddWithValue("@picture", Picture);
                command.Parameters.AddWithValue("@capacity", Capacity);
                command.Parameters.AddWithValue("@duration", Duration);
                command.Parameters.AddWithValue("@grades", Grades);
                command.Parameters.AddWithValue("@notes", Notes);
                command.Parameters.AddWithValue("@organiser", Organiser);
                command.Parameters.AddWithValue("@type", Type);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Do Nothing I Guess??
                        // OK
                    }
                }
            }
            connection.Close();
            // END

            if (CoOrganisers != null)
            {
                foreach (string username in CoOrganisers.Split(','))
                {
                    int user_id = -1;
                    
                    // Get User ID
                    // BEGIN
                    connection.Open();
                    query = "SELECT id FROM users WHERE username = @username";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user_id = reader.GetInt16(0);
                            }
                        }
                    }
                    connection.Close();
                    // END

                    // Can Skip Checks Here Because We Verified CoOrganisers Before

                    // Insert Formatted Data Into `co_organisers` Table
                    // BEGIN
                    connection.Open();
                    query = "INSERT INTO co_organisers (event_id, user_id) VALUES (@event_id, @user_id)";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@event_id", id);
                        command.Parameters.AddWithValue("@user_id", user_id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Do Nothing I Guess??
                                // OK
                            }
                        }
                    }
                    connection.Close();
                    // END
                }
            }
        }
        // Reset Old ErrorStrign Value
        ErrorString = string.Empty;
        return Redirect("/AddEvent");
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
            Grades += "7,";
        if (Grade8)
            Grades += "8,";
        if (Grade9)
            Grades += "9,";
        if (Grade10)
            Grades += "10,";
        if (Grade11)
            Grades += "11,";
        if (Grade12)
            Grades += "12,";

        if (Name == null || Description == null || Organiser == null || Date == null || Room == Rooms.NULL || Capacity <= 0 || Duration <= 0 || Grades == string.Empty || Type == "NULL")
        {
            return -1;
        }
        return 0;
    }
    private (int, string) CoOrganisersCheck()
    {
        using (var connection = new SQLiteConnection(configuration.GetConnectionString("GawoDbContext")))
        {
            // Verify CoOrganiser Before We Create The Event
            if (CoOrganisers != null)
            {
                foreach (string username in CoOrganisers.Split(','))
                {
                    int user_id = -1;
                    
                    // Get User ID
                    // BEGIN
                    connection.Open();
                    string _query = "SELECT id FROM users WHERE username = @username";
                    using (var command = new SQLiteCommand(_query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user_id = reader.GetInt16(0);
                            }
                        }
                    }
                    connection.Close();
                    // END

                    if (user_id == -1)
                    {
                        return (-1, username);
                    }
                }
            }
        }
        return (0, "");
    }
}