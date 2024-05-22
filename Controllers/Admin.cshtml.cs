using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;

namespace GaWo.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminModel : PageModel
{
    public SurrealDbClient? Db { get; set; }
    public int NotAccepted { get; set; }


    public async void OnGet()
    {
        var secrets = await Secrets.Get();
        Db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await Db.SignIn(new DatabaseAuth
        {
            Namespace = Constants.SurrealDbNameSpace,
            Database = Constants.SurrealDbDatabase,
            Username = secrets.Username,
            Password = secrets.Password
        });
    }
}