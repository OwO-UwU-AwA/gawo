using System.Diagnostics;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers;

[Authorize]
public class AddEventModel : PageModel
{
    public readonly IAuthorizationService AuthorizationService;

    /// <summary>
    /// Razor-Accessible <c>SurrealDbClient</c>
    /// </summary>
    public SurrealDbClient Db;

    /// <summary>
    /// Create New Instance Of The Page Model
    /// </summary>
    /// <param name="authorizationService">Verifies Whether The Current User Has The <c>AdminOnly</c> Role</param>
    public AddEventModel(IAuthorizationService authorizationService)
    {
        AuthorizationService = authorizationService;
        Db = new SurrealDbClient(Constants.SurrealDbUrl);
        var secrets = Secrets.Get().Result;
        Db.SignIn(new DatabaseAuth
        {
            Namespace = Constants.SurrealDbNameSpace,
            Database = Constants.SurrealDbDatabase,
            Username = secrets.Username,
            Password = secrets.Password
        });
        // Pre-Initialise Event For Frontend To Use
        Event = new Event();
    }

    /// <summary>
    /// <cref>Event</cref> Object For All Attributes That Can Be Inserted Into The Database Without Any Additional Processing
    /// </summary>
    [BindProperty]
    public Event Event { get; set; }

    /// <summary>
    /// Database ID Of The Organiser As A String
    /// </summary>
    [BindProperty]
    public string Organiser { get; set; } = string.Empty;

    /// <summary>
    /// Database ID Of The Teacher As A String
    /// </summary>
    [BindProperty]
    public string Teacher { get; set; } = string.Empty;

    /// <summary>
    /// Grade Selection Booleans For Each Grade/Checkbox
    /// </summary>
    [BindProperty]
    public bool Grade5 { get; set; }

    /// <summary>
    /// Grade Selection Booleans For Each Grade/Checkbox
    /// </summary>
    [BindProperty]
    public bool Grade6 { get; set; }

    /// <summary>
    /// Grade Selection Booleans For Each Grade/Checkbox
    /// </summary>
    [BindProperty]
    public bool Grade7 { get; set; }

    /// <summary>
    /// Grade Selection Booleans For Each Grade/Checkbox
    /// </summary>
    [BindProperty]
    public bool Grade8 { get; set; }

    /// <summary>
    /// Grade Selection Booleans For Each Grade/Checkbox
    /// </summary>
    [BindProperty]
    public bool Grade9 { get; set; }

    /// <summary>
    /// Grade Selection Booleans For Each Grade/Checkbox
    /// </summary>
    [BindProperty]
    public bool Grade10 { get; set; }

    /// <summary>
    /// Grade Selection Booleans For Each Grade/Checkbox
    /// </summary>
    [BindProperty]
    public bool Grade11 { get; set; }

    /// <summary>
    /// Grade Selection Booleans For Each Grade/Checkbox
    /// </summary>
    [BindProperty]
    public bool Grade12 { get; set; }

    /// <summary>
    /// Creates An Event Entry In The Database
    /// </summary>
    /// <returns><cref>Microsoft.AspNetCore.Mvc.IActionResult</cref></returns>
    public async Task<IActionResult> OnPostAddEvent()
    {
        try
        {
            PreSetup();
            await Db.Create("Events", Event);
        }
        catch (Exception e)
        {
            
            Log.Error("{ExceptionName} {ExceptionDescription} - {ExceptionSource}", e.InnerException?.GetType(),
                e.InnerException?.Message, new StackTrace(e, true).GetFrame(1)?.GetMethod());
            return RedirectToPage("/Error");
        }
        return RedirectToPage();
    }

    public void PreSetup()
    {
        // Encode Grades Into Bitfield
        Event.Grades = (Grade5 ? 1 << 0 : 0) | (Grade6 ? 1 << 1 : 0) | (Grade7 ? 1 << 2 : 0) | (Grade8 ? 1 << 3 : 0) |
                       (Grade9 ? 1 << 4 : 0) | (Grade10 ? 1 << 5 : 0) |
                       (Grade11 ? 1 << 4 : 0) | (Grade12 ? 1 << 6 : 0);

        // Set Organiser And Teacher To Correct Values
        Event.Organiser = new Thing(Organiser);
        Event.Teacher = new Thing(Teacher);

        // Set Even Creation Time To Current Time
        Event.Created = DateTime.Now;

        // Sanitize Description

        var sanitizer = new HtmlSanitizer();
        Event.Description = sanitizer.Sanitize(Event.Description, "http://localhost:5000");
    }
}