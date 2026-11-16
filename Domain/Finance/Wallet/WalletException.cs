namespace Pisco.Domain;

public sealed class WalletException(string message, DomainField? field = null) : DomainException(message, field)
{
    public static WalletException NotFound() => new("Wallet not found.");

    public static WalletException NameEmpty() => new("The name can't be empty.", DomainField.Name);

    public static WalletException NameTooLong(int max) => new($"The name is too long (max {max}).", DomainField.Name);

    public static WalletException NameTaken(string name) => new($"There is already a wallet called {name}.", DomainField.Name);

    public static WalletException InvalidColor() => new("Pick a colour from the list.");
}
