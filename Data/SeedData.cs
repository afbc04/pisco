using Pisco.Services;

namespace Pisco.Data;

public static class SeedData
{
    public static async Task SeedAsync(WorkspaceService workspaces, ConfigService config)
    {
        if ((await workspaces.ListAllAsync()).Count == 0)
            await workspaces.CreateAsync("demo", isPublic: true);

        await config.EnsureCreatedAsync("admin", "admin");
    }
}
