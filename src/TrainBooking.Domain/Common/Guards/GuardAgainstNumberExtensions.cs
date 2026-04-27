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
    /// Ensures that the provided numeric value is greater than zero.
    /// </summary>
    /// <typeparam name="T">The type of the numeric value.</typeparam>
    /// <param name="guard">The guard instance used as an entry point for validation.</param>
    /// <param name="value">The numeric value to validate.</param>
    /// <param name="parameterName">
    /// The name of the parameter being validated. Automatically populated via caller information.
    /// </param>
    /// <returns>The validated numeric value if it is greater than zero.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is less than or equal to zero.
    /// </exception>
    public static T NegativeOrZero<T>(
        this IGuard guard,
        T value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where T : struct, IComparable<T>
    {
        if (value.CompareTo(default) <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"{parameterName} must be greater than zero.");
        }
        return value;
    }
}
