namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>Accepts one framework singleton, such as a comparer a collection is keyed by.</summary>
/// <param name="type">The singleton's handling.</param>
internal sealed class SingletonProbe(SingletonStateType type) : StateGraphProbe(type)
{
    /// <inheritdoc/>
    internal override bool Accepts(object value) => ReferenceEquals(value, type.Instance);
}
