using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers.ChartAPI;

public class PerDayModel : PageModel
{
    public async Task<ActionResult> OnGet()
    {
        if (HttpContext.Request.Headers["From"] != "Code")
            return RedirectPermanent("/");

        var secrets = await Secrets.Get();

        var db = new SurrealDbClient(Constants.SurrealDbUrl);

        await db.SignIn(new DatabaseAuth
        {
            Namespace = Constants.SurrealDbNameSpace,
            Database = Constants.SurrealDbDatabase,
            Username = secrets.Username,
            Password = secrets.Password
        });

        var query = await db.Query($"RETURN (SELECT type FROM Events).type");

        var res = query.GetValue<List<string>>(0);

        await db.Invalidate();
        db.Dispose();

        string[] values =
        [
            "PRESENTATION", "GPRESENTATION", "FLANGPRESENTATION", "THESISDEF", "COMPETITION", "WORKSHOP", "QF", "SPORT",
            "ELMOS", "OTHER"
        ];

        /*
           res.Count(x => x == value)) returns the number of elements inside res that are equal to value where value is one of each of the values from values.Select(value => ....) returning an array of numbers
         */
        var array = values.Select(value => res!.Count(x => x == value)).ToArray();

        return new JsonResult(array);
    }
}