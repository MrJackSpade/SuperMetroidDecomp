namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>Accepts every value of <typeparamref name="TBase"/>, whatever specialization of it the framework made.</summary>
/// <typeparam name="TBase">The framework's public base type.</typeparam>
/// <param name="type">The handling of its values.</param>
/// <param name="enumerating">Whether <typeparamref name="TBase"/> is a collection, which only an object that enumerates is.</param>
internal sealed class TypedProbe<TBase>(StateGraphType type, bool enumerating = false) : StateGraphProbe(type)
{
    /// <inheritdoc/>
    internal override bool AcceptsOnlyEnumerating => enumerating;

    /// <inheritdoc/>
    internal override bool Accepts(object value) => value is TBase;
}
