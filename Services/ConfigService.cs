using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Pisco.Data;
using Pisco.Domain;

namespace Pisco.Services;

public class ConfigService(IDbContextFactory<PiscoDbContext> dbFactory)
{
    public async Task<Config?> GetAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Config.AsNoTracking().FirstOrDefaultAsync();
    }

    public async Task EnsureCreatedAsync(string username, string password)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var config = await db.Config.FirstOrDefaultAsync();
        if (config is null)
        {
            config = new Config(username, password);
            db.Config.Add(config);
        }
        if (config.LandingWorkspaceId is null && await db.Workspaces.OrderBy(w => w.Id).FirstOrDefaultAsync(w => w.IsPublic) is { } landing)
            config.SetLanding(landing);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ValidateCredentialsAsync(string username, string password) =>
        await GetAsync() is { } config && config.CheckCredentials(username, password);

    public Task SetLastWorkspaceAsync(long? workspaceId) => updateAsync((_, config) => config.SetLastWorkspace(workspaceId));

    public Task SetLandingAsync(long workspaceId, ClaimsPrincipal actor)
    {
        actor.EnsureSignedIn();
        return updateAsync(async (db, config) =>
        {
            var ws = await db.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId) ?? throw new DomainException("Domain_Workspace_NotFound");
            config.SetLanding(ws);
        });
    }

    private Task updateAsync(Action<PiscoDbContext, Config> change) =>
        updateAsync((db, config) =>
        {
            change(db, config);
            return Task.CompletedTask;
        });

    private async Task updateAsync(Func<PiscoDbContext, Config, Task> change)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var config = await db.Config.FirstOrDefaultAsync() ?? throw new DomainException("Domain_Config_Missing");
        await change(db, config);
        await db.SaveChangesAsync();
    }
}
