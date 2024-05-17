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
        var secrets = await Secrets.Get();
        await db.SignIn(new DatabaseAuth
        {
            Namespace = secrets.Namespace,
            Database = secrets.Database,
            Username = secrets.Username,
            Password = secrets.Password
        });
        await db.Use("main", "main");

        if (!HttpContext.Request.Query.ContainsKey("secret")) RedirectToPage("Index");

        try
        {
            await db.Query(
                $"DELETE (RETURN array::at((SELECT id FROM VerificationLinks WHERE secret = type::string({HttpContext.Request.Query["secret"].ToString()})).id, 0));");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return RedirectToPage("/Profile");
    }
}