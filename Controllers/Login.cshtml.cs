using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;
using Microsoft.IdentityModel.Tokens;

namespace gawo.Pages;

public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public bool Error { get; set; } = false;

    public string ReturnUrl { get; set; } = string.Empty;

    public static SurrealDbClient? Db { get; set; }

    public LoginModel(ILogger<LoginModel> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> OnPostLogin()
    {
        // Connect to local SurrealDB
        Db = new SurrealDbClient("ws://127.0.0.1:8000/rpc", configureJsonSerializerOptions: options => {
            options.PropertyNameCaseInsensitive = true;
        });
        await Db.SignIn(new RootAuth { Username = "root", Password = "root" });
        await Db.Use("main", "main");

        // First Check if Username Exists; Then If Password Matches User
        if (await VerifyUsername(Username, Db) == false || await VerifyPassword(Password, Username, Db) == false)
        {
            Error = true;
            return Page();
        }
        else
        {

        Claim[] claims;

        if (await GetPermissions(Username, Db) == 3)
        {
            claims = new[] {
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.Role, "Admin")
            };
        }
        else
        {
            claims = new[] {
                new Claim(ClaimTypes.Name, Username),
            };
        }

        

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        ReturnUrl = (TempData["ReturnUrl"] as string);
        TempData.Remove("ReturnUrl");

        if (ReturnUrl != null)
            return Redirect(ReturnUrl);
        return Redirect("/");
        }
    }

    public void OnGet()
    {
        // Ugly Solution Please Fix Somehow
        // This Is Read On ≈ Line 67
        ReturnUrl = Request.Query["ReturnUrl"];
        TempData["ReturnUrl"] = ReturnUrl;
    }

    public async Task<int> GetPermissions(string username, SurrealDbClient Db)
    {
        // Get Permission Bitfield
        var query = await Db.Query("RETURN array::at((SELECT permissions FROM Users WHERE username = type::string($username)).permissions, 0);", new Dictionary<string, object>{{ "username", username }});

        return query.GetValue<int>(0);
    }

    public async Task<bool> VerifyPassword(string password, string username, SurrealDbClient Db)
    {
        var query = await Db.Query("RETURN array::at((SELECT password FROM Users WHERE username = type::string($username)).password, 0);", new Dictionary<string, object>{{ "username", username }});

        return BCrypt.Net.BCrypt.Verify(password, query.GetValue<string>(0));
    }

    public async Task<bool> VerifyUsername(string username, SurrealDbClient Db)
    {
        var query = await Db.Query("RETURN array::at((SELECT count() FROM Users WHERE username = type::string($username)).count, 0);", new Dictionary<string, object>{{ "username", username }});

        return query.GetValue<int?>(0) > 0;
    }
}