using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Pisco.Domain;

namespace Pisco.Services;

public class WorkspaceState(
    WorkspaceService svc,
    ConfigService config,
    AuthenticationStateProvider auth,
    PersistentComponentState persisted) : IDisposable
{
    private const string persistKey = "pisco-workspace";

    private Task<Workspace?>? loading;
    private PersistingComponentStateSubscription? persisting;

    public Workspace? Current { get; private set; }

    public event Action? Changed;

    public Task<Workspace?> EnsureLoadedAsync()
    {
        if (loading is not null)
            return loading;

        if (persisted.TryTakeFromJson<Workspace>(persistKey, out var saved) && saved is not null)
        {
            Current = saved;
            return loading = Task.FromResult<Workspace?>(saved);
        }

        persisting ??= persisted.RegisterOnPersisting(() =>
        {
            persisted.PersistAsJson(persistKey, Current);
            return Task.CompletedTask;
        }, RenderMode.InteractiveServer);

        return loading = loadAsync();
    }

    public async Task<List<Workspace>> ListAccessibleAsync() =>
        await svc.ListForUserAsync(await signedInAsync());

    public async Task SelectAsync(long workspaceId)
    {
        var ws = (await ListAccessibleAsync()).FirstOrDefault(w => w.Id == workspaceId)
                 ?? throw new DomainException("The workspace doesn't exist.");
        if (await signedInAsync())
            await config.SetLastWorkspaceAsync(ws.Id);
        Current = ws;
        loading = Task.FromResult<Workspace?>(ws);
        Changed?.Invoke();
    }

    public async Task ReloadAsync()
    {
        loading = loadAsync(Current?.Id);
        await loading;
        Changed?.Invoke();
    }

    public void Dispose() => persisting?.Dispose();

    private async Task<Workspace?> loadAsync(long? prefer = null)
    {
        var list = await ListAccessibleAsync();
        var settings = await config.GetAsync();
        var last = await signedInAsync() ? settings?.LastWorkspaceId : null;
        Current = list.FirstOrDefault(w => w.Id == prefer)
                  ?? list.FirstOrDefault(w => w.Id == last)
                  ?? list.FirstOrDefault(w => w.Id == settings?.LandingWorkspaceId)
                  ?? list.FirstOrDefault();
        return Current;
    }

    private async Task<bool> signedInAsync() => (await auth.GetAuthenticationStateAsync()).User.IsSignedIn();
}
