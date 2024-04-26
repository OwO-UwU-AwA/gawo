using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;
using Cassandra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
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
        UserStruct = await FillUser();
        if (UserStruct.Email == NewEmail)
        {
            Error = (true, "Neue E-Mail-Adresse gleicht aktueller.");
            return Page();
        }

        TimeUuid secret = TimeUuid.NewId();

        VerificationLink link = new()
        {
            Secret = secret.ToString(),
            Type = "email",
            User = UserStruct.Id!
        };

        // Connect to local SurrealDB
        var db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await db.SignIn(new RootAuth { Username = "root", Password = "root" });
        await db.Use("main", "main");

        await db.Create<VerificationLink>("VerificationLinks", link);

        var htmlContent = new StringBuilder();
        var plainContent = new StringBuilder();

        htmlContent.AppendLine($"Sehr geehrte/r {UserStruct.FirstName}, <br></br>");
        htmlContent.AppendLine(
            $"Ihre E-Mail-Adresse wurde am <code>{DateTime.Now:dd.MM.yyyy}</code> um <code>{DateTime.Now:HH:mm}</code> von <code style=\"color: #FF6961;\">{UserStruct.Email}</code> auf <code style=\"color: #FF6961;\">{NewEmail}</code> geändert.<br>");
        htmlContent.AppendLine(
            $"Bestätigen Sie diese Änderung unter <a style=\"color: #A1B8FB;\" href=\"http://localhost:5000/Verify?secret={secret}\">diesem Link</a>.<br>");
        htmlContent.AppendLine(
            "Falls Sie diese Änderung nicht veranlasst haben, kontaktieren Sie das <a style=\"color: #A1B8FB;\" href=\"mailto:gawo@gauss-gymnasium.de\">GaWo-Team</a> bitte umgehend.<br></br>");
        htmlContent.AppendLine("Mit freundlichen Grüßen, <br></br> Das GaWo-Team.");

        plainContent.AppendLine($"Sehr geehrte/r {UserStruct.FirstName},");
        plainContent.AppendLine();
        plainContent.AppendLine(
            $"Ihre E-Mail-Adresse wurde am {DateTime.Now:dd.MM.yyyy} um {DateTime.Now:HH:mm} von `{UserStruct.Email}` auf `{NewEmail}` geändert.");
        plainContent.AppendLine();
        plainContent.AppendLine($"Bestätigen Sie diese Änderung unter http://localhost:5000/Verify?secret={secret} .");
        plainContent.AppendLine(
            "Falls Sie diese Änderung nicht veranlasst haben, kontaktieren Sie gawo@gauss-gymnasium.de bitte umgehend.");
        plainContent.AppendLine();
        plainContent.AppendLine();
        plainContent.AppendLine("Mit freundlichen Grüßen,");
        plainContent.AppendLine();
        plainContent.AppendLine();
        plainContent.AppendLine("Das GaWo-Team.");

        AlternateView view = AlternateView.CreateAlternateViewFromString(htmlContent.ToString());
        view.ContentType = new System.Net.Mime.ContentType("text/html");


        if (SendNotificationEmail("[NOREPLY] Bestätigen Sie Ihre E-Mail-Adresse", plainContent.ToString(),
                UserStruct.Email,
                UserStruct.FirstName + " " + UserStruct.LastName, view) != true)
        {
            Error = (true, "Ein Fehler ist bei dem Senden der E-Mail aufgetreten.");
            return Page();
        }

        UserStruct.Email = NewEmail;

        await db.Upsert<GawoUser>(UserStruct);

        return Redirect("/Profile?e");
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

        if (NewPassword.Length >= 8 && NewPassword.Any(char.IsDigit) && NewPassword.Any(char.IsUpper) &&
            NewPassword.Any(char.IsLower)) return Redirect("/Profile?p");
        Error = (true,
            "Passwort muss <b>mindestens</b> 8 Zeichen, eine Ziffer und einen Groß- und Kleinbuchstaben enthalten.");
        return Page();

        // Change the password and send an email with a rollback link;
    }

    public bool SendNotificationEmail(string title, string content, string email, string name, AlternateView alt)
    {
        var x = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetValue<string>("SmtpAddress")!
            .Split(":");
        SmtpClient client = new()
        {
            UseDefaultCredentials = true,
            Host = x[0],
            Port = int.Parse(x[1]),
            // TODO: Change this to true in production
            EnableSsl = false,
            Credentials = new NetworkCredential("noreply@gauss-gymnasium.de", "gawo2024"),
        };

        MailAddress from = new("noreply@gauss-gymnasium.de", "[NOREPLY]");
        MailAddress to = new(email, name);

        MailMessage message = new(from, to)
        {
            Subject = title,
            SubjectEncoding = Encoding.UTF8,

            Body = content,
            BodyEncoding = Encoding.UTF8,

            HeadersEncoding = Encoding.UTF8,
            IsBodyHtml = true,
            AlternateViews = { alt },
            Priority = MailPriority.High,
        };

        try
        {
            client.Send(message);
        }
        catch (Exception e)
        {
            Log.Error("{ExceptionName} {ExceptionDescription} - {ExceptionSource}", e.InnerException?.GetType(),
                e.InnerException?.Message, new StackTrace(e, true).GetFrame(1)?.GetMethod());
            return false;
        }

        return true;
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