using System.Collections;
using Microsoft.EntityFrameworkCore;
using Pisco.Data;
using Pisco.Domain;

namespace Pisco.Repository.Finance;

public class TagRepository(IDbContextFactory<PiscoDbContext> dbFactory) : IDictionary<int, Tag>
{
    public Tag this[int key]
    {
        get => find(key) ?? throw new KeyNotFoundException($"Tag {key} not found.");
        set => put(key, value);
    }

    public ICollection<int> Keys => run(db => db.Tags.Select(t => t.ID).ToList());

    public ICollection<Tag> Values => run(db => db.Tags.AsNoTracking().ToList());

    public int Count => run(db => db.Tags.Count());

    public bool IsReadOnly => false;

    public void Add(int key, Tag value)
    {
        if (key != 0 && ContainsKey(key))
            throw new ArgumentException($"Tag {key} already exists.", nameof(key));
        put(key, value);
    }

    public void Add(KeyValuePair<int, Tag> item) => Add(item.Key, item.Value);

    public bool ContainsKey(int key) => run(db => db.Tags.Any(t => t.ID == key));

    public bool Contains(KeyValuePair<int, Tag> item) =>
        item.Value.ID == item.Key && ContainsKey(item.Key);

    public bool TryGetValue(int key, out Tag value)
    {
        var tag = find(key);
        value = tag!;
        return tag is not null;
    }

    public bool Remove(int key) => run(db => db.Tags.Where(t => t.ID == key).ExecuteDelete()) > 0;

    public bool Remove(KeyValuePair<int, Tag> item) => Contains(item) && Remove(item.Key);

    public void Clear() => run(db => db.Tags.ExecuteDelete());

    public void CopyTo(KeyValuePair<int, Tag>[] array, int arrayIndex) =>
        pairs().CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<int, Tag>> GetEnumerator() => pairs().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public List<Tag> ByWorkspace(long workspaceId) =>
        run(db => db.Tags.AsNoTracking()
            .Where(t => t.Workspace == workspaceId)
            .OrderBy(t => t.Name)
            .ToList());

    public bool NameExists(long workspaceId, string name, int exceptId)
    {
        var lower = name.ToLower();
        return run(db => db.Tags.Any(t => t.Workspace == workspaceId
                                          && t.ID != exceptId
                                          && t.Name.ToLower() == lower));
    }

    public Workspace? FindWorkspace(long workspaceId) =>
        run(db => db.Workspaces.AsNoTracking().FirstOrDefault(w => w.Id == workspaceId));

    private Tag? find(int key) => run(db => db.Tags.AsNoTracking().FirstOrDefault(t => t.ID == key));

    private void put(int key, Tag value)
    {
        if (value.ID != key)
            throw new ArgumentException("The key must be the tag id.", nameof(key));
        run(db =>
        {
            if (key == 0)
                db.Tags.Add(value);
            else if (db.Tags.Any(t => t.ID == key))
                db.Tags.Update(value);
            else
                throw new KeyNotFoundException($"Tag {key} not found.");
            return db.SaveChanges();
        });
    }

    private List<KeyValuePair<int, Tag>> pairs() =>
        Values.Select(t => new KeyValuePair<int, Tag>(t.ID, t)).ToList();

    private T run<T>(Func<PiscoDbContext, T> query)
    {
        using var db = dbFactory.CreateDbContext();
        return query(db);
    }
}
