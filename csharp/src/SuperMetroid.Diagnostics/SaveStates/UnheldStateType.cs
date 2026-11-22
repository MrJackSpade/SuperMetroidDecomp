namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// A framework object a debugger state never holds, such as a cache a definition keeps beside its data (#1681): it reaches
/// nothing a state names, and its keys' hashes and lifetimes belong to the capturing process, so a capture holding one is
/// refused.
/// </summary>
/// <param name="type">The object's type.</param>
/// <param name="types">The registry.</param>
internal sealed class UnheldStateType(Type type, StateGraphTypes types) : StateGraphType(type, types)
{
    /// <inheritdoc/>
    protected override string Named() => Type.FullName ?? throw new InvalidOperationException("A framework type has a full name.");

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value) =>
        throw new NotSupportedException($"Debugger state cannot hold the {Name}: it has no rebuild, and its contents belong to the capturing process.");

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind) =>
        throw new InvalidDataException($"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the state holds a {Name}, which no state holds.");
}
