namespace SuperMetroid.Core;

/// <summary>Receives each value an object of the game holds that can reach another object, field by field (#2602).</summary>
public interface IStateGraphVisitor
{
    /// <summary>Receives the value <paramref name="field"/> holds.</summary>
    /// <param name="field">The field.</param>
    /// <param name="value">Its value; a value type's is boxed.</param>
    void Child(StateGraphField field, object? value);
}
