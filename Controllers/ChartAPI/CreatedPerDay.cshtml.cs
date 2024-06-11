using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers.ChartAPI;

public class CreatedPerDayModel : PageModel
{
    public async Task<ActionResult> OnGet()
    {
        if (HttpContext.Request.Headers.From != "Code")
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

        var query = await db.Query($"RETURN (SELECT created FROM Events).created");

        var res = query.GetValue<List<DateTime>>(0);

        await db.Invalidate();
        db.Dispose();

        var days = new int[5];

        foreach (var day in res!)
        {
            switch (day.DayOfWeek)
            {
                case DayOfWeek.Monday:
                {
                    days[0]++;
                    break;
                }
                case DayOfWeek.Tuesday:
                {
                    days[1]++;
                    break;
                }
                case DayOfWeek.Wednesday:
                {
                    days[2]++;
                    break;
                }
                case DayOfWeek.Thursday:
                {
                    days[3]++;
                    break;
                }
                case DayOfWeek.Friday:
                {
                    days[4]++;
                    break;
                }
            }
        }

        return new JsonResult(new int[] { days[0], days[1], days[2], days[3], days[4] });
    }
}