using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Writes the elements of an array of the game's value types as one packed run (#1696): each element's fields back to
/// back with no marker, type name or field name, which the run's schema names once; an enum member as its place in the
/// run's table of the enum.
/// </summary>
/// <param name="output">The run's values.</param>
/// <param name="types">The registry.</param>
/// <param name="tables">The run's enum tables.</param>
internal sealed class PackedWriter(BinaryWriter output, StateGraphTypes types, PackedEnumTables tables) : IStateGraphWriter
{
    /// <summary>The run's values.</summary>
    internal BinaryWriter Output { get; } = output;

    /// <inheritdoc/>
    public void Fields(int count)
    {
    }

    /// <inheritdoc/>
    public void Field(StateGraphField field)
    {
    }

    /// <summary>Writes a value a field of a packed value type holds: none, or a scalar, an enum member or a value type.</summary>
    public void Write(object? value)
    {
        Output.Write(value is not null);
        if (value is not null)
            types.For(value).WritePacked(this, value);
    }

    /// <inheritdoc/>
    public void WriteBoolean(bool value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteByte(byte value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteSByte(sbyte value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteInt16(short value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteUInt16(ushort value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteInt32(int value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteUInt32(uint value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteInt64(long value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteUInt64(ulong value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteChar(char value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteSingle(float value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteDouble(double value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteDecimal(decimal value) => Output.Write(value);

    /// <inheritdoc/>
    public void WriteEnum<TEnum>(TEnum value) where TEnum : struct, Enum => Output.Write7BitEncodedInt(tables.Index(value));
}
