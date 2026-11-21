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
    public void WriteScalar<T>(T value) where T : unmanaged => types.Scalar<T>().WriteValue(Output, value);

    /// <inheritdoc/>
    public void WriteEnum<TEnum>(TEnum value) where TEnum : struct, Enum => Output.Write7BitEncodedInt(tables.Index(value));
}
