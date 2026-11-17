using System.Globalization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pisco.Components;
using Pisco.Data;
using Pisco.Repository.Finance;
using Pisco.Services;
using Pisco.Services.Finance;

foreach (var line in File.Exists(".env") ? File.ReadAllLines(".env") : [])
    if (line.Split('=', 2) is [var key, var value] && !key.StartsWith('#') && Environment.GetEnvironmentVariable(key.Trim()) is null)
        Environment.SetEnvironmentVariable(key.Trim(), value.Trim());

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddNpgsqlDataSource(new NpgsqlConnectionStringBuilder
{
    Host = builder.Configuration["POSTGRES_HOST"],
    Port = int.Parse(builder.Configuration["POSTGRES_PORT"] ?? "5432"),
    Database = builder.Configuration["POSTGRES_DB"],
    Username = builder.Configuration["POSTGRES_USER"],
    Password = builder.Configuration["POSTGRES_PASSWORD"],
}.ConnectionString);
builder.Services.AddDbContextFactory<PiscoDbContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.Redirect(AppRoutes.Login(ctx.Request.Path + ctx.Request.QueryString));
            return Task.CompletedTask;
        };
        o.Events.OnRedirectToAccessDenied = o.Events.OnRedirectToLogin;
        o.Events.OnValidatePrincipal = async ctx =>
        {
            var config = await ctx.HttpContext.RequestServices.GetRequiredService<ConfigService>().GetAsync();
            if (config is null || config.Username != ctx.Principal!.Identity?.Name)
            {
                ctx.RejectPrincipal();
                await ctx.HttpContext.SignOutAsync();
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<WorkspaceService>();
builder.Services.AddScoped<WorkspaceState>();
builder.Services.AddScoped<WalletRepository>();
builder.Services.AddScoped<WalletManager>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CategoryManager>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddSingleton<AdminPresence>();
builder.Services.AddScoped<PresenceCircuit>();
builder.Services.AddScoped<CircuitHandler>(sp => sp.GetRequiredService<PresenceCircuit>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    await DbSchema.EnsureCreatedAsync(sp.GetRequiredService<NpgsqlDataSource>());
    await SeedData.SeedAsync(sp.GetRequiredService<WorkspaceService>(), sp.GetRequiredService<ConfigService>());
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache",
});
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapFallback(() => Results.Redirect("/"));

var auth = app.MapGroup("/auth");

auth.MapPost("/login", async (HttpContext http, AuthService authSvc, IAntiforgery antiforgery) =>
{
    var form = await http.Request.ReadFormAsync();
    var returnUrl = AppRoutes.LocalOrHome(form[AppRoutes.ReturnUrlParam]);
    var ok = await antiforgery.IsRequestValidAsync(http)
             && await authSvc.SignInAsync(http, form["username"].ToString(), form["password"].ToString());
    return Results.LocalRedirect(ok ? returnUrl : AppRoutes.Login(returnUrl, failed: true));
});

auth.MapPost("/logout", async (HttpContext http) =>
{
    await http.SignOutAsync();
    return Results.Redirect("/");
}).DisableAntiforgery();

app.Run();
