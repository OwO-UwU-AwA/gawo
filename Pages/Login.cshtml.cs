using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;
using System.Security.Claims;

namespace gawo.Pages;

public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string ValidationErrorMessage { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;

    public LoginModel(ILogger<LoginModel> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> OnPostLogin()
    {
        // First Check if Username Exists; Then If Password Matches User
        if (VerifyUsername(Username) == false || VerifyPassword(Password, Username) == false)
        {
            ValidationErrorMessage = "Benutzername Oder Passwort Inkorrekt.";
            return Page();
        }
        else
        {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, Username!)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        
        ReturnUrl = (TempData["ReturnUrl"] as string)!;
        TempData.Remove("ReturnUrl");

        if (ReturnUrl != null)
            return Redirect(ReturnUrl);
        return Redirect("/");
        }
    }

    public void OnGet()
    {
        // Ugly Solution Please Fix
        ReturnUrl = Request.Query["ReturnUrl"]!;
        TempData["ReturnUrl"] = ReturnUrl;
    }

    public bool VerifyPassword(string password, string username)
    {
        string? hashedPassword = null;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        using (var connection = new SQLiteConnection(configuration.GetConnectionString("GawoDbContext")))
        {
            connection.Open();
            string query = "SELECT password FROM users WHERE username = @username";
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

    public bool VerifyUsername(string username)
    {
        bool x = true;
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        using (var connection = new SQLiteConnection(configuration.GetConnectionString("GawoDbContext")))
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