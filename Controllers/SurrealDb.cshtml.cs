using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers;

[Authorize]
public class SurrealDbModel(IAuthorizationService authorization) : PageModel
{

    [BindProperty] public string Query { get; set; }
    [BindProperty] public string Result { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        if (!(await authorization.AuthorizeAsync(User, "AdminOnly")).Succeeded == true) 
            return RedirectToPage("Index");
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await db.SignIn(new RootAuth { Password = "root", Username = "root" });
        await db.Use("main", "main");
        await db.Query(Query);
        return Page();
    }
}
