namespace WorkflowPlatform.Domain.Common.Primitives;

/// <summary>
/// Base class for value objects in the domain model.
/// Value objects are immutable and compared by their structural equality.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the atomic values that comprise this value object.
    /// Used for equality comparisons and hash code generation.
    /// </summary>
    /// <returns>An enumerable of the atomic values</returns>
    protected abstract IEnumerable<object?> GetAtomicValues();

    /// <summary>
    /// Determines whether the specified value object is equal to the current value object.
    /// </summary>
    /// <param name="other">The value object to compare with the current value object</param>
    /// <returns>True if the specified value object is equal to the current value object; otherwise, false</returns>
    public virtual bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
        {
            return false;
        }

        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current value object.
    /// </summary>
    /// <param name="obj">The object to compare with the current value object</param>
    /// <returns>True if the specified object is equal to the current value object; otherwise, false</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ValueObject);
    }

    /// <summary>
    /// Returns the hash code for the current value object.
    /// </summary>
    /// <returns>A hash code for the current value object</returns>
    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(1, (current, obj) => 
                HashCode.Combine(current, obj?.GetHashCode() ?? 0));
    }

    /// <summary>
    /// Determines whether two value objects are equal.
    /// </summary>
    /// <param name="left">The first value object to compare</param>
    /// <param name="right">The second value object to compare</param>
    /// <returns>True if the value objects are equal; otherwise, false</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two value objects are not equal.
    /// </summary>
    /// <param name="left">The first value object to compare</param>
    /// <param name="right">The second value object to compare</param>
    /// <returns>True if the value objects are not equal; otherwise, false</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Creates a copy of the value object.
    /// Since value objects are immutable, this returns the same instance.
    /// </summary>
    /// <returns>The same instance of the value object</returns>
    public ValueObject Copy()
    {
        return this;
    }
}
