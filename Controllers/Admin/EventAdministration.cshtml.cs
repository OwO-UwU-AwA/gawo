using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers.Admin;

public class EventAdministrationModel(IAuthorizationService authorizationService) : PageModel
{
    public IAuthorizationService AuthorizationService { get; set; } = authorizationService;

    public async Task<IEnumerable<Event>> Get()
    {
        var secrets = Secrets.Get().Result;
        var db = new SurrealDbClient(Constants.SurrealDbUrl);
        await db.SignIn(new DatabaseAuth
        {
            Namespace = Constants.SurrealDbNameSpace,
            Database = Constants.SurrealDbDatabase,
            Username = secrets.Username,
            Password = secrets.Password
        });

        var res = await db.Select<Event>("Events");

        await db.Invalidate();
        db.Dispose();

        return res;
    }
}