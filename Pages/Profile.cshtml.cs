using System.Data.SQLite;
using System.Net.Mail;
using System.Runtime.ExceptionServices;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

    public string ValidationErrorMessage = string.Empty;
    public ProfileModel(ILogger<ProfileModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {        
    }

    public IActionResult OnPostChangeEmail()
    {
        if (NewEmail == null)
            return Page();

        string OldEmail = string.Empty;
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        using (var connection = new SQLiteConnection(configuration.GetConnectionString("GawoDbContext")))
        {
            connection.Open();
            string query = "SELECT email FROM users WHERE username = @username";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@username", User.Identity!.Name);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        OldEmail = reader.GetString(0);
                    }
                }
            }
            connection.Close();
        }
        if (NewEmail == OldEmail)
        {
            ValidationErrorMessage = "Email Identisch.";
            return Page();
        }
        using (var connection = new SQLiteConnection(configuration.GetConnectionString("GawoDbContext")))
        {
            connection.Open();
            string query = "UPDATE users SET email = @email WHERE username = @username";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@email", NewEmail);
                command.Parameters.AddWithValue("@username", User.Identity.Name);
                command.ExecuteNonQuery();
            }
            connection.Close();
            ValidationErrorMessage = string.Empty;
        }
        return Page();
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
                sendNotificationEmail("GAWO Passwortänderung", "", email!, last + ", " + first);
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

        message.Body = "<b>Ihr GAWO-Passwort wurde geändert!<br>Falls Sie dies nicht waren, wenden Sie sich an das <a href=\"mailto:gawo@gauss-gymnasium.de?subject=Ungewöhnliche Kontoaktivitäten\">GAWO-Team</a>, andernfalls können Sie diese E-Mail ignorieren.</b>";
        message.BodyEncoding = Encoding.UTF8;

        message.IsBodyHtml = true;

        client.Send(message);
    }
}