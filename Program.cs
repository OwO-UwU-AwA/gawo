using Microsoft.AspNetCore.Authentication.Cookies;
using gawo;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.Cookie.Name = "AuthCookie";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Error";
});

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Run();
