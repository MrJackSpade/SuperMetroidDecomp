namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A framework singleton, such as a comparer a collection is keyed by: a restore holds this process's instance.</summary>
/// <param name="types">The registry.</param>
/// <param name="name">The singleton's name.</param>
/// <param name="instance">The singleton.</param>
internal sealed class SingletonStateType(StateGraphTypes types, string name, object instance) : StateGraphType(instance.GetType(), types)
{
    /// <summary>The singleton.</summary>
    internal object Instance { get; } = instance;

    /// <inheritdoc/>
    protected override string Named() => name;

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
        return reader.Register(referenceId, Instance);
    }
}
