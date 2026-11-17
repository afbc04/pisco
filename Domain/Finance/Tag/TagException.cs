namespace Pisco.Domain;

public sealed class TagException(string message, DomainField? field = null) : DomainException(message, field)
{
    public static TagException NotFound() => new("Tag not found.");

    public static TagException NameEmpty() => new("The name can't be empty.", DomainField.Name);

    public static TagException NameTooLong(int max) => new($"The name is too long (max {max}).", DomainField.Name);

    public static TagException NameTaken(string name) => new($"There is already a tag called {name}.", DomainField.Name);

    public static TagException InvalidColor() => new("Pick a valid colour.");
}
