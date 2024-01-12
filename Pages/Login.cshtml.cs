using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Newtonsoft.Json;

using SurrealDb.Net.Models.Auth;
using SurrealDb.Net;

using System.Text.Json.Nodes;

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
        if (await VerifyUsername(Username) == false || await VerifyPassword(Password, Username) == false)
        {
            ValidationErrorMessage = "Benutzername Oder Passwort Inkorrekt.";
            return Page();
        }
        else
        {

        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.Name, Username)
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

    public async Task<bool> VerifyPassword(string password, string username)
    {
        // Connect to local SurrealDB
        var db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await db.SignIn(new RootAuth { Username = "root", Password = "root" });
        await db.Use("main", "main");

        var query = await db.Query($"SELECT password FROM Users WHERE username = '{username}';");

        List<dynamic> x = JsonConvert.DeserializeObject<List<dynamic>>(query.GetValue<JsonValue>(0)!.ToJsonString())!;

        return BCrypt.Net.BCrypt.Verify(password, x[0].password.ToString());
    }

    public async Task<bool> VerifyUsername(string username)
    {
        // Connect to local SurrealDB
        var db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await db.SignIn(new RootAuth { Username = "root", Password = "root" });
        await db.Use("main", "main");
        
        // Returns a JSON array:
        /*
        [
            {
                "count": <>
            }
        ]
        */
        var query = await db.Query($"SELECT count() FROM Users WHERE username = '{username}';");

        List<dynamic> x = JsonConvert.DeserializeObject<List<dynamic>>(query.GetValue<JsonArray>(0)!.ToString())!;

        return x[0].count > 0;
    }
}
