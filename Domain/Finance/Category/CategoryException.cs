namespace Pisco.Domain;

public sealed class CategoryException(string message, DomainField? field = null) : DomainException(message, field)
{
    public static CategoryException NotFound() => new("Category not found.");

    public static CategoryException NameEmpty() => new("The name can't be empty.", DomainField.Name);

    public static CategoryException NameTooLong(int max) => new($"The name is too long (max {max}).", DomainField.Name);

    public static CategoryException NameTaken(string name) => new($"There is already a category called {name}.", DomainField.Name);

    public static CategoryException InvalidIcon() => new("Pick an icon from the list.");

    public static CategoryException ParentRequired() => new("Pick the category this one belongs to.", DomainField.Parent);

    public static CategoryException ParentInvalid() => new("A subcategory can only belong to a simple category of the same type.", DomainField.Parent);

    public static CategoryException HasSubcategories() => new("This category has subcategories. Move or delete them first.");

    public static CategoryException ParentDeleted() => new("Restore the category this one belongs to first.");
}
