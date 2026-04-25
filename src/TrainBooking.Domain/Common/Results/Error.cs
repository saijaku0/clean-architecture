namespace TrainBooking.Domain.Common.Results;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Internal,
    BadRequest,
    TooManyRequests
}

public record Error(string Code, string Message, ErrorType Type)
{
    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);
    public static Error Validation(string code, string message)
        => new(code, message, ErrorType.Validation);
    public static Error Unauthorized(string code, string message)
        => new(code, message, ErrorType.Unauthorized);
    public static Error Internal(string code, string message)
        => new(code, message, ErrorType.Internal);
    public static Error Conflict(string code, string message)
        => new(code, message, ErrorType.Conflict);
    public static Error BadRequest(string code, string message)
        => new(code, message, ErrorType.BadRequest);
    public static Error Forbidden(string code, string message)
        => new(code, message, ErrorType.Forbidden);
    public static Error TooManyRequests(string code, string message)
        => new(code, message, ErrorType.TooManyRequests);

    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Internal);
}
