using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;
using Cassandra;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace GaWo.Controllers;

[Authorize]
public class ProfileModel(IAuthorizationService authorizationService) : PageModel
{
    public readonly IAuthorizationService AuthorizationService = authorizationService;

    public required SurrealDbClient Db { get; set; }
    public GawoUser? UserStruct { get; set; }

    [BindProperty] public byte Monday { get; set; }

    [BindProperty] public byte Tuesday { get; set; }

    [BindProperty] public byte Wednesday { get; set; }

    [BindProperty] public byte Thursday { get; set; }

    [BindProperty] public byte Friday { get; set; }

    [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse")]
    [Required(ErrorMessage = "E-Mail-Adresse erforderlich")]
    [BindProperty]
    public string NewEmail { get; set; } = string.Empty;

    [PasswordPropertyText]
    [Required(ErrorMessage = "Passwort erforderlich")]
    [BindProperty]
    public string CurrentPassword { get; set; } = string.Empty;

    [MinLength(8, ErrorMessage = "Passwort muss mindestens 8 Zeichen enthalten")]
    [Required(ErrorMessage = "Passwort erforderlich")]
    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;

    [Compare(nameof(NewPassword), ErrorMessage = "Passwörter stimmen nicht überein")]
    [MinLength(8, ErrorMessage = "Passwort muss mindestens 8 Zeichen enthalten")]
    [Required(ErrorMessage = "Passwort erforderlich")]
    [BindProperty]
    public string NewPasswordConf { get; set; } = string.Empty;

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var db = new SurrealDbClient(Constants.SurrealDbUrl);
            var secrets = await Secrets.Get();

            await db.SignIn(new DatabaseAuth
            {
                Namespace = Constants.SurrealDbNameSpace,
                Database = Constants.SurrealDbDatabase,
                Username = secrets.Username,
                Password = secrets.Password
            });
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            return RedirectToPage("/Error");
        }

        return Page();
    }

    public async Task<GawoUser> FillUser()
    {
        Db = new SurrealDbClient(Constants.SurrealDbUrl);
        var secrets = await Secrets.Get();

        await Db.SignIn(new DatabaseAuth
        {
            Namespace = Constants.SurrealDbNameSpace,
            Database = Constants.SurrealDbDatabase,
            Username = secrets.Username,
            Password = secrets.Password
        });

        var query = await Db.Query(
            $"RETURN array::at((SELECT id FROM Users WHERE username = type::string({HttpContext.User.Identity!.Name!})).id, 0);");

        var thing = new Thing(query.GetValue<string>(0)!);

        return (await Db.Select<GawoUser>(thing))!;
    }

    public async Task<IActionResult> SyncDatabase(GawoUser user)
    {
        var query = await Db.Query(
            $"RETURN array::at((SELECT id FROM Users WHERE username = type::string({user.Username})).id, 0);");

        user.Id = new Thing(query.GetValue<string>(0)!);

        await Db.Upsert(user);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangeEmail()
    {
        EmailValidator validator = new();
        UserStruct = await FillUser();

        ValidationResult result = await validator.ValidateAsync(this);

        if (result.IsValid == false)
        {
            // TODO: Add error message
            return Page();
        }


        TimeUuid secret = TimeUuid.NewId();

        VerificationLink link = new()
        {
            Secret = secret.ToString(),
            Type = "email",
            User = UserStruct.Id!
        };

        await Db.Create("VerificationLinks", link);

        var date = $"{DateTime.Now:dd.MM.yyyyy}";
        var time = $"{DateTime.Now:HH:mm}";

        var htmlContent = new StreamReader(Constants.VerificationEmailHtmlPath).ReadToEndAsync()
            .Result
            .Replace("FIRSTNAME", UserStruct.FirstName).Replace("DATE", date).Replace("TIME", time)
            .Replace("OLDEMAIL", UserStruct.Email).Replace("NEWEMAIL", NewEmail).Replace("SECRET", secret.ToString());

        var plainContent = new StreamReader(Constants.VerificationEmailPlainPath)
            .ReadToEndAsync().Result
            .Replace("FIRSTNAME", UserStruct.FirstName).Replace("DATE", date).Replace("TIME", time)
            .Replace("OLDEMAIL", UserStruct.Email).Replace("NEWEMAIL", NewEmail).Replace("SECRET", secret.ToString());

        AlternateView view = AlternateView.CreateAlternateViewFromString(htmlContent);
        view.ContentType = new System.Net.Mime.ContentType("text/html");


        if (SendNotificationEmail("[NOREPLY] Bestätigen Sie Ihre E-Mail-Adresse", plainContent,
                UserStruct.Email,
                new StringBuilder().AppendFormat($"{UserStruct.FirstName} {UserStruct.LastName}").ToString(), view) !=
            true)
        {
        }

        UserStruct.Email = NewEmail;

        await Db.Upsert(UserStruct);

        return Redirect("/Profile?e");
    }

    // TODO REWRITE THIS IMMEDIATELY
    public async Task<IActionResult> OnPostChangePassword()
    {
        if (NewPassword != NewPasswordConf)
        {
            return Page();
        }

        if (CurrentPassword == NewPassword)
        {
            return Page();
        }

        return Redirect("/Profile?p");

        // Change the password and send an email with a rollback link;
    }

    public bool SendNotificationEmail(string title, string content, string email, string name, AlternateView alt)
    {
        var x = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetValue<string>("SmtpAddress")!
            .Split(":");
        var secrets = Secrets.Get().Result;
        SmtpClient client = new()
        {
            UseDefaultCredentials = true,
            Host = x[0],
            Port = int.Parse(x[1]),
            // TODO: Change this to true in production
            EnableSsl = false,

            Credentials = new NetworkCredential("noreply@gauss-gymnasium.de", secrets.EmailPassword)
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
            Priority = MailPriority.High
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

    public class EmailValidator : AbstractValidator<ProfileModel>
    {
        public EmailValidator()
        {
            RuleFor(x => x.NewEmail).NotNull().EmailAddress().WithMessage("Ungültige E-Mail-Adresse.");
        }
    }

    public class PasswordValidator : AbstractValidator<ProfileModel>
    {
        public PasswordValidator()
        {
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
                .WithMessage("Passwort muss mindestens 8 Zeichen lang sein").NotEqual(x => x.CurrentPassword)
                .WithMessage("Neues Passwort darf nicht dem alten entsprechen.").Equal(x => x.NewPasswordConf)
                .WithMessage("Passwörter stimmen nicht überein.");
        }
    }
}