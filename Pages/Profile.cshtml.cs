using System.Data.SQLite;
using System.Net.Mail;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;

namespace gawo.Pages;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly ILogger<ProfileModel> _logger;

    [BindProperty]
    public string CurrentPassword { get; set; } = string.Empty;
    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;
    [BindProperty]
    public string NewPasswordConf { get; set; } = string.Empty;
    [BindProperty]
    public string CurrentEmail { get; set; } = string.Empty;
    [BindProperty]
    public string NewEmail { get; set; } = string.Empty;
    [BindProperty]
    public string Monday { get; set; } = string.Empty;
    [BindProperty]
    public string Tuesday { get; set; } = string.Empty;
    [BindProperty]
    public string Wednesday { get; set; } = string.Empty;
    [BindProperty]
    public string Thursday { get; set; } = string.Empty;
    [BindProperty]
    public string Friday { get; set; } = string.Empty;
    public GawoUser? UserStruct { get; set; }
    public static SurrealDbClient? Db { get; set; }

    public class GawoUser : Record
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MasterPassword { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        /*
            Teacher: 0000 0001
            Admin:   0000 0010
            Guest:   0000 0100

            Teacher+Admin: 0000 0011
        */
        public byte Permissions { get; set; } = 0;

        public byte Absence { get; set; } = 0;

        public GawoUser(string username)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            using (var connection = new SQLiteConnection(configuration.GetConnectionString("GawoDbContext")))
            {
                connection.Open();
                string query = "SELECT first_name,last_name,username,email,class,is_teacher,is_admin,is_guest,absent,password FROM users WHERE username = @username";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            FirstName = reader.GetString(0);
                            LastName = reader.GetString(1);
                            UserName = reader.GetString(2);
                            Email = reader.GetString(3);
                            Class = reader.GetValue(4).ToString()!;
                            Class = Class.IsNullOrEmpty() ? "keine" : Class;
                            Permissions = Convert.ToByte((reader.GetInt16(5) == 1 ? (0 | (1 << 0)) : Permissions) | (reader.GetInt16(6) == 1 ? (0 | (1 << 1)) : Permissions) | (reader.GetInt16(7) == 1 ? (0 | (1 << 2)) : Permissions));
                            Absence = Convert.ToByte(reader.GetInt16(8));
                            Password = reader.GetString(9);
                        }
                    }
                }
            }
        }
    }
    public string ValidationErrorMessage = string.Empty;
    public ProfileModel(ILogger<ProfileModel> logger)
    {
        _logger = logger;
    }

    public async void OnGet()
    {
        UserStruct = new GawoUser(HttpContext.User.Identity!.Name!);
        // Connect to local SurrealDB
        Db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await Db.SignIn(new RootAuth { Username = "root", Password = "root" });
        await Db.Use("main", "main");
    }

    public async Task<IActionResult> SyncDatabase(GawoUser user, string username)
    {
        var query = await Db!.Query($"SELECT id FROM Users WHERE username = '{username}';");

        string id = JsonConvert.DeserializeObject<List<dynamic>>(query.GetValue<JsonValue>(0)!.ToJsonString())![0].id;

        await Db!.Query($"UPDATE Users SET firstname = '{user.FirstName}', lastname = '{user.LastName}', masterpassword = '{user.MasterPassword}', password = '{user.Password}', username = '{user.UserName}', email = '{user.Email}', class = '{user.Class}', permissions = {user.Permissions}, absence = {user.Absence} WHERE id = '{id}';");

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangeEmail()
    {
        GawoUser user = new GawoUser(HttpContext.User.Identity!.Name!);

        // Disregard nonsensical change
        if (!CurrentEmail.Equals(NewEmail) && CurrentEmail.Equals(user.Email))
        {
            user.Email = NewEmail;
            await SyncDatabase(user, user.UserName);
        }
        return RedirectToPage();
    }

    public IActionResult OnPostChangePassword()
    {
        if (NewPassword == null)
            return Page();

        if (NewPassword != NewPasswordConf)
        {
            ValidationErrorMessage = "Passwörter Stimmen Nicht Überein.";
            return Page();
        }

        ValidationErrorMessage = string.Empty;
        string? hashedPassword = null;
        string? email = null;
        string? first = null;
        string? last = null;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        using (var connection = new SQLiteConnection(configuration.GetConnectionString("GawoDbContext")))
        {
            connection.Open();
            string query = "SELECT password,email,first_name,last_name FROM users WHERE username = @username";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@username", User.Identity!.Name);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hashedPassword = reader.GetString(0);
                        email = reader.GetString(1);
                        first = reader.GetString(2);
                        last = reader.GetString(3);
                    }
                }
            }
            connection.Close();
        }
        if (BCrypt.Net.BCrypt.Verify(CurrentPassword, hashedPassword))
        {
            if (NewPassword == NewPasswordConf)
                NewPassword = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            else
            {
                ValidationErrorMessage = "Passwörter Stimmen Nicht überein.";
                return Page();
            }
            using (var connection = new SQLiteConnection(configuration.GetConnectionString("GawoDbContext")))
            {
                connection.Open();
                string query = "UPDATE users SET password = @hash WHERE username = @username";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@hash", NewPassword);
                    command.Parameters.AddWithValue("@username", User.Identity.Name);
                    command.ExecuteNonQuery();
                }
                connection.Close();
                ValidationErrorMessage = string.Empty;
                sendNotificationEmail("GAWO Passwortänderung", "<b>Ihr GAWO-Passwort wurde geändert!<br>Falls Sie dies nicht waren, wenden Sie sich an das <a href=\"mailto:gawo@gauss-gymnasium.de?subject=Ungewöhnliche Kontoaktivitäten\">GAWO-Team</a>, andernfalls können Sie diese E-Mail ignorieren.</b>", email!, last + ", " + first);
                return Page();
            }
        }
        else
        {
            ValidationErrorMessage = "Falsches Passwort.";
        }
        return Page();
    }

    public void sendNotificationEmail(string title, string content, string email, string name)
    {
        SmtpClient client = new SmtpClient("localhost", 1025);
        client.UseDefaultCredentials = true;

        MailAddress from = new MailAddress("gawo@gauss-gymnasium.de", "GAWO-Team");
        MailAddress to = new MailAddress(email, name);

        MailMessage message = new MailMessage(from, to);

        message.Subject = title;
        message.SubjectEncoding = Encoding.UTF8;

        message.Body = content;
        message.BodyEncoding = Encoding.UTF8;

        message.IsBodyHtml = true;

        client.Send(message);
    }

    public async Task<IActionResult> OnPostChangeAbsence()
    {
        UserStruct = new GawoUser(HttpContext.User.Identity!.Name!);
        byte bitfield = 0;

        if (!Monday.IsNullOrEmpty())
        {
            bitfield |= (1 << 0);
        }
        if (!Tuesday.IsNullOrEmpty())
        {
            bitfield |= (1 << 1);
        }
        if (!Wednesday.IsNullOrEmpty())
        {
            bitfield |= (1 << 2);
        }
        if (!Thursday.IsNullOrEmpty())
        {
            bitfield |= (1 << 3);
        }
        if (!Friday.IsNullOrEmpty())
        {
            bitfield |= (1 << 4);
        }

        GawoUser user = UserStruct;
        user.Absence = bitfield;

        await SyncDatabase(user, user.UserName);
        
        return RedirectToPage();
    }
}
