using System.Security.Claims;
using Pisco.Domain;
using Pisco.Repository.Finance;

namespace Pisco.Services.Finance;

public class WalletManager(WalletRepository wallets)
{
    public event Action? Changed;

    public long TotalBalance(long workspaceId, ClaimsPrincipal actor) =>
        List(workspaceId, actor).Where(w => w.IsPublished).Sum(w => w.InitialBalance);

    public List<Wallet> List(long workspaceId, ClaimsPrincipal actor)
    {
        ensureCanRead(workspaceId, actor);
        return wallets.ByWorkspace(workspaceId);
    }

    public Wallet Get(long workspaceId, int id, ClaimsPrincipal actor)
    {
        ensureCanRead(workspaceId, actor);
        return find(workspaceId, id);
    }

    public Wallet Create(long workspaceId, string name, string color, string icon, long initialBalance, bool active, bool draft, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var wallet = new Wallet(workspaceId, name, color, icon, initialBalance, active, draft);
        ensureNameFree(wallet);
        wallets.Add(wallet.ID, wallet);
        Changed?.Invoke();
        return wallet;
    }

    public void Update(long workspaceId, int id, string name, string color, string icon, long initialBalance, bool active, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var wallet = find(workspaceId, id);
        wallet.Update(name, color, icon, initialBalance);
        if (wallet.Active != active)
            wallet.Active = active;
        ensureNameFree(wallet);
        wallets[wallet.ID] = wallet;
        Changed?.Invoke();
    }

    public void SetDraft(long workspaceId, int id, bool draft, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var wallet = find(workspaceId, id);
        wallet.Status = draft ? WalletStatus.Draft : WalletStatus.Published;
        wallets[wallet.ID] = wallet;
        Changed?.Invoke();
    }

    public void Delete(long workspaceId, int id, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var wallet = find(workspaceId, id);
        wallet.Status = WalletStatus.Deleted;
        wallets[wallet.ID] = wallet;
        Changed?.Invoke();
    }

    public List<Wallet> ListDeleted(long workspaceId, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        return wallets.DeletedByWorkspace(workspaceId);
    }

    public void Restore(long workspaceId, int id, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var wallet = findDeleted(workspaceId, id);
        wallet.Status = WalletStatus.Published;
        ensureNameFree(wallet);
        wallets[wallet.ID] = wallet;
        Changed?.Invoke();
    }

    public void Purge(long workspaceId, int id, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var wallet = findDeleted(workspaceId, id);
        wallets.Remove(wallet.ID);
        Changed?.Invoke();
    }

    private Wallet findDeleted(long workspaceId, int id) =>
        wallets.TryGetValue(id, out var wallet) && wallet.Workspace == workspaceId && wallet.IsDeleted
            ? wallet
            : throw WalletException.NotFound();

    private Wallet find(long workspaceId, int id) =>
        wallets.TryGetValue(id, out var wallet) && wallet.Workspace == workspaceId && !wallet.IsDeleted
            ? wallet
            : throw WalletException.NotFound();

    private void ensureNameFree(Wallet wallet)
    {
        if (wallets.NameExists(wallet.Workspace, wallet.Name, wallet.ID))
            throw WalletException.NameTaken(wallet.Name);
    }

    private void ensureCanRead(long workspaceId, ClaimsPrincipal actor)
    {
        var ws = wallets.FindWorkspace(workspaceId);
        if (ws is null || (!ws.IsPublic && !actor.IsSignedIn()))
            throw new DomainException("The workspace doesn't exist.");
    }

    private void ensureCanWrite(long workspaceId, ClaimsPrincipal actor)
    {
        actor.EnsureSignedIn();
        if (wallets.FindWorkspace(workspaceId) is null)
            throw new DomainException("The workspace doesn't exist.");
    }
}
