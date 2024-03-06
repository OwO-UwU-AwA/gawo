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
        var Db = new SurrealDbClient("ws://127.0.0.1:8000/rpc", configureJsonSerializerOptions: options => {
            options.PropertyNameCaseInsensitive = true;
        });
        await Db.SignIn(new RootAuth { Username = "root", Password = "root" });
        await Db.Use("main", "main");

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
            // TODO: Error, Can't get data out
        }

        List<User> x = JsonConvert.DeserializeObject<List<User>>(query!.GetValue<JsonArray>(0)!.ToString())!;

        return x[0];
    }
}

public class User
{
    public int? Absence { get; set; }
    public int Permissions { get; set; }
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Class { get; set; }
    public string? Masterpassword { get; set; }
};

public class UserDB : Record
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