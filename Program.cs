using Dapper;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Npgsql;
using Pisco.Components;
using Pisco.Data;
using Pisco.Services;

foreach (var line in File.Exists(".env") ? File.ReadAllLines(".env") : [])
    if (line.Split('=', 2) is [var key, var value] && !key.StartsWith('#') && Environment.GetEnvironmentVariable(key.Trim()) is null)
        Environment.SetEnvironmentVariable(key.Trim(), value.Trim());

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

var cultures = new[] { "pt", "en" };

DefaultTypeMap.MatchNamesWithUnderscores = true;
builder.Services.AddNpgsqlDataSource(new NpgsqlConnectionStringBuilder
{
    Host = builder.Configuration["POSTGRES_HOST"],
    Port = int.Parse(builder.Configuration["POSTGRES_PORT"] ?? "5432"),
    Database = builder.Configuration["POSTGRES_DB"],
    Username = builder.Configuration["POSTGRES_USER"],
    Password = builder.Configuration["POSTGRES_PASSWORD"],
}.ConnectionString);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/login";
        o.AccessDeniedPath = "/login";
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        o.SlidingExpiration = true;
        o.Cookie.Name = "Pisco.Auth";
        o.Events.OnValidatePrincipal = async ctx =>
        {
            var user = await ctx.HttpContext.RequestServices.GetRequiredService<AuthService>()
                .GetUserAsync(ctx.Principal!.UserId());
            if (user is null || user.Username != ctx.Principal!.Identity?.Name || user.IsAdmin != ctx.Principal!.IsAdmin())
            {
                ctx.RejectPrincipal();
                await ctx.HttpContext.SignOutAsync();
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<WorkspaceService>();
builder.Services.AddScoped<WorkspaceState>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<NpgsqlDataSource>();
    await CoreDatabase.EnsureCoreIsInitializatedAsync(db);

    var workspaces = sp.GetRequiredService<WorkspaceService>();
    await SeedData.SeedAsync(db, workspaces, sp.GetRequiredService<AuthService>());

    await using var conn = await db.OpenConnectionAsync();
    foreach (var ws in await workspaces.ListAllAsync())
        await WorkspaceSchema.CreateAsync(conn, ws.SchemaName);
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

app.UseRequestLocalization(o => o
    .SetDefaultCulture("pt")
    .AddSupportedCultures(cultures)
    .AddSupportedUICultures(cultures));
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapGet("/culture/set", (string culture, string? redirectUri, HttpContext http) =>
{
    if (cultures.Contains(culture))
        http.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
    return Results.LocalRedirect(string.IsNullOrWhiteSpace(redirectUri) ? "/" : redirectUri);
});

var auth = app.MapGroup("/auth");

auth.MapPost("/login", async (HttpContext http, AuthService authSvc, IAntiforgery antiforgery) =>
{
    if (!await antiforgery.IsRequestValidAsync(http))
        return Results.Redirect("/login?error=1");

    var form = await http.Request.ReadFormAsync();
    var returnUrl = form["returnUrl"].ToString();

    var user = await authSvc.ValidateCredentialsAsync(form["username"].ToString(), form["password"].ToString());
    if (user is null)
    {
        var back = string.IsNullOrEmpty(returnUrl) ? "" : $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
        return Results.Redirect($"/login?error=1{back}");
    }

    await authSvc.SignInAsync(http, user);
    return Results.LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
});

auth.MapPost("/logout", async (HttpContext http) =>
{
    await http.SignOutAsync();
    return Results.Redirect("/");
}).DisableAntiforgery();

app.Run();
