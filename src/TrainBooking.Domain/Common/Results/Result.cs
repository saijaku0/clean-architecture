namespace TrainBooking.Domain.Common.Results;

/// <summary>
/// Represents the result of an operation, containing either a success state or an error.
/// </summary>
/// <remarks>
/// Use Result.Success() for a successful result and Result.Failure(Error) for a failure.
/// Supports implicit conversion from Error to Result for convenient error returns.
/// </remarks>
public class Result : ResultBase
{
    private Result(bool isSuccess, Error? error)
        : base(isSuccess, error) { }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Implicitly converts an Error to Result, returning a failed result.
    /// </summary>
    /// <remarks>
    /// Equivalent to calling Result.Failure(error).
    /// </remarks>
    /// <param name="error">The reason for the failure.</param>
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation: contains a value of type T on success or an Error on failure.
/// </summary>
/// <remarks>
/// Supports implicit conversions from T and Error.
/// Accessing Value throws an InvalidOperationException for a failed result.
/// Created via Success(T) and Failure(Error).
/// </remarks>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class Result<T> : ResultBase
{
    private readonly T? _value;
    private Result(T value) : base(true, Error.None) => _value = value;
    private Result(Error error) : base(false, error) => _value = default;

    public T Value => IsSuccess
    ? _value!
    : throw new InvalidOperationException("Cannot access Value on a failed result.");
    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    /// <summary>
    /// Implicitly converts a value of type T to Result<T>, returning a successful result with the specified value.
    /// </summary>
    /// <remarks>
    /// Equivalent to calling Success(value).
    /// </remarks>
    /// <param name="value">The value to be wrapped in a successful Result<T>.</param>
    public static implicit operator Result<T>(T value) => Success(value);
    /// <summary>
    /// Implicitly converts a value of type Error to Result<T>, returning a failed result with the specified error.
    /// </summary>
    /// <remarks>
    /// Equivalent to calling Failure(error).
    /// </remarks>
    /// <param name="error">The reason for the failure.</param>
    public static implicit operator Result<T>(Error error) => Failure(error);
}
