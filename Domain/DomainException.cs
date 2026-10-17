namespace Pisco.Domain;

public sealed class DomainException(string messageKey, params object[] args) : Exception(messageKey)
{
    public object[] Args { get; } = args;
}
