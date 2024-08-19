using System.Globalization;
using System.IO.Compression;
using FluentValidation;
using GaWo;
using GaWo.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.File.GzArchive;
using RollingInterval = Serilog.Sinks.FileEx.RollingInterval;

var builder = WebApplication.CreateBuilder(args);

// Needed If Running In A Docker Container Because Encryption Keys Are Not Persistent Otherwise
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("temp-keys")).UseCryptographicAlgorithms(
    new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });


// Add Cookie-Based Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
        {
            options.Cookie.Name = "AuthCookie";
            // Change This As Required To Invalidate Authentication Cookie Earlier Or Later
            options.ExpireTimeSpan = TimeSpan.FromHours(3);
            // This Cookie Is Required For Any Kind Of Authentication
            options.Cookie.IsEssential = true;
            options.LoginPath = "/Login";
            options.LogoutPath = "/Logout";
            options.AccessDeniedPath = "/";
        }
    );


// Add Authorisation Role To Allow Marking Entire Pages As AdminOnly
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireAuthenticatedUser();
    });

// Add Session-Based Authentication
builder.Services.AddSession(options =>
{
    // Change This As Required To Invalidate The Session Earlier Or Later
    options.IdleTimeout = TimeSpan.FromHours(3);
    options.Cookie.IsEssential = true;
    // Change This To Match The Actual Domain
    options.Cookie.Domain = Constants.Url;
});

// Register FluentValidation Validators
builder.Services.AddTransient<IValidator<ProfileModel>, ProfileModel.EmailValidator>();
builder.Services.AddTransient<IValidator<ProfileModel>, ProfileModel.PasswordValidator>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
});

var app = builder.Build();

app.UseRouting();

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

await using var log = new LoggerConfiguration().MinimumLevel.Debug().Enrich.WithExceptionDetails().Enrich.FromLogContext().WriteTo.FileEx(Constants.LogFilePath,
    rollingInterval: RollingInterval.Hour,
    hooks: new FileArchiveRollingHooks(CompressionLevel.SmallestSize, targetDirectory: Constants.ArchivedLogFilePath),
    retainedFileCountLimit: 1).CreateLogger();

// Create Global Logger
Log.Logger = log;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("de-DE");

Log.Information("Started Meow");

// Will Block As Long As The Application Is Running
await app.RunAsync();

// Must Be Called Before The Application Ends
await Log.CloseAndFlushAsync();