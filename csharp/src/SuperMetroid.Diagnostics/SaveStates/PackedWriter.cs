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
/// <param name="objects">The run's table of the objects its elements hold.</param>
internal sealed class PackedWriter(BinaryWriter output, StateGraphTypes types, PackedEnumTables tables, PackedObjects objects) : IStateGraphWriter
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

    /// <summary>
    /// Writes a value a field of a packed value type holds: an object as one more than its place in the run's table, none
    /// as zero; a value type, which is never none but in a nullable field, as whether it is there, then its packed value.
    /// </summary>
    public void Write(object? value)
    {
        if (value is null)
        {
            Output.Write7BitEncodedInt(0);
            return;
        }
        if (!value.GetType().IsValueType)
        {
            Output.Write7BitEncodedInt(objects.Place(value) + 1);
            return;
        }
        Output.Write(true);
        types.For(value).WritePacked(this, value);
    }

    /// <inheritdoc/>
    public void WriteScalar<T>(T value) where T : unmanaged => types.Scalar<T>().WriteValue(Output, value);

    /// <inheritdoc/>
    public void WriteValue<T>(T value) where T : struct
    {
        if (types.NodeOf<T>() is not { } node)
        {
            Write(value);
            return;
        }
        Output.Write(true);
        node.WriteTyped(this, value);
    }

    /// <inheritdoc/>
    public void WriteEnum<TEnum>(TEnum value) where TEnum : struct, Enum => Output.Write7BitEncodedInt(tables.Index(value));
}
