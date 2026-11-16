using System.Text.Json.Serialization;

namespace Pisco.Domain;

public class Workspace
{
    public const int NameLengthMax = 30;

    [JsonInclude] public long Id { get; private set; }
    [JsonInclude] public string Name { get; private set; } = "";
    [JsonInclude] public bool IsPublic { get; private set; }

    [JsonConstructor]
    private Workspace() { }

    public Workspace(string name, bool isPublic)
    {
        Rename(name);
        IsPublic = isPublic;
    }

    public void Rename(string name)
    {
        name = (name ?? "").Trim();
        if (name.Length == 0)
            throw new DomainException("The name can't be empty", DomainField.Name);
        if (name.Length > NameLengthMax)
            throw new DomainException($"The name is too long (max {NameLengthMax})", DomainField.Name);
        Name = name;
    }

    public void SetPublic(bool isPublic) => IsPublic = isPublic;
}
