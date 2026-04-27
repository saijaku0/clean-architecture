namespace TrainBooking.Domain.Common.Entities;

/// <summary>
/// Base abstract class for entities with a generic identifier.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
     where TId : notnull
{
    public TId Id { get; protected init; } = default!;

    protected Entity(TId id)
    {
        if (id is null)
            throw new ArgumentNullException(nameof(id), "Id cannot be null.");
        Id = id;
    }
    protected Entity() { }

    public bool Equals(Entity<TId>? other) =>
        other is not null
        && GetType() == other.GetType()
        && Id.Equals(other.Id);

    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}

