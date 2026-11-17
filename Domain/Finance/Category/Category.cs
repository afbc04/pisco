namespace Pisco.Domain;

public enum CategoryFlow
{
    Expense = 0,
    Income = 1,
}

public enum CategoryKind
{
    Simple = 0,
    Composite = 1,
    Subcategory = 2,
}

public enum CategoryStatus
{
    Draft = 0,
    Published = 1,
    Deleted = 2,
}

public class Category
{
    public const int NameLengthMax = 40;
    public const int IconLengthMax = 40;

    private string name = "";
    private string icon = "";
    private bool active;
    private CategoryStatus status;

    public int ID { get; set; }
    public long Workspace { get; set; }
    public CategoryFlow Flow { get; set; }
    public CategoryKind Kind { get; private set; }
    public int? Parent { get; private set; }

    public string Name
    {
        get => name;
        set
        {
            var trimmed = (value ?? "").Trim();
            if (trimmed.Length == 0)
                throw CategoryException.NameEmpty();
            if (trimmed.Length > NameLengthMax)
                throw CategoryException.NameTooLong(NameLengthMax);
            name = trimmed;
        }
    }

    public string Icon
    {
        get => icon;
        set
        {
            var trimmed = (value ?? "").Trim();
            if (trimmed.Length == 0 || trimmed.Length > IconLengthMax)
                throw CategoryException.InvalidIcon();
            icon = trimmed;
        }
    }

    public bool Active
    {
        get => active;
        set
        {
            if (active == value)
                return;
            active = value;
            InactiveDate = value ? null : DateOnly.FromDateTime(DateTime.UtcNow);
            Touch();
        }
    }

    public DateOnly CreationDate { get; private set; }
    public DateTime UpdatedDate { get; private set; }
    public DateOnly? InactiveDate { get; private set; }
    public DateOnly? DeletedDate { get; private set; }

    public CategoryStatus Status
    {
        get => status;
        set
        {
            if (status == value)
                return;
            status = value;
            DeletedDate = value == CategoryStatus.Deleted ? DateOnly.FromDateTime(DateTime.UtcNow) : null;
            Touch();
        }
    }

    public bool IsDeleted => status == CategoryStatus.Deleted;
    public bool IsDraft => status == CategoryStatus.Draft;
    public bool IsPublished => status == CategoryStatus.Published;
    public bool IsSubcategory => Kind == CategoryKind.Subcategory;

    private Category() { }

    internal Category(long workspaceId, CategoryFlow flow, CategoryKind kind, int? parent, string name, string icon, bool active = true, bool draft = true)
    {
        Workspace = workspaceId;
        Flow = flow;
        SetKind(kind, parent);
        Name = name;
        Icon = icon;
        this.active = active;
        status = draft ? CategoryStatus.Draft : CategoryStatus.Published;
        UpdatedDate = DateTime.UtcNow;
        CreationDate = DateOnly.FromDateTime(UpdatedDate);
        if (!active)
            InactiveDate = CreationDate;
    }

    internal void Update(CategoryKind kind, int? parent, string name, string icon)
    {
        SetKind(kind, parent);
        Name = name;
        Icon = icon;
        Touch();
    }

    private void SetKind(CategoryKind kind, int? parent)
    {
        if (kind == CategoryKind.Subcategory && parent is null)
            throw CategoryException.ParentRequired();
        Kind = kind;
        Parent = kind == CategoryKind.Subcategory ? parent : null;
    }

    private void Touch() => UpdatedDate = DateTime.UtcNow;
}
