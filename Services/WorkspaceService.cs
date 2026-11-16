using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Pisco.Data;
using Pisco.Domain;

namespace Pisco.Services;

public class WorkspaceService(IDbContextFactory<PiscoDbContext> dbFactory)
{
    public async Task<List<Workspace>> ListForUserAsync(bool signedIn)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var query = db.Workspaces.AsNoTracking();
        if (!signedIn)
            query = query.Where(w => w.IsPublic);
        return await query.OrderBy(w => w.Name).ToListAsync();
    }

    public Task<List<Workspace>> ListAllAsync(ClaimsPrincipal actor)
    {
        actor.EnsureSignedIn();
        return ListAllAsync();
    }

    public async Task<List<Workspace>> ListAllAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Workspaces.AsNoTracking().OrderBy(w => w.Name).ToListAsync();
    }

    public async Task<Workspace> CreateAsync(string name, bool isPublic = false)
    {
        var ws = new Workspace(name, isPublic);
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Workspaces.Add(ws);
        await db.SaveChangesAsync();
        return ws;
    }

    public Task<Workspace> CreateAsync(string name, bool isPublic, ClaimsPrincipal actor)
    {
        actor.EnsureSignedIn();
        return CreateAsync(name, isPublic);
    }

    public async Task UpdateAsync(long id, string name, bool isPublic, ClaimsPrincipal actor)
    {
        actor.EnsureSignedIn();
        await using var db = await dbFactory.CreateDbContextAsync();
        var ws = await db.Workspaces.FirstOrDefaultAsync(w => w.Id == id) ?? throw new DomainException("The workspace doesn't exist.");
        if (!isPublic)
            (await configAsync(db)).EnsureCanMakePrivate(ws);
        ws.Rename(name);
        ws.SetPublic(isPublic);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id, ClaimsPrincipal actor)
    {
        actor.EnsureSignedIn();
        await using var db = await dbFactory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();
        var ws = await db.Workspaces.FirstOrDefaultAsync(w => w.Id == id) ?? throw new DomainException("The workspace doesn't exist.");
        var config = await configAsync(db);
        config.EnsureCanDelete(ws);
        config.Forget(ws);
        await db.SaveChangesAsync();

        db.Workspaces.Remove(ws);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private static async Task<Config> configAsync(PiscoDbContext db) =>
        await db.Config.FirstOrDefaultAsync() ?? throw new DomainException("The configuration is missing.");
}
