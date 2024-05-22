using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers;

public class LoginModel : PageModel
{
    [Required] [BindProperty] public string Username { get; set; } = string.Empty;

    [Required] [BindProperty] public string Password { get; set; } = string.Empty;

    public bool Error { get; set; }

    public string? ReturnUrl { get; set; } = string.Empty;

    public static SurrealDbClient? Db { get; set; }


    // TODO : MAKE THIS FASTER OwO
    public async Task<IActionResult> OnPostLogin()
    {
        try
        {
            // Connect to local SurrealDB
            Db = new SurrealDbClient(Constants.SurrealDbUrl,
                configureJsonSerializerOptions: options => { options.PropertyNameCaseInsensitive = true; });
            var secrets = await Secrets.Get();

            await Db.SignIn(new DatabaseAuth
            {
                Namespace = Constants.SurrealDbNameSpace,
                Database = Constants.SurrealDbDatabase,
                Username = secrets.Username,
                Password = secrets.Password
            });
        }
        catch (Exception e)
        {
            Log.Error("{ExceptionName} {ExceptionDescription} - {ExceptionSource}", e.InnerException?.GetType(),
                e.InnerException?.Message, new StackTrace(e, true).GetFrame(1)?.GetMethod());
            return Page();
        }

        // First Check if Username Exists Then If Password Matches User
        if (await VerifyUsername(Username, Db) == false || await VerifyPassword(Password, Username, Db) == false)
        {
            Error = true;
            return Page();
        }

        Claim[] claims;

        if (await GetPermissions(Username, Db) == 3)
            claims =
            [
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.Role, "Admin")
            ];
        else
            claims =
            [
                new Claim(ClaimTypes.Name, Username)
            ];


        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        ReturnUrl = TempData["ReturnUrl"] as string;
        TempData.Remove("ReturnUrl");

        return Redirect(ReturnUrl ?? "/");
    }

    public void OnGet()
    {
        // TODO: Figure Out If This is Ugly
        // Ugly Solution Please Fix Somehow
        // This Is Read On ≈ Line 82
        ReturnUrl = Request.Query["ReturnUrl"];
        TempData["ReturnUrl"] = ReturnUrl;
    }

    public async Task<int> GetPermissions(string username, SurrealDbClient db)
    {
        // Get Permission Bitfield
        var query = await db.Query(
            $"RETURN array::at((SELECT permissions FROM Users WHERE username = type::string({username})).permissions, 0);");

        return query.GetValue<int>(0);
    }

    public async Task<bool> VerifyPassword(string password, string username, SurrealDbClient db)
    {
        var query = await db.Query(
            $"RETURN array::at((SELECT password FROM Users WHERE username = type::string({username})).password, 0);");

        return BCrypt.Net.BCrypt.Verify(password, query.GetValue<string>(0));
    }

    public async Task<bool> VerifyUsername(string username, SurrealDbClient db)
    {
        var query = await db.Query(
            $"RETURN array::at((SELECT count() FROM Users WHERE username = type::string({username})).count, 0);");

        return query.GetValue<int>(0) > 0;
    }
}