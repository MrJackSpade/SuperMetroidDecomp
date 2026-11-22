namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// A read-only list of <typeparamref name="T"/> the compiler made for a collection expression, whose type no other assembly
/// can name: written as an array of <typeparamref name="T"/>, which a restore holds in its place.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class CompilerReadOnlyListStateType<T>(StateGraphTypes types) : StateGraphType(typeof(IReadOnlyList<T>), types), ICompilerReadOnlyList
{
    /// <inheritdoc/>
    protected override string Named() => Types.NameOf(typeof(T[]));

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        if (Types.Registered(typeof(T[])) is not ArrayStateType<T> array)
            throw new NotSupportedException($"Debugger state cannot hold a read-only list of {typeof(T).FullName}: arrays of it are not registered.");
        array.WriteArray(writer, [.. (IReadOnlyList<T>)value]);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind) =>
        throw new InvalidOperationException("A state names a compiler-made read-only list as the array it holds.");

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        var list = (IReadOnlyList<T>)value;
        for (int index = 0; index < list.Count; index++)
            children.Element(index, list[index]);
    }

    /// <inheritdoc/>
    public bool HoldsElementsOf(object array) => array is IReadOnlyList<T>;
}
