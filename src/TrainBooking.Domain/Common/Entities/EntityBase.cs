namespace TrainBooking.Domain.Common.Entities;

/// <summary>
/// Base abstract entity class with a Guid identifier and creation and update timestamps.
/// </summary>
/// <remarks>
/// Uses UTC time: CreatedAt and UpdatedAt.
/// The constructor with an id parameter validates that the id is not Guid.Empty and throws an ArgumentException.
/// A protected parameterless constructor is provided for ORM or serialization.
/// Properties have protected setters; the Touch method updates UpdatedAt to the current UTC time.
/// </remarks>
public abstract class EntityBase : Entity<Guid>
{
    public DateTime CreatedAt { get; protected init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    protected EntityBase(Guid id) : base(id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));
    }
    protected EntityBase() { }
    protected void Touch() => UpdatedAt = DateTime.UtcNow;
}
