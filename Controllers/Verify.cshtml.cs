using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers;

public class VerifyModel : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        // Connect to local SurrealDB
        var db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await db.SignIn(new RootAuth { Username = "root", Password = "root" });
        await db.Use("main", "main");

        if (!HttpContext.Request.Query.ContainsKey("secret")) RedirectToPage("Index");

        try
        {
            await db.Query(
                "DELETE (RETURN array::at((SELECT id FROM VerificationLinks WHERE secret = type::string(secret)).id, 0));",
                new Dictionary<string, object> { { "secret", HttpContext.Request.Query["secret"] } });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return RedirectToPage("/Profile");
    }
}