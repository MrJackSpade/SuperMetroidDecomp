namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>An object of the framework that holds no state of the game, such as a lock: a restore makes a new one.</summary>
/// <param name="type">The object's type.</param>
/// <param name="types">The registry.</param>
/// <param name="create">Makes a new one.</param>
internal sealed class PlainObjectStateType(Type type, StateGraphTypes types, Func<object> create) : StateGraphType(type, types)
{
    /// <inheritdoc/>
    protected override string Named() => Type.FullName ?? throw new InvalidOperationException("A framework type has a full name.");

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        writer.Kind(PayloadKind.Fields);
        writer.Fields(0);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.Fields)
            throw UnexpectedKind(kind);
        reader.Fields(0);
        return reader.Register(referenceId, create());
    }
}
