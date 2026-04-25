namespace TrainBooking.Domain.Common.Results;

/// <summary>
/// Represents the outcome of an operation as either success or failure.
/// </summary>
/// <remarks>
/// Guarantees consistency between state and error:
/// success results have no error, while failure results always include one.
/// </remarks>
public class ResultBase
{
    public Error? Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    protected ResultBase(bool isSuccess, Error? error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot carry an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must carry an error.");

        IsSuccess = isSuccess;
        Error = error;
    }
}
