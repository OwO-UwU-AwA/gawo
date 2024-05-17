using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers;

[Authorize]
public class AddEventModel : PageModel
{
    // For Use In Razor 
    public readonly IAuthorizationService AuthorizationService;
    public SurrealDbClient Db;

    public AddEventModel(IAuthorizationService authorizationService)
    {
        AuthorizationService = authorizationService;
        Db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        var secrets = Secrets.Get().Result;
        Db.SignIn(new DatabaseAuth
        {
            Namespace = secrets.Namespace,
            Database = secrets.Database,
            Username = secrets.Username,
            Password = secrets.Password
        });
    }

    [BindProperty] public Event @Event { get; set; } = new();

    // Will Be Converted To Class After Database Retrieval
    [BindProperty] public string Organiser { get; set; } = string.Empty;

    // As Above
    [BindProperty] public string Teacher { get; set; } = string.Empty;

    [BindProperty] public bool Grade5 { get; set; } = false;
    [BindProperty] public bool Grade6 { get; set; } = false;
    [BindProperty] public bool Grade7 { get; set; } = false;
    [BindProperty] public bool Grade8 { get; set; } = false;
    [BindProperty] public bool Grade9 { get; set; } = false;
    [BindProperty] public bool Grade10 { get; set; } = false;
    [BindProperty] public bool Grade11 { get; set; } = false;
    [BindProperty] public bool Grade12 { get; set; } = false;

    public void OnGet()
    {
        Event = new Event();
    }

    public async Task<IActionResult> OnPostAddEvent()
    {
        // Transform Grades Into Bitfield And Set Organiser To Valid Value
        PreSetup();

        await Db.Create<Event>("Events", @Event);

        return RedirectToPage();
    }

    private void PreSetup()
    {
        Event.Grades = (Grade5 ? 1 << 0 : 0) | (Grade6 ? 1 << 1 : 0) | (Grade7 ? 1 << 2 : 0) | (Grade8 ? 1 << 3 : 0) |
                       (Grade9 ? 1 << 4 : 0) | (Grade10 ? 1 << 5 : 0) |
                       (Grade11 ? 1 << 4 : 0) | (Grade12 ? 1 << 6 : 0);
        Thing organiserThing = new Thing(Organiser);
        Thing teacherThing = new Thing(Teacher);

        Event.Organiser = Db.Select<GawoUser>(organiserThing).Result!.Id!;
        Event.Teacher = Db.Select<GawoUser>(teacherThing).Result!.Id!;
    }
}