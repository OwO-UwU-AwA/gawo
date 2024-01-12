using HotChocolate.Execution;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.Cookie.Name = "AuthCookie";
        options.ExpireTimeSpan = TimeSpan.FromHours(3);
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Error";
});

builder.Services.AddAuthorization();

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromHours(3);
    options.Cookie.IsEssential = true;
    options.Cookie.Domain = "gauss-gymnasium.de/gawo";
});

builder.Services.AddRazorPages();

builder.Services.AddGraphQLServer().AddQueryType<QueryType>();

var app = builder.Build();

app.UseRouting();

app.MapGraphQL("/Api");
app.MapGraphQLPlayground("/Playground");

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Run();