namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Accepts the read-only lists the compiler makes for collection expressions of <typeparamref name="T"/>, whose types it
/// names only inside the assembly that made them. Among the element types a list is a list of through covariance, the
/// most derived is preferred, so the restored array holds what the list held.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
/// <param name="type">The handling, which writes such a list as an array of <typeparamref name="T"/>.</param>
internal sealed class CompilerReadOnlyListProbe<T>(CompilerReadOnlyListStateType<T> type) : StateGraphProbe(type)
{
    /// <inheritdoc/>
    internal override bool Accepts(object value) =>
        value.GetType().Name.StartsWith("<>z__", StringComparison.Ordinal) && value is IReadOnlyList<T>;

    /// <inheritdoc/>
    internal override bool AcceptsOnlyEnumerating => true;

    /// <inheritdoc/>
    internal override bool MoreSpecificThan(StateGraphType other) =>
        other is ICompilerReadOnlyList list && list.HoldsElementsOf(System.Array.Empty<T>());
}
