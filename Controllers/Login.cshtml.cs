using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers;

public class LoginModel(IAuthorizationService authorizationService) : PageModel
{
    public readonly IAuthorizationService AuthorizationService = authorizationService;

    [Required] [BindProperty] public string Username { get; set; } = string.Empty;

    [Required] [BindProperty] public string Password { get; set; } = string.Empty;

    public string Error { get; set; } = "";
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
            Log.Error($"{e}");
            return Page();
        }
        
        // First Check if Username Exists Then If Password Matches User
        if (await VerifyUsername(Username, Db) == false || await VerifyPassword(Password, Username, Db) == false)
        {
            Error = "Anmeldung fehlgeschlagen. Falsches Passwort oder falscher Benutzername.";
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

    public async void OnGetAsync()
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

    private string GenPassword(string password)
    {
        byte[] salt;
        RandomNumberGenerator.Create().GetBytes(salt = new byte[16]);

        // Create the Rfc2898DeriveBytes instance
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);
        
        // Get the hash
        byte[] hash = pbkdf2.GetBytes(20);

        // Combine salt and hash
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);
        
        return Convert.ToBase64String(hashBytes);
    }

    public async Task<bool> VerifyPassword(string password, string username, SurrealDbClient db)
    {
        var query = await db.Query(
            $"RETURN array::at((SELECT password FROM Users WHERE username = type::string({username})).password, 0);");
        
        return VerifyPassword(query.GetValue<string>(0)!, password);
    }
    
    public static bool VerifyPassword(string savedPasswordHash, string passwordToVerify)
    {
        // Extract the bytes
        byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
        
        // Get the salt
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        // Create the Rfc2898DeriveBytes instance with the same salt and iterations
        var pbkdf2 = new Rfc2898DeriveBytes(passwordToVerify, salt, 10000, HashAlgorithmName.SHA512);
        
        // Get the hash of the input password
        byte[] hash = pbkdf2.GetBytes(20);

        // Compare the results
        for (int i = 0; i < 20; i++)
        {
            if (hashBytes[i + 16] != hash[i])
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> VerifyUsername(string username, SurrealDbClient db)
    {
        var query = await db.Query(
            $"RETURN array::at((SELECT count() FROM Users WHERE username = type::string({username})).count, 0);");

        var res = query.GetValue<int?>(0) ?? -1;

        return res > 0;
    }
}