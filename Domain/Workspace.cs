namespace Pisco.Domain;

public class Workspace
{
    public const int NameLengthMax = 30;
    public const int SchemaNameLengthMax = 63;

    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string SchemaName { get; set; } = "";
    public bool IsDemo { get; set; }
    public bool IsLanding { get; set; }
}
