using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers;

public class VerifyModel : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        // Check If Parameter Is In The Query, If Not Redirect To Index
        if (!HttpContext.Request.Query.ContainsKey("secret"))
            return Redirect($"/Error?referrer={HttpContext.Request.Path}{HttpContext.Request.QueryString}");

        try
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
            

            // Delete Entry From VerificationLinks Which Matches The Secret
            await db.Query(
                $"DELETE (RETURN array::at((SELECT id FROM VerificationLinks WHERE secret = type::string({HttpContext.Request.Query["secret"].ToString()})).id, 0));");
            
            // Delete Database Object And Invalidate The Connection
            await db.Invalidate();
        }
        catch (Exception e)
        {
            Log.Error($"{e}");
            return RedirectToPage("/Error");
        }
        // Redirect Back To Profile After Verifying Email
        return RedirectToPage("/Profile");
    }
}