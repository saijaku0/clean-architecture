using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TrainBooking.Domain.Common.Guards;

/// <summary>
/// Static extension class for the IGuard interface, providing methods to validate null or empty values.
/// </summary>
/// <remarks>
/// These extension methods allow validating values for null or empty strings,
/// throwing exceptions when validation fails.
/// </remarks>
public static class GuardAgainstNullExtensions
{
    /// <summary>
    /// Validates that the value is not null or empty. Throws an ArgumentException if the value is null or empty.
    /// </summary>
    /// <param name="guard">The guard interface instance.</param>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null or empty.
    /// </exception>
    /// <remarks>
    /// This extension method checks a string value for null or emptiness and throws
    /// an ArgumentException with the specified parameter name if validation fails.
    /// </remarks>
    public static string NullOrEmpty(
        this IGuard guard,
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException($"Parameter '{parameterName}' cannot be null or empty.", parameterName);
        return value;
    }

    /// <summary>
    /// Validates that the value is not null or whitespace. Throws an ArgumentException if the value is null or whitespace.
    /// </summary>
    /// <param name="guard">The guard interface instance.</param>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null or whitespace.
    /// </exception>
    /// <remarks>
    /// This extension method checks a string value for null or whitespace and throws
    /// an ArgumentException with the specified parameter name if validation fails.
    /// </remarks>
    public static string NullOrWhiteSpace(
        this IGuard guard,
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Parameter '{parameterName}' cannot be null or whitespace.", parameterName);
        return value;
    }

    public static T Null<T>(
        this IGuard guard,
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName, $"Parameter '{parameterName}' cannot be null.");
        return value;
    }
}
