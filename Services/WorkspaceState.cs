using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Npgsql;
using Pisco.Data;
using Pisco.Domain;

namespace Pisco.Services;

public class WorkspaceState(
    WorkspaceService svc,
    NpgsqlDataSource db,
    AuthenticationStateProvider auth,
    PersistentComponentState persisted) : IDisposable
{
    private const string PersistKey = "pisco-workspace";

    private Task<Workspace?>? _loading;
    private PersistingComponentStateSubscription? _persisting;

    public Workspace? Current { get; private set; }

    public event Action? Changed;

    public Task<Workspace?> EnsureLoadedAsync()
    {
        if (_loading is not null)
            return _loading;

        if (persisted.TryTakeFromJson<Workspace>(PersistKey, out var saved) && saved is not null)
        {
            Current = saved;
            return _loading = Task.FromResult<Workspace?>(saved);
        }

        _persisting ??= persisted.RegisterOnPersisting(() =>
        {
            persisted.PersistAsJson(PersistKey, Current);
            return Task.CompletedTask;
        }, RenderMode.InteractiveServer);

        return _loading = LoadAsync();
    }

    public async Task<List<Workspace>> ListAccessibleAsync()
    {
        var (userId, isAdmin) = await UserAsync();
        return await svc.ListForUserAsync(userId, isAdmin);
    }

    public async Task SelectAsync(long workspaceId)
    {
        var ws = (await ListAccessibleAsync()).FirstOrDefault(w => w.Id == workspaceId)
                 ?? throw new DomainException("Domain_Workspace_NotFound");
        await svc.SaveLastWorkspaceAsync((await UserAsync()).UserId, ws.Id);
        Current = ws;
        _loading = Task.FromResult<Workspace?>(ws);
        Changed?.Invoke();
    }

    public async Task ReloadAsync()
    {
        _loading = LoadAsync();
        await _loading;
        Changed?.Invoke();
    }

    public async Task<NpgsqlConnection?> OpenDataAsync() =>
        await EnsureLoadedAsync() is { } ws ? await WorkspaceSchema.OpenAsync(db, ws.SchemaName) : null;

    public void Dispose() => _persisting?.Dispose();

    private async Task<Workspace?> LoadAsync()
    {
        var list = await ListAccessibleAsync();
        var (userId, _) = await UserAsync();
        var last = userId == 0 ? null : await svc.GetLastWorkspaceIdAsync(userId);
        Current = list.FirstOrDefault(w => w.Id == last) ?? list.FirstOrDefault(w => w.IsLanding) ?? list.FirstOrDefault();
        return Current;
    }

    private async Task<(long UserId, bool IsAdmin)> UserAsync()
    {
        var user = (await auth.GetAuthenticationStateAsync()).User;
        return (user.UserId(), user.IsAdmin());
    }
}
