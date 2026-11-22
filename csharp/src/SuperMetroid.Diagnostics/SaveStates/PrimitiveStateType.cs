namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A scalar a state writes through its fixed binary representation.</summary>
/// <typeparam name="T">The scalar.</typeparam>
internal sealed class PrimitiveStateType<T>(StateGraphTypes types, string name, Action<BinaryWriter, T> write, Func<BinaryReader, T> read)
    : StateGraphType(typeof(T), types)
{
    /// <inheritdoc/>
    internal override bool ReachesObjects => false;

    /// <inheritdoc/>
    protected override string Named() => name;

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        writer.Kind(PayloadKind.Primitive);
        write(writer.Output, (T)value);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind) =>
        kind == PayloadKind.Primitive ? read(reader.Input)! : throw UnexpectedKind(kind);
}
