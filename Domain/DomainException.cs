namespace Pisco.Domain;

public enum DomainField { Name, Parent }

public class DomainException(string message, DomainField? field = null) : Exception(message)
{
    public DomainField? Field { get; } = field;
}
