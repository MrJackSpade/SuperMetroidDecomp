using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>Reads the elements of a packed run back (#1696), in the order <see cref="PackedWriter"/> wrote them.</summary>
/// <param name="input">The run's values.</param>
/// <param name="types">The registry.</param>
/// <param name="tables">The run's enum tables.</param>
internal sealed class PackedReader(BinaryReader input, StateGraphTypes types, PackedEnumTables tables) : IStateGraphReader
{
    /// <summary>The run's values.</summary>
    internal BinaryReader Input { get; } = input;

    /// <inheritdoc/>
    public void Fields(int count)
    {
    }

    /// <inheritdoc/>
    public void Field(StateGraphField field)
    {
    }

    /// <inheritdoc/>
    public void Created(object instance)
    {
    }

    /// <inheritdoc/>
    public T Read<T>()
    {
        if (!Input.ReadBoolean())
            return default(T) is null ? default! : throw new InvalidDataException($"A packed run holds no value where this build stores {typeof(T).FullName}.");
        Type stored = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        StateGraphType type = types.Registered(stored)
            ?? throw new InvalidDataException($"{DebuggerObjectGraphSerializer.UnsupportedVersion}: a packed run holds a {stored.FullName}, which this build does not name.");
        return (T)type.ReadPacked(this);
    }

    /// <inheritdoc/>
    public T Argument<T>(int ordinal, int count, StateGraphField field, out T value)
    {
        value = Read<T>();
        return value;
    }

    /// <inheritdoc/>
    public void Fixed<T>(T current)
    {
        if (!EqualityComparer<T>.Default.Equals(Read<T>(), current))
            throw new InvalidDataException($"{DebuggerObjectGraphSerializer.UnsupportedVersion}: a packed value holds what only its construction sets, and rebuilding it set another.");
    }

    /// <inheritdoc/>
    public bool ReadBoolean() => Input.ReadBoolean();

    /// <inheritdoc/>
    public byte ReadByte() => Input.ReadByte();

    /// <inheritdoc/>
    public sbyte ReadSByte() => Input.ReadSByte();

    /// <inheritdoc/>
    public short ReadInt16() => Input.ReadInt16();

    /// <inheritdoc/>
    public ushort ReadUInt16() => Input.ReadUInt16();

    /// <inheritdoc/>
    public int ReadInt32() => Input.ReadInt32();

    /// <inheritdoc/>
    public uint ReadUInt32() => Input.ReadUInt32();

    /// <inheritdoc/>
    public long ReadInt64() => Input.ReadInt64();

    /// <inheritdoc/>
    public ulong ReadUInt64() => Input.ReadUInt64();

    /// <inheritdoc/>
    public char ReadChar() => Input.ReadChar();

    /// <inheritdoc/>
    public float ReadSingle() => Input.ReadSingle();

    /// <inheritdoc/>
    public double ReadDouble() => Input.ReadDouble();

    /// <inheritdoc/>
    public decimal ReadDecimal() => Input.ReadDecimal();

    /// <inheritdoc/>
    public TEnum ReadEnum<TEnum>() where TEnum : struct, Enum => tables.Member<TEnum>(Input.Read7BitEncodedInt());
}
