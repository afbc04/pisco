namespace Pisco.Domain;

public enum WalletStatus
{
    Draft = 0,
    Published = 1,
    Deleted = 2,
}

public class Wallet
{
    public const int NameLengthMax = 40;
    public const int ColorLengthMax = 6;
    public const int IconLengthMax = 40;

    private string name = "";
    private string color = "";
    private string icon = "";
    private bool active;
    private WalletStatus status;

    public int ID { get; set; }
    public long Workspace { get; set; }

    public string Name
    {
        get => name;
        set
        {
            var trimmed = (value ?? "").Trim();
            if (trimmed.Length == 0)
                throw WalletException.NameEmpty();
            if (trimmed.Length > NameLengthMax)
                throw WalletException.NameTooLong(NameLengthMax);
            name = trimmed;
        }
    }

    public string Color
    {
        get => color;
        set
        {
            var hex = (value ?? "").Trim().ToLowerInvariant();
            if (hex.Length != ColorLengthMax || hex[0] != '#' || !hex[1..].All(Uri.IsHexDigit))
                throw WalletException.InvalidColor();
            color = hex;
        }
    }

    public string Icon
    {
        get => icon;
        set => icon = (value ?? "").Trim();
    }

    public long InitialBalance { get; set; }
    public bool Active
    {
        get => active;
        set
        {
            active = value;
            InactiveDate = value ? null : DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }

    public DateOnly CreationDate { get; private set; }
    public DateTime UpdatedDate { get; private set; }
    public DateOnly? InactiveDate { get; private set; }
    public DateOnly? DeletedDate { get; private set; }

    public WalletStatus Status
    {
        get => status;
        set
        {
            status = value;
            DeletedDate = value == WalletStatus.Deleted ? DateOnly.FromDateTime(DateTime.UtcNow) : null;
        }
    }

    public bool IsDeleted => status == WalletStatus.Deleted;
    public bool IsDraft => status == WalletStatus.Draft;
    public bool IsPublished => status == WalletStatus.Published;

    private Wallet() { }

    internal Wallet(long workspaceId, string name, string color, string icon, long initialBalance = 0, bool active = true, bool draft = true)
    {
        Workspace = workspaceId;
        Name = name;
        Color = color;
        Icon = icon;
        InitialBalance = initialBalance;
        Active = active;
        Status = draft ? WalletStatus.Draft : WalletStatus.Published;
        UpdatedDate = DateTime.UtcNow;
        CreationDate = DateOnly.FromDateTime(UpdatedDate);
    }

    internal void Update(string name, string color, string icon, long initialBalance)
    {
        Name = name;
        Color = color;
        Icon = icon;
        InitialBalance = initialBalance;
        Touch();
    }

    private void Touch() => UpdatedDate = DateTime.UtcNow;
}
