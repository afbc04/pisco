using Dapper;
using Npgsql;
using Pisco.Services;

namespace Pisco.Data;

public static class SeedData
{
    public static async Task SeedAsync(NpgsqlDataSource db, WorkspaceService workspaces, AuthService auth)
    {
        await using var conn = await db.OpenConnectionAsync();

        if (!await conn.ExecuteScalarAsync<bool>("SELECT EXISTS (SELECT 1 FROM core.users)"))
            await auth.CreateAdminAsync("admin", "admin");

        if (!await conn.ExecuteScalarAsync<bool>("SELECT EXISTS (SELECT 1 FROM core.workspaces)"))
            await workspaces.CreateAsync("demo", isDemo: true, isLanding: true);
    }
}
