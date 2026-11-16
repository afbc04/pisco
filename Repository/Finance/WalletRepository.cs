using System.Collections;
using Microsoft.EntityFrameworkCore;
using Pisco.Data;
using Pisco.Domain;

namespace Pisco.Repository.Finance;

public class WalletRepository(IDbContextFactory<PiscoDbContext> dbFactory) : IDictionary<int, Wallet>
{
    public Wallet this[int key]
    {
        get => find(key) ?? throw new KeyNotFoundException($"Wallet {key} not found.");
        set => put(key, value);
    }

    public ICollection<int> Keys => run(db => db.Wallets.Select(w => w.ID).ToList());

    public ICollection<Wallet> Values => run(db => db.Wallets.AsNoTracking().ToList());

    public int Count => run(db => db.Wallets.Count());

    public bool IsReadOnly => false;

    public void Add(int key, Wallet value)
    {
        if (key != 0 && ContainsKey(key))
            throw new ArgumentException($"Wallet {key} already exists.", nameof(key));
        put(key, value);
    }

    public void Add(KeyValuePair<int, Wallet> item) => Add(item.Key, item.Value);

    public bool ContainsKey(int key) => run(db => db.Wallets.Any(w => w.ID == key));

    public bool Contains(KeyValuePair<int, Wallet> item) =>
        item.Value.ID == item.Key && ContainsKey(item.Key);

    public bool TryGetValue(int key, out Wallet value)
    {
        var wallet = find(key);
        value = wallet!;
        return wallet is not null;
    }

    public bool Remove(int key) => run(db => db.Wallets.Where(w => w.ID == key).ExecuteDelete()) > 0;

    public bool Remove(KeyValuePair<int, Wallet> item) => Contains(item) && Remove(item.Key);

    public void Clear() => run(db => db.Wallets.ExecuteDelete());

    public void CopyTo(KeyValuePair<int, Wallet>[] array, int arrayIndex) =>
        pairs().CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<int, Wallet>> GetEnumerator() => pairs().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public List<Wallet> ByWorkspace(long workspaceId) =>
        run(db => db.Wallets.AsNoTracking()
            .Where(w => w.Workspace == workspaceId && w.Status != WalletStatus.Deleted)
            .OrderBy(w => w.Name)
            .ToList());

    public List<Wallet> DeletedByWorkspace(long workspaceId) =>
        run(db => db.Wallets.AsNoTracking()
            .Where(w => w.Workspace == workspaceId && w.Status == WalletStatus.Deleted)
            .OrderByDescending(w => w.DeletedDate)
            .ThenBy(w => w.Name)
            .ToList());

    public bool NameExists(long workspaceId, string name, int exceptId)
    {
        var lower = name.ToLower();
        return run(db => db.Wallets.Any(w => w.Workspace == workspaceId
                                             && w.Status != WalletStatus.Deleted
                                             && w.ID != exceptId
                                             && w.Name.ToLower() == lower));
    }

    public Workspace? FindWorkspace(long workspaceId) =>
        run(db => db.Workspaces.AsNoTracking().FirstOrDefault(w => w.Id == workspaceId));

    private Wallet? find(int key) => run(db => db.Wallets.AsNoTracking().FirstOrDefault(w => w.ID == key));

    private void put(int key, Wallet value)
    {
        if (value.ID != key)
            throw new ArgumentException("The key must be the wallet id.", nameof(key));
        run(db =>
        {
            if (key == 0)
                db.Wallets.Add(value);
            else if (db.Wallets.Any(w => w.ID == key))
                db.Wallets.Update(value);
            else
                throw new KeyNotFoundException($"Wallet {key} not found.");
            return db.SaveChanges();
        });
    }

    private List<KeyValuePair<int, Wallet>> pairs() =>
        Values.Select(w => new KeyValuePair<int, Wallet>(w.ID, w)).ToList();

    private T run<T>(Func<PiscoDbContext, T> query)
    {
        using var db = dbFactory.CreateDbContext();
        return query(db);
    }
}
