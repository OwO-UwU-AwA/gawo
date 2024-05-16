using FluentValidation;
using GaWo.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.ResourceDetectors.Container;
using OpenTelemetry.ResourceDetectors.Host;
using OpenTelemetry.Logs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Serilog;
using Serilog.Exceptions;

var builder = WebApplication.CreateBuilder(args);

///////// BEGIN PROMETHEUS METRICS

Action<ResourceBuilder> appResourceBuilder = resource =>
    resource.AddDetector(new ContainerResourceDetector()).AddDetector(new HostDetector());


builder.Services.AddOpenTelemetry().ConfigureResource(appResourceBuilder).WithTracing(tracerBuilder =>
    tracerBuilder.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddGrpcClientInstrumentation()
        .AddOtlpExporter());

builder.Services.AddOpenTelemetry().ConfigureResource(appResourceBuilder).WithMetrics(meterBuilder =>
    meterBuilder.AddProcessInstrumentation().AddRuntimeInstrumentation().AddAspNetCoreInstrumentation()
        .AddOtlpExporter().AddPrometheusExporter());

builder.Logging.AddOpenTelemetry(options => options.AddOtlpExporter());

///////// END PROMETHEUS METRICS


///////// BEGIN AUTH

// Needed If Running In A Docker Container
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("temp-keys")).UseCryptographicAlgorithms(
    new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
        {
            options.Cookie.Name = "AuthCookie";
            // Change This As Required
            options.ExpireTimeSpan = TimeSpan.FromHours(3);
            // This Cookie Is Required For Any Kind Of Authentication
            options.Cookie.IsEssential = true;
            options.LoginPath = "/Login";
            options.LogoutPath = "/Logout";
            options.AccessDeniedPath = "/Error";
        }
    );

builder.Services.AddAuthorizationBuilder()
    // Allows Marking Entire Pages As AdminOnly
    .AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireAuthenticatedUser();
    });

builder.Services.AddSession(options =>
{
    // Change This As Required
    options.IdleTimeout = TimeSpan.FromHours(3);
    options.Cookie.IsEssential = true;
    // Change This To Match The Actual Domain
    options.Cookie.Domain = "gauss-gymnasium.de/gawo";
});

///////// END AUTH

builder.Services.AddTransient<IValidator<ProfileModel>, ProfileModel.EmailValidator>();
builder.Services.AddTransient<IValidator<ProfileModel>, ProfileModel.PasswordValidator>();

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

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

await using var log = new LoggerConfiguration().Enrich.WithExceptionDetails().WriteTo.Console().CreateLogger();

Log.Logger = log;

Log.Information("Started Meow");

var timer = new Timer(state =>
{
    // Reset Email changes after 1 hour
}, null, TimeSpan.Zero, TimeSpan.FromHours(1));

await app.RunAsync();