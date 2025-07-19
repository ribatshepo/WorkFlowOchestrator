namespace WorkflowPlatform.Domain.Common.Primitives;

/// <summary>
/// Base class for all domain entities.
/// Provides identity equality and audit fields.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : struct, IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the unique identifier of the entity.
    /// </summary>
    public TId Id { get; protected set; }

    /// <summary>
    /// Gets the user who created this entity.
    /// </summary>
    public Guid CreatedBy { get; protected set; }

    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Gets the user who last modified this entity.
    /// </summary>
    public Guid? ModifiedBy { get; protected set; }

    /// <summary>
    /// Gets the date and time when the entity was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; protected set; }

    /// <summary>
    /// Gets the version for optimistic concurrency control.
    /// </summary>
    public int Version { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the Entity class.
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="createdBy">The user who created the entity</param>
    protected Entity(TId id, Guid createdBy)
    {
        Id = id;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        Version = 0;
    }

    /// <summary>
    /// Initializes a new instance of the Entity class.
    /// Used by EF Core for materialization.
    /// </summary>
    protected Entity()
    {
    }

    /// <summary>
    /// Updates the modification audit fields.
    /// </summary>
    /// <param name="modifiedBy">The user who modified the entity</param>
    protected void UpdateModified(Guid modifiedBy)
    {
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Determines whether the specified entity is equal to the current entity.
    /// </summary>
    /// <param name="other">The entity to compare with the current entity</param>
    /// <returns>True if the specified entity is equal to the current entity; otherwise, false</returns>
    public virtual bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && GetType() == other.GetType();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity</param>
    /// <returns>True if the specified object is equal to the current entity; otherwise, false</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Entity<TId>);
    }

    /// <summary>
    /// Returns the hash code for the current entity.
    /// </summary>
    /// <returns>A hash code for the current entity</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, GetType());
    }

    /// <summary>
    /// Determines whether two entities are equal.
    /// </summary>
    /// <param name="left">The first entity to compare</param>
    /// <param name="right">The second entity to compare</param>
    /// <returns>True if the entities are equal; otherwise, false</returns>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two entities are not equal.
    /// </summary>
    /// <param name="left">The first entity to compare</param>
    /// <param name="right">The second entity to compare</param>
    /// <returns>True if the entities are not equal; otherwise, false</returns>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}
