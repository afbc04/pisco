using System.Collections;
using Microsoft.EntityFrameworkCore;
using Pisco.Data;
using Pisco.Domain;

namespace Pisco.Repository.Finance;

public class CategoryRepository(IDbContextFactory<PiscoDbContext> dbFactory) : IDictionary<int, Category>
{
    public Category this[int key]
    {
        get => find(key) ?? throw new KeyNotFoundException($"Category {key} not found.");
        set => put(key, value);
    }

    public ICollection<int> Keys => run(db => db.Categories.Select(c => c.ID).ToList());

    public ICollection<Category> Values => run(db => db.Categories.AsNoTracking().ToList());

    public int Count => run(db => db.Categories.Count());

    public bool IsReadOnly => false;

    public void Add(int key, Category value)
    {
        if (key != 0 && ContainsKey(key))
            throw new ArgumentException($"Category {key} already exists.", nameof(key));
        put(key, value);
    }

    public void Add(KeyValuePair<int, Category> item) => Add(item.Key, item.Value);

    public bool ContainsKey(int key) => run(db => db.Categories.Any(c => c.ID == key));

    public bool Contains(KeyValuePair<int, Category> item) =>
        item.Value.ID == item.Key && ContainsKey(item.Key);

    public bool TryGetValue(int key, out Category value)
    {
        var category = find(key);
        value = category!;
        return category is not null;
    }

    public bool Remove(int key) => run(db => db.Categories.Where(c => c.ID == key).ExecuteDelete()) > 0;

    public bool Remove(KeyValuePair<int, Category> item) => Contains(item) && Remove(item.Key);

    public void Clear() => run(db => db.Categories.ExecuteDelete());

    public void CopyTo(KeyValuePair<int, Category>[] array, int arrayIndex) =>
        pairs().CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<int, Category>> GetEnumerator() => pairs().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public List<Category> ByWorkspace(long workspaceId, CategoryFlow flow) =>
        run(db => db.Categories.AsNoTracking()
            .Where(c => c.Workspace == workspaceId && c.Flow == flow && c.Status != CategoryStatus.Deleted)
            .OrderBy(c => c.Name)
            .ToList());

    public List<Category> DeletedByWorkspace(long workspaceId, CategoryFlow flow) =>
        run(db => db.Categories.AsNoTracking()
            .Where(c => c.Workspace == workspaceId && c.Flow == flow && c.Status == CategoryStatus.Deleted)
            .OrderByDescending(c => c.DeletedDate)
            .ThenBy(c => c.Name)
            .ToList());

    public bool NameExists(long workspaceId, CategoryFlow flow, string name, int exceptId)
    {
        var lower = name.ToLower();
        return run(db => db.Categories.Any(c => c.Workspace == workspaceId
                                               && c.Flow == flow
                                               && c.Status != CategoryStatus.Deleted
                                               && c.ID != exceptId
                                               && c.Name.ToLower() == lower));
    }

    public bool HasChildren(int parentId, bool includeDeleted) =>
        run(db => db.Categories.Any(c => c.Parent == parentId && (includeDeleted || c.Status != CategoryStatus.Deleted)));

    public Workspace? FindWorkspace(long workspaceId) =>
        run(db => db.Workspaces.AsNoTracking().FirstOrDefault(w => w.Id == workspaceId));

    private Category? find(int key) => run(db => db.Categories.AsNoTracking().FirstOrDefault(c => c.ID == key));

    private void put(int key, Category value)
    {
        if (value.ID != key)
            throw new ArgumentException("The key must be the category id.", nameof(key));
        run(db =>
        {
            if (key == 0)
                db.Categories.Add(value);
            else if (db.Categories.Any(c => c.ID == key))
                db.Categories.Update(value);
            else
                throw new KeyNotFoundException($"Category {key} not found.");
            return db.SaveChanges();
        });
    }

    private List<KeyValuePair<int, Category>> pairs() =>
        Values.Select(c => new KeyValuePair<int, Category>(c.ID, c)).ToList();

    private T run<T>(Func<PiscoDbContext, T> query)
    {
        using var db = dbFactory.CreateDbContext();
        return query(db);
    }
}
