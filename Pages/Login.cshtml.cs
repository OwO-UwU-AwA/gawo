using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;
using System.Security.Claims;

namespace gawo.Pages;

public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;

    [BindProperty]
    public string? Username { get; set; }

    [BindProperty]
    public string? Password { get; set; }

    private readonly GawoDbContext _dbContext;

    public LoginModel(ILogger<LoginModel> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Login()
    {
        // First Check if Username Exists; Then If Password Matches User
        if (VerifyUsername(Username) == false)
        {
            // Error
            ModelState.AddModelError("", "INVALID");
            return Page();
        }
        if (VerifyPassword(Password, Username) == false)
        {
            // Error
            ModelState.AddModelError("", "INVALID");
            return Page();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, Username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Redirect("/Profile");
    }

    public void OnGet()
    {        
    }

    public bool VerifyPassword(string? password, string? username)
    {
        string? hashedPassword = null;

        using (var connection = new SQLiteConnection("Data Source=/home/fedora/Programming/gawo/users.db"))
        {
            connection.Open();
            string query = "SELECT password_hash FROM users WHERE username = @username";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@username", username);
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
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    public bool VerifyUsername(string? username)
    {
        bool x = true;
        using (var connection = new SQLiteConnection("Data Source=/home/fedora/Programming/gawo/users.db"))
        {
            connection.Open();
            string query = "SELECT COUNT(*) FROM users WHERE username = @username";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@username", username);
                int count = Convert.ToInt32(command.ExecuteScalar());
                if (count == 0)
                    x = false;
            }
            connection.Close();
        }
        return x;
    }
}