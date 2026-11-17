using Pisco.Domain;

namespace Pisco.Components.Finance;

public static class CategoryLook
{
    public static readonly string[] Icons =
    [
        "cart3", "basket2", "basket3", "bag", "shop", "cup-hot", "egg-fried", "house",
        "lightning-charge", "droplet", "wifi", "phone", "car-front", "fuel-pump", "bus-front", "train-front",
        "airplane", "p-circle", "heart-pulse", "capsule", "hospital", "mortarboard", "book", "controller",
        "film", "music-note-beamed", "ticket-perforated", "gift", "balloon", "scissors", "tools", "umbrella",
        "briefcase", "cash-coin", "cash-stack", "piggy-bank", "graph-up-arrow", "bank", "receipt", "credit-card",
        "tag", "percent", "arrow-repeat", "award", "people", "person-heart", "tree", "globe2",
    ];

    public static readonly CategoryKind[] Kinds = [CategoryKind.Simple, CategoryKind.Composite, CategoryKind.Subcategory];

    public static string FlowColor(CategoryFlow flow) => flow == CategoryFlow.Income ? "var(--success)" : "var(--danger)";

    public static string FlowName(CategoryFlow flow) => flow == CategoryFlow.Income ? "Income" : "Expenses";

    public static string KindName(CategoryKind kind) => kind switch
    {
        CategoryKind.Composite => "Composite",
        CategoryKind.Subcategory => "Subcategory",
        _ => "Simple",
    };

    public static string KindHint(CategoryKind kind) => kind switch
    {
        CategoryKind.Composite => "Groups items from any simple category.",
        CategoryKind.Subcategory => "Belongs to a simple category.",
        _ => "A regular category. Can have subcategories.",
    };

    public static string KindClass(CategoryKind kind) => kind switch
    {
        CategoryKind.Composite => "is-composite",
        CategoryKind.Subcategory => "is-subcategory",
        _ => "is-simple",
    };

    public static string StatusName(CategoryStatus status) => status switch
    {
        CategoryStatus.Draft => "Draft",
        CategoryStatus.Published => "Published",
        _ => "Deleted",
    };

    public static string StatusPlural(CategoryStatus status) => status switch
    {
        CategoryStatus.Draft => "Drafts",
        CategoryStatus.Published => "Published",
        _ => "Deleted",
    };

    public static string StatusColor(CategoryStatus status) => status switch
    {
        CategoryStatus.Draft => "secondary",
        CategoryStatus.Published => "success",
        _ => "danger",
    };

    public static string Badge(CategoryStatus status)
    {
        var color = StatusColor(status);
        return $"badge rounded-pill border text-{color}-emphasis bg-{color}-subtle border-{color}-subtle";
    }
}
