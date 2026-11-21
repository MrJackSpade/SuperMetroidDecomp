namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>Accepts every value of <typeparamref name="TBase"/>, whatever specialization of it the framework made.</summary>
/// <typeparam name="TBase">The framework's public base type.</typeparam>
/// <param name="type">The handling of its values.</param>
internal sealed class TypedProbe<TBase>(StateGraphType type) : StateGraphProbe(type)
{
    /// <inheritdoc/>
    internal override bool Accepts(object value) => value is TBase;
}
