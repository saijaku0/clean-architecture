namespace TrainBooking.Domain.Common.Exceptions;

/// <summary>
/// Extension for enforcing conditions in domain logic. Provides methods to validate various conditions
/// and throw exceptions when they are violated.
/// </summary>
[Serializable]
public class DomainException : Exception
{
    public string? ParamName { get; }
    public string? ErrorCode { get; }

    public DomainException() { }

    public DomainException(string? message) : base(message) { }

    public DomainException(string? message, Exception? innerException)
        : base(message, innerException)
    { }

    public DomainException(string? message, string? paramName)
        : base(message)
    {
        ParamName = paramName;
    }

    public DomainException(string? message, string? paramName, string? errorCode)
        : base(message)
    {
        ParamName = paramName;
        ErrorCode = errorCode;
    }
}
