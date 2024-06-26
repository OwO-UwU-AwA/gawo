using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers.Admin;

public class EventAdministrationModel(IAuthorizationService authorizationService) : PageModel
{
    public async Task<IEnumerable<string>> GetName(Thing id)
    {
        try
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

            var res = await db.Query(
                $"RETURN object::values(array::at((SELECT firstname, lastname FROM Users WHERE id = {id}), 0))");

            var ret = res.GetValue<List<string>>(0);
            return ret;
        }
        catch (Exception e)
        {
            Log.Error("{ExceptionName} {ExceptionDescription} - {ExceptionSource}", e.InnerException?.GetType(),
                e.InnerException?.Message, new StackTrace(e, true).GetFrame(1)?.GetMethod());
            return [null!, null!];
        }
    }

    public async Task<IEnumerable<Event>> Get()
    {
        try
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

            return res;
        }
        catch (Exception e)
        {
            Log.Error("{ExceptionName} {ExceptionDescription} - {ExceptionSource}", e.InnerException?.GetType(),
                e.InnerException?.Message, new StackTrace(e, true).GetFrame(1)?.GetMethod());
            return null!;
        }
    }

    public async Task<GawoUser> GetUser(Thing thing)
    {
        try
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

            var res = await db.Select<GawoUser>(thing);

            return res!;
        }
        catch (Exception e)
        {
            Log.Error("{ExceptionName} {ExceptionDescription} - {ExceptionSource}", e.InnerException?.GetType(),
                e.InnerException?.Message, new StackTrace(e, true).GetFrame(1)?.GetMethod());
            return null!;
        }
    }
}