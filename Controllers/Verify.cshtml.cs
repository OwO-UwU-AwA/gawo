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
        var db = new SurrealDbClient(Constants.SurrealDbUrl);
        var secrets = await Secrets.Get();
        await db.SignIn(new DatabaseAuth
        {
            Namespace = Constants.SurrealDbNameSpace,
            Database = Constants.SurrealDbDatabase,
            Username = secrets.Username,
            Password = secrets.Password
        });

        // Check if parameter is in the query, if not redirect to index
        if (!HttpContext.Request.Query.ContainsKey("secret")) RedirectToPage("Index");

        try
        {
            // Delete entry from VerificationLinks which matches the secret
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