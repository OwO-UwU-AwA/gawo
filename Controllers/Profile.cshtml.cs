using System.Data.SQLite;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;

using Database;

namespace gawo.Pages;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly ILogger<ProfileModel> _logger;

    public (bool, string) Error { get; set; } = (false, string.Empty);

    [BindProperty]
    public string CurrentPassword { get; set; } = string.Empty;
    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;
    [BindProperty]
    public string NewPasswordConf { get; set; } = string.Empty;
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

            using var connection = new SQLiteConnection(configuration.GetConnectionString("GawoDbContext"));
            connection.Open();
            string query = "SELECT first_name,last_name,username,email,class,is_teacher,is_admin,is_guest,absent,password FROM users WHERE username = @username";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                FirstName = reader.GetString(0);
                LastName = reader.GetString(1);
                UserName = reader.GetString(2);
                Email = reader.GetString(3);
                Class = reader.GetValue(4).ToString();
                Class = Class.IsNullOrEmpty() ? "keine" : Class;
                Permissions = Convert.ToByte((reader.GetInt16(5) == 1 ? (0 | (1 << 0)) : Permissions) | (reader.GetInt16(6) == 1 ? (0 | (1 << 1)) : Permissions) | (reader.GetInt16(7) == 1 ? (0 | (1 << 2)) : Permissions));
                Absence = Convert.ToByte(reader.GetInt16(8));
                Password = reader.GetString(9);
            }
        }
    }
    public ProfileModel(ILogger<ProfileModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        UserStruct = new GawoUser(HttpContext.User.Identity.Name);
    }

    public async Task<IActionResult> SyncDatabase(GawoUser user)
    {
        // Connect to local SurrealDB
        Db = new SurrealDbClient("ws://127.0.0.1:8000/rpc", configureJsonSerializerOptions: options => {
            options.PropertyNamingPolicy = new Database.LowerCaseNamingPolicy();
        });
        await Db.SignIn(new RootAuth { Username = "root", Password = "root" });
        await Db.Use("main", "main");

        var query = await Db.Query($"RETURN array::at((SELECT id FROM Users WHERE username = type::string($username)).id, 0);", new Dictionary<string, object>{{ "username", user.UserName }});

        user.Id = new Thing(query.GetValue<string>(0));

	    await Db.Upsert(user);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangeEmail()
    {
        GawoUser user = new(HttpContext.User.Identity.Name);

        // Disregard Nonsensical Change
        if (!user.Email.Equals(NewEmail))
        {
            user.Email = NewEmail;
            await SyncDatabase(user);
        }
        return RedirectToPage();
    }

    // TODO REWRITE THIS IMMEDIATELY
    public IActionResult OnPostChangePassword()
    {
        if (NewPassword == CurrentPassword)
        {
            Error = (true, "Passwort nicht geändert.");
            return Page();
        }
        return RedirectToPage();
    }

    public void SendNotificationEmail(string title, string content, string email, string name)
    {
        SmtpClient client = new(host: "localhost", port: 1025)
        {
            UseDefaultCredentials = true
        };

        MailAddress from = new(address: "gawo@gauss-gymnasium.de", displayName: "GAWO-Team");
        MailAddress to = new(email, name);

        MailMessage message = new(from, to)
        {
            Subject = title,
            SubjectEncoding = Encoding.UTF8,

            Body = content,
            BodyEncoding = Encoding.UTF8,

            IsBodyHtml = true
        };

        client.Send(message);
    }

    public async Task<IActionResult> OnPostChangeAbsence()
    {
        UserStruct = new GawoUser(HttpContext.User.Identity.Name);
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

        await SyncDatabase(user);
        
        return RedirectToPage();
    }
}
