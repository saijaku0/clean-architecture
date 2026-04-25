using System.Runtime.CompilerServices;

namespace TrainBooking.Domain.Common.Guards;

/// <summary>
/// Extension methods for <see cref="IGuard"/> providing validation rules for <see cref="Guid"/> values.
/// </summary>
/// <remarks>
/// These guard clauses are used to enforce domain and application-level contracts
/// by validating GUID values and throwing exceptions when rules are violated.
/// </remarks>
public static class GuardAgainstGuidExtensions
{
    /// <summary>
    /// Ensures that the provided <see cref="Guid"/> is not empty.
    /// </summary>
    /// <param name="guard">The guard instance used as an entry point for validation.</param>
    /// <param name="value">The GUID value to validate.</param>
    /// <param name="parameterName">
    /// The name of the parameter being validated. Automatically populated via caller information.
    /// </param>
    /// <returns>The validated GUID value if it is valid.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided GUID is <see cref="Guid.Empty"/>.
    /// </exception>
    public static Guid Empty(
        this IGuard guard,
        Guid value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"Parameter '{parameterName}' cannot be an empty GUID.", parameterName);
        return value;
    }
}
