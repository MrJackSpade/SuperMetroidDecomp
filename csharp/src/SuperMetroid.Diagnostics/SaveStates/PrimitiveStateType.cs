namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A scalar a state writes through its fixed binary representation.</summary>
/// <typeparam name="T">The scalar.</typeparam>
internal sealed class PrimitiveStateType<T>(StateGraphTypes types, string name, Action<BinaryWriter, T> write, Func<BinaryReader, T> read)
    : StateGraphType(typeof(T), types)
{
    /// <summary>What every framed value of the scalar starts with: its marker, number zero, name and payload kind.</summary>
    internal byte[] Prefix => prefix ??= [(byte)ObjectMarker.New, 0, 0, 0, 0, .. EncodedName, (byte)PayloadKind.Primitive];

    private byte[]? prefix;

    /// <inheritdoc/>
    protected override string Named() => name;

    /// <summary>Writes the scalar's bytes.</summary>
    internal void WriteValue(BinaryWriter writer, T value) => write(writer, value);

    /// <summary>Reads the scalar's bytes.</summary>
    internal T ReadValue(BinaryReader reader) => read(reader);

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        writer.Kind(PayloadKind.Primitive);
        write(writer.Output, (T)value);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind) =>
        kind == PayloadKind.Primitive ? read(reader.Input)! : throw UnexpectedKind(kind);

    /// <inheritdoc/>
    internal override void WritePacked(PackedWriter writer, object value) => write(writer.Output, (T)value);

    /// <inheritdoc/>
    internal override object ReadPacked(PackedReader reader) => read(reader.Input)!;
}
