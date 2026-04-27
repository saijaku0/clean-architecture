namespace TrainBooking.Domain.Common.Guards;

public interface IGuard { }

public sealed class Guard : IGuard
{
    /// <summary>
    /// Entry point for guard clauses, providing a fluent interface for validation and precondition checks.
    /// </summary>
    public static IGuard Against { get; } = new Guard();

    private Guard() { }
}
