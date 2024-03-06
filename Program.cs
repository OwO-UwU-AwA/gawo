using GraphQL.AspNet.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.ResourceDetectors.Container;
using OpenTelemetry.ResourceDetectors.Host;
using OpenTelemetry.Logs;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

Action<ResourceBuilder> appResourceBuilder = resource => resource.AddDetector(new ContainerResourceDetector()).AddDetector(new HostDetector());

builder.Services.AddOpenTelemetry().ConfigureResource(appResourceBuilder).WithTracing(tracerBuilder => tracerBuilder.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddGrpcClientInstrumentation().AddOtlpExporter());

builder.Services.AddOpenTelemetry().ConfigureResource(appResourceBuilder).WithMetrics(meterBuilder => meterBuilder.AddProcessInstrumentation().AddRuntimeInstrumentation().AddAspNetCoreInstrumentation().AddOtlpExporter().AddPrometheusExporter());

builder.Logging.AddOpenTelemetry(options => options.AddOtlpExporter());

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.Cookie.Name = "AuthCookie";
        options.ExpireTimeSpan = TimeSpan.FromHours(3);
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Error";
        }
    );

builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminOnly", policy => {
        policy.RequireRole("Admin");
    });
});

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromHours(3);
    options.Cookie.IsEssential = true;
    options.Cookie.Domain = "gauss-gymnasium.de/gawo";
});

builder.Services.AddRazorPages();

builder.Services.AddGraphQL(options => {
    options.AuthorizationOptions.Method = GraphQL.AspNet.Security.AuthorizationMethod.PerRequest;
});

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseRouting();

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

app.UseGraphQL();

await app.RunAsync();
