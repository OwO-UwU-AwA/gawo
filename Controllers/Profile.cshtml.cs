using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers;

[Authorize]
public class ProfileModel : PageModel
{
    public GawoUser? UserStruct { get; set; }

    public (bool, string) Error { get; set; } = (false, string.Empty);

    [BindProperty] public string CurrentPassword { get; set; } = string.Empty;

    // (New, Confirmation)
    [BindProperty] public string NewPassword { get; set; } = string.Empty;

    [BindProperty] public string NewPasswordConf { get; set; } = string.Empty;

    [BindProperty] public string NewEmail { get; set; } = string.Empty;

    [BindProperty] public byte Monday { get; set; }

    [BindProperty] public byte Tuesday { get; set; } = 0;

    [BindProperty] public byte Wednesday { get; set; } = 0;

    [BindProperty] public byte Thursday { get; set; } = 0;

    [BindProperty] public byte Friday { get; set; } = 0;

    public void OnGet()
    {
    }

    public async Task<GawoUser> FillUser()
    {
        var db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await db.SignIn(new RootAuth { Username = "root", Password = "root" });
        await db.Use("main", "main");

        var query = await db.Query(
            "RETURN array::at((SELECT id FROM Users WHERE username = type::string($username)).id, 0);",
            new Dictionary<string, object> { { "username", HttpContext.User.Identity!.Name! } });

        var thing = new Thing(query.GetValue<string>(0)!);

        return (await db.Select<GawoUser>(thing))!;
    }

    public async Task<IActionResult> SyncDatabase(GawoUser user)
    {
        // Connect to local SurrealDB
        var db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await db.SignIn(new RootAuth { Username = "root", Password = "root" });
        await db.Use("main", "main");

        var query = await db.Query(
            "RETURN array::at((SELECT id FROM Users WHERE username = type::string($username)).id, 0);",
            new Dictionary<string, object> { { "username", user.Username } });

        user.Id = new Thing(query.GetValue<string>(0)!);

        await db.Upsert(user);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangeEmail()
    {
        return RedirectToPage();
    }

    // TODO REWRITE THIS IMMEDIATELY
    public async Task<IActionResult> OnPostChangePassword()
    {

        if (NewPassword != NewPasswordConf)
        {
            Error = (true, "Passwörter stimmen nicht miteinander überein.");
            return Page();
        }

        if (CurrentPassword == NewPassword)
        {
            Error = (true, "Neues Passwort identisch zu derzeitigem Passwort.");
            return Page();
        }

        if (NewPassword.Length < 8 || !NewPassword.Any(char.IsDigit) || !NewPassword.Any(char.IsUpper) || !NewPassword.Any(char.IsLower))
        {
            Error = (true, "Passwort muss <b>mindestens</b> 8 Zeichen, eine Ziffer und einen Groß- und Kleinbuchstaben enthalten.");
            return Page();
        }
        
        // Change the password and send an email with a rollback link;

        return RedirectToPage();
    }
    
    public void SendNotificationEmail(string title, string content, string email, string name)
    {
        SmtpClient client = new("localhost", 1025)
        {
            UseDefaultCredentials = true
        };

        MailAddress from = new("gawo@gauss-gymnasium.de", "GAWO-Team");
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
        byte bitfield = 0;

        if (Monday != 0) bitfield |= 1 << 0;
        if (Tuesday != 0) bitfield |= 1 << 1;
        if (Wednesday != 0) bitfield |= 1 << 2;
        if (Thursday != 0) bitfield |= 1 << 3;
        if (Friday != 0) bitfield |= 1 << 4;

        UserStruct = await FillUser();
        UserStruct.Absence = bitfield;

        await SyncDatabase(UserStruct);

        return RedirectToPage();
    }
}