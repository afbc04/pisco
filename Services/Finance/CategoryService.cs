using System.Security.Claims;
using Pisco.Domain;
using Pisco.Repository.Finance;

namespace Pisco.Services.Finance;

public class CategoryManager(CategoryRepository categories)
{
    public List<Category> List(long workspaceId, CategoryFlow flow, ClaimsPrincipal actor)
    {
        ensureCanRead(workspaceId, actor);
        return categories.ByWorkspace(workspaceId, flow);
    }

    public List<Category> ListDeleted(long workspaceId, CategoryFlow flow, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        return categories.DeletedByWorkspace(workspaceId, flow);
    }

    public Category Get(long workspaceId, int id, ClaimsPrincipal actor)
    {
        ensureCanRead(workspaceId, actor);
        return find(workspaceId, id);
    }

    public Category Create(long workspaceId, CategoryFlow flow, CategoryKind kind, int? parentId, string name, string icon, bool active, bool draft, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var category = new Category(workspaceId, flow, kind, parentId, name, icon, active, draft);
        ensureParentValid(category);
        ensureNameFree(category);
        categories.Add(category.ID, category);
        return category;
    }

    public void Update(long workspaceId, int id, CategoryKind kind, int? parentId, string name, string icon, bool active, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var category = find(workspaceId, id);
        if (kind != CategoryKind.Simple && categories.HasChildren(category.ID, includeDeleted: true))
            throw CategoryException.HasSubcategories();
        category.Update(kind, parentId, name, icon);
        ensureParentValid(category);
        ensureNameFree(category);
        category.Active = active;
        categories[category.ID] = category;
    }

    public void SetDraft(long workspaceId, int id, bool draft, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var category = find(workspaceId, id);
        category.Status = draft ? CategoryStatus.Draft : CategoryStatus.Published;
        categories[category.ID] = category;
    }

    public void Delete(long workspaceId, int id, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var category = find(workspaceId, id);
        if (categories.HasChildren(category.ID, includeDeleted: false))
            throw CategoryException.HasSubcategories();
        category.Status = CategoryStatus.Deleted;
        categories[category.ID] = category;
    }

    public void Restore(long workspaceId, int id, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var category = findDeleted(workspaceId, id);
        if (category.Parent is { } parentId && !isLiveParent(category, parentId))
            throw CategoryException.ParentDeleted();
        category.Status = CategoryStatus.Published;
        ensureNameFree(category);
        categories[category.ID] = category;
    }

    public void Purge(long workspaceId, int id, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var category = findDeleted(workspaceId, id);
        if (categories.HasChildren(category.ID, includeDeleted: true))
            throw CategoryException.HasSubcategories();
        categories.Remove(category.ID);
    }

    private Category findDeleted(long workspaceId, int id) =>
        categories.TryGetValue(id, out var category) && category.Workspace == workspaceId && category.IsDeleted
            ? category
            : throw CategoryException.NotFound();

    private Category find(long workspaceId, int id) =>
        categories.TryGetValue(id, out var category) && category.Workspace == workspaceId && !category.IsDeleted
            ? category
            : throw CategoryException.NotFound();

    private bool isLiveParent(Category category, int parentId) =>
        categories.TryGetValue(parentId, out var parent)
        && parent.Workspace == category.Workspace
        && !parent.IsDeleted;

    private void ensureParentValid(Category category)
    {
        if (category.Parent is not { } parentId)
            return;
        if (parentId == category.ID
            || !categories.TryGetValue(parentId, out var parent)
            || parent.Workspace != category.Workspace
            || parent.Flow != category.Flow
            || parent.Kind != CategoryKind.Simple
            || parent.IsDeleted)
            throw CategoryException.ParentInvalid();
    }

    private void ensureNameFree(Category category)
    {
        if (categories.NameExists(category.Workspace, category.Flow, category.Name, category.ID))
            throw CategoryException.NameTaken(category.Name);
    }

    private void ensureCanRead(long workspaceId, ClaimsPrincipal actor)
    {
        var ws = categories.FindWorkspace(workspaceId);
        if (ws is null || (!ws.IsPublic && !actor.IsSignedIn()))
            throw new DomainException("The workspace doesn't exist.");
    }

    private void ensureCanWrite(long workspaceId, ClaimsPrincipal actor)
    {
        actor.EnsureSignedIn();
        if (categories.FindWorkspace(workspaceId) is null)
            throw new DomainException("The workspace doesn't exist.");
    }
}
