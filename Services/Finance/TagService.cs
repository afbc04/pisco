using System.Security.Claims;
using Pisco.Domain;
using Pisco.Repository.Finance;

namespace Pisco.Services.Finance;

public class TagManager(TagRepository tags)
{
    public List<Tag> List(long workspaceId, ClaimsPrincipal actor)
    {
        ensureCanRead(workspaceId, actor);
        return tags.ByWorkspace(workspaceId);
    }

    public Tag Create(long workspaceId, string name, string color, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var tag = new Tag(workspaceId, name, color);
        ensureNameFree(tag);
        tags.Add(tag.ID, tag);
        return tag;
    }

    public void Update(long workspaceId, int id, string name, string color, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var tag = find(workspaceId, id);
        tag.Update(name, color);
        ensureNameFree(tag);
        tags[tag.ID] = tag;
    }

    public void Delete(long workspaceId, int id, ClaimsPrincipal actor)
    {
        ensureCanWrite(workspaceId, actor);
        var tag = find(workspaceId, id);
        tags.Remove(tag.ID);
    }

    private Tag find(long workspaceId, int id) =>
        tags.TryGetValue(id, out var tag) && tag.Workspace == workspaceId
            ? tag
            : throw TagException.NotFound();

    private void ensureNameFree(Tag tag)
    {
        if (tags.NameExists(tag.Workspace, tag.Name, tag.ID))
            throw TagException.NameTaken(tag.Name);
    }

    private void ensureCanRead(long workspaceId, ClaimsPrincipal actor)
    {
        var ws = tags.FindWorkspace(workspaceId);
        if (ws is null || (!ws.IsPublic && !actor.IsSignedIn()))
            throw new DomainException("The workspace doesn't exist.");
    }

    private void ensureCanWrite(long workspaceId, ClaimsPrincipal actor)
    {
        actor.EnsureSignedIn();
        if (tags.FindWorkspace(workspaceId) is null)
            throw new DomainException("The workspace doesn't exist.");
    }
}
