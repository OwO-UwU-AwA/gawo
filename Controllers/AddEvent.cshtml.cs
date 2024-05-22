using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers;

// ReSharper disable once InvalidXmlDocComment
/*!
 @file AddEvent.cshtml.cs
 @author Winterer, Mathis Aaron <mrmagic223325@fedora.email>
 @version 1.0
 @brief AddEvent Page Controller
 @includedoc license.html
**/
[Authorize]
public class AddEventModel : PageModel
{
 // For Use In Razor Page
    public readonly IAuthorizationService AuthorizationService;
    public SurrealDbClient Db;

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

    /*!
     @brief Event Object For All Attributes
     That Can Be Inserted Into The Database Without Any Additional Processing
    **/
    [BindProperty] public Event Event { get; set; }

    /*!
     @brief Database ID Of The Organiser As A String
    **/
    [BindProperty] public string Organiser { get; set; } = string.Empty;

    /*!
     @brief Database ID Of The Teacher As A String
    **/
    [BindProperty] public string Teacher { get; set; } = string.Empty;

    /*!
     @defgroup Grades Grades
     @brief Grade Selection Booleans For Each Grade/Checkbox
     @{
    **/
    [BindProperty] public bool Grade5 { get; set; }
    [BindProperty] public bool Grade6 { get; set; }
    [BindProperty] public bool Grade7 { get; set; }
    [BindProperty] public bool Grade8 { get; set; }
    [BindProperty] public bool Grade9 { get; set; }
    [BindProperty] public bool Grade10 { get; set; }
    [BindProperty] public bool Grade11 { get; set; }

    [BindProperty] public bool Grade12 { get; set; }
    /*!
     @}
    **/


    /*!
     @fn OnPostAddEvent()
     @brief Creates An Event Entry In The Database
     @ret Redirect To Current Page
   **/
    public async Task<IActionResult> OnPostAddEvent()
    {
        PreSetup();

        await Db.Create("Events", Event);

        return RedirectToPage();
    }

    /*!
     @fn PreSetup()
     @brief Encode Grades Into Bitfield And Convert Organiser & Teacher IDs To Database @c Things
    **/
    public void PreSetup()
    {
     // Encode Grades Into Bitfield
        Event.Grades = (Grade5 ? 1 << 0 : 0) | (Grade6 ? 1 << 1 : 0) | (Grade7 ? 1 << 2 : 0) | (Grade8 ? 1 << 3 : 0) |
                       (Grade9 ? 1 << 4 : 0) | (Grade10 ? 1 << 5 : 0) |
                       (Grade11 ? 1 << 4 : 0) | (Grade12 ? 1 << 6 : 0);

        Event.Organiser = new Thing(Organiser);
        Event.Teacher = new Thing(Teacher);
    }
}