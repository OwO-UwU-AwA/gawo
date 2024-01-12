using System.Data;
using System.Text.Json.Nodes;
using HotChocolate.Types;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;
using SurrealDb.Net.Models.Response;

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

public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Field(t => t.Absence).Type<NonNullType<IntType>>().Description("Bit field which stores the days on which a user will be absent: 000F TWTM (Friday, Thursday, Wednesday, Tuesday, Monday)");
        descriptor.Field(t => t.Permissions).Type<NonNullType<IntType>>().Description("Bit field which manages the users permissions: 0000 0GAT (Guest, Admin, Teacher)");
        descriptor.Field(t => t.Firstname).Type<NonNullType<StringType>>().Description("First name of the user");
        descriptor.Field(t => t.Lastname).Type<NonNullType<StringType>>().Description("Surname of the user");
        descriptor.Field(t => t.Username).Type<NonNullType<StringType>>().Description("Username used to log into the website");
        descriptor.Field(t => t.Email).Type<NonNullType<StringType>>().Description("Email address that the user has set");
        descriptor.Field(t => t.Password).Type<NonNullType<StringType>>().Description("Hashed password of the user");
        descriptor.Field(t => t.Class).Type<NonNullType<StringType>>().Description("Name of the users class or 'keine' for non-student users");
        descriptor.Field(t => t.Masterpassword).Type<NonNullType<StringType>>().Description("Master password of the user");

    }
}

public class Query
{
    public User GetUserById(string id)
    {
        return GetUser(null, id).Result;
    }

    public User GetUserByUsername(string username)
    {
        return GetUser(username, null).Result;
    }

    public async Task<User> GetUser(string? username, string? id)
    {
        // Connect to local SurrealDB
        SurrealDbClient Db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await Db.SignIn(new RootAuth { Username = "root", Password = "root" });
        await Db.Use("main", "main");

        SurrealDbResponse? query = null;


        if (!username.IsNullOrEmpty())
        {
            query = await Db.Query($"SELECT * FROM Users WHERE username = '{username}';");
        }
        else if (!id.IsNullOrEmpty())
        {
            var t = new Thing("Users", id!);
            query = await Db.Query($"SELECT * FROM Users WHERE id = '{t}';");
        }
        else
        {
            // Error, Can't get data out
        }

        List<User> x = JsonConvert.DeserializeObject<List<User>>(query!.GetValue<JsonArray>(0)!.ToString())!;

        return x[0];

    }
}

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor.Field(q => q.GetUserById(default)).Argument("id", a => a.Type<NonNullType<StringType>>());
        descriptor.Field(q => q.GetUserByUsername(default!)).Argument("username", a => a.Type<NonNullType<StringType>>());
    }
}