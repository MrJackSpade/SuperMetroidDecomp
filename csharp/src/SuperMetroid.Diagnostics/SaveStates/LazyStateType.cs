namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A lazily loaded part of <typeparamref name="T"/>: its value, loaded when the state is written.</summary>
/// <typeparam name="T">The value type.</typeparam>
internal sealed class LazyStateType<T>(StateGraphTypes types) : StateGraphType(typeof(Lazy<T>), types)
{
    /// <inheritdoc/>
    protected override string Named() => "System.Lazy`1<" + Types.NameOf(typeof(T)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        writer.Kind(PayloadKind.Lazy);
        writer.WriteElement(((Lazy<T>)value).Value);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.Lazy)
            throw UnexpectedKind(kind);
        var box = new RestoredValue();
        var lazy = new Lazy<T>(() => box.Value!);
        reader.Register(referenceId, lazy);
        box.Value = reader.ReadElement<T>();
        _ = lazy.Value;
        return lazy;
    }

    /// <inheritdoc/>
    internal override bool IsLazy => true;

    /// <inheritdoc/>
    internal override object? LazyValue(object value, bool force)
    {
        var lazy = (Lazy<T>)value;
        return force || lazy.IsValueCreated ? lazy.Value : null;
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        var lazy = (Lazy<T>)value;
        if (lazy.IsValueCreated)
            children.Part("=value", lazy.Value);
    }

    /// <summary>The value a restored part holds, read after the part is registered so objects inside it can hold it.</summary>
    private sealed class RestoredValue
    {
        /// <summary>The value.</summary>
        internal T? Value { get; set; }
    }
}
