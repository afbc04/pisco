namespace Pisco.Domain;

public class Tag
{
    public const int NameLengthMax = 24;
    public const int ColorLength = 7;

    private string name = "";
    private string color = "";

    public int ID { get; set; }
    public long Workspace { get; set; }

    public string Name
    {
        get => name;
        set
        {
            var trimmed = (value ?? "").Trim();
            if (trimmed.Length == 0)
                throw TagException.NameEmpty();
            if (trimmed.Length > NameLengthMax)
                throw TagException.NameTooLong(NameLengthMax);
            name = trimmed;
        }
    }

    public string Color
    {
        get => color;
        set
        {
            var hex = (value ?? "").Trim().ToLowerInvariant();
            if (hex.Length != ColorLength || hex[0] != '#' || !hex[1..].All(Uri.IsHexDigit))
                throw TagException.InvalidColor();
            color = hex;
        }
    }

    private Tag() { }

    internal Tag(long workspaceId, string name, string color)
    {
        Workspace = workspaceId;
        Name = name;
        Color = color;
    }

    internal void Update(string name, string color)
    {
        Name = name;
        Color = color;
    }
}
