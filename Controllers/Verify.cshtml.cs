using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers;

public class VerifyModel : PageModel
{
    public VerifyModel()
    {
    }

    public async Task<IActionResult> OnGet()
    {
        // Connect to local SurrealDB
        var db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await db.SignIn(new RootAuth { Username = "root", Password = "root" });
        await db.Use("main", "main");

        if (!HttpContext.Request.Query.ContainsKey("secret")) return RedirectToPage("Index");

        var query = await db.Query(
            "DELETE (RETURN array::at((SELECT id FROM VerificationLinks WHERE secret = type::string(secret)).id, 0));",
            new Dictionary<string, object> { { "secret", HttpContext.Request.Query["secret"] } });

        // Redirect To Profile Again After verifying
        return RedirectToPage("/Profile");
    }
}