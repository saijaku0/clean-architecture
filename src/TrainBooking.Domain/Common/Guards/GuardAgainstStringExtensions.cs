using System.Runtime.CompilerServices;

namespace TrainBooking.Domain.Common.Guards;

/// <summary>
/// Static extension class for the IGuard interface, providing methods to validate string lengths.
/// </summary>
/// <remarks>
/// These extension methods allow validating string values for maximum length,
/// or other string-related constraints, throwing exceptions when validation fails.
/// </remarks> 
public static class GuardAgainstStringExtensions
{
    /// <summary>
    /// Static extension method for the IGuard interface that checks if a string exceeds a specified maximum length.
    /// </summary>
    /// <param name="guard">The guard interface instance.</param>
    /// <param name="value">The string value to validate.</param>
    /// <param name="maxLength">The maximum allowed length.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The validated string.</returns>
    /// <exception cref="ArgumentException">Thrown when the string exceeds the maximum length.</exception>
    /// <remarks>
    /// This extension method checks if the provided string exceeds the specified maximum length.
    /// </remarks>
    public static string StringTooLong(
        this IGuard guard,
        string value,
        int maxLength,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value.Length > maxLength)
            throw new ArgumentException($"Parameter '{parameterName}' cannot exceed {maxLength} characters.", parameterName);
        return value;
    }
}
