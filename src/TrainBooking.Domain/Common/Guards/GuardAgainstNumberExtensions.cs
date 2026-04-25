using System.Runtime.CompilerServices;

namespace TrainBooking.Domain.Common.Guards;

/// <summary>
/// Extension methods for <see cref="IGuard"/> providing validation rules for numeric values.
/// </summary>
/// <remarks>
/// These guard clauses are used to enforce domain and application-level constraints
/// by validating numeric inputs and throwing exceptions when rules are violated.
/// </remarks>
public static class GuardAgainstNumberExtensions
{
    /// <summary>
    /// Ensures that the provided integer value is greater than zero.
    /// </summary>
    /// <param name="guardClause">The guard instance used as an entry point for validation.</param>
    /// <param name="value">The integer value to validate.</param>
    /// <param name="parameterName">
    /// The name of the parameter being validated. Automatically populated via caller information.
    /// </param>
    /// <returns>The validated integer value if it is greater than zero.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is less than or equal to zero.
    /// </exception>
    public static int NegativeOrZero(
        this IGuard guardClause,
        int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"{parameterName} must be greater than zero.");
        }
        return value;
    }
}
