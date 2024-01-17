using System.ComponentModel;
using System.Text.Json.Nodes;
using GraphQL.AspNet.Attributes;
using GraphQL.AspNet.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;
using SurrealDb.Net.Models.Response;

public class UserController : GraphController
{

    [Description("Update A User Record")]
    [Authorize(Policy = "AdminOnly")]
    [MutationRoot("setUser")]
    public int setUser(string? username, string? id)
    {
        return -1;
    }

    [Description("Retrieve A User Record From The Database")]
    [Authorize(Policy = "AdminOnly")]
    [QueryRoot("getUser")]
    public User GetUser(string? username, string? id)
    {
        return GetUserDB(username, id).Result;
    }
    public async Task<User> GetUserDB(string? username, string? id)
    {
        // Connect to local SurrealDB
        SurrealDbClient Db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await Db.SignIn(new RootAuth { Username = "root", Password = "root" }).ConfigureAwait(false);
        await Db.Use("main", "main").ConfigureAwait(false);

        SurrealDbResponse? query = null;


        if (!username.IsNullOrEmpty())
        {
            query = await Db.Query($"SELECT * FROM Users WHERE username = type::string($username);", new Dictionary<string, object>{{"username", username!}}).ConfigureAwait(false);
        }
        else if (!id.IsNullOrEmpty())
        {
            var t = new Thing("Users", id!);
            query = await Db.Query($"SELECT * FROM Users WHERE id = type::thing($thing);", new Dictionary<string, object>{{"thing", t}}).ConfigureAwait(false);
        }
        else
        {
            // Error, Can't get data out
        }

        List<User> x = JsonConvert.DeserializeObject<List<User>>(query!.GetValue<JsonArray>(0)!.ToString())!;

        return x[0];
    }
}

public class User
{
    public int Absence { get; set; }
    public int Permissions { get; set; }
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Class { get; set; }
    public string? Masterpassword { get; set; }
};