using System.Data.SQLite;
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

    public string ValidationErrorMessage = string.Empty;
    public ProfileModel(ILogger<ProfileModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {        
        return;
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

        using (var connection = new SQLiteConnection("Data Source=/home/fedora/Programming/gawo/users.db"))
        {
            connection.Open();
            string query = "SELECT password_hash FROM users WHERE username = @username";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@username", User.Identity!.Name);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hashedPassword = reader.GetString(0);
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
            using (var connection = new SQLiteConnection("Data Source=/home/fedora/Programming/gawo/users.db"))
            {
                connection.Open();
                string query = "UPDATE users SET password_hash = @hash WHERE username = @username";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@hash", NewPassword);
                    command.Parameters.AddWithValue("@username", User.Identity.Name);
                    command.ExecuteNonQuery();
                }
                connection.Close();
                ValidationErrorMessage = string.Empty;
                return Page();
            }
        }
        else
        {
            ValidationErrorMessage = "Falsches Passwort.";
        }
        return Page();
    }
}