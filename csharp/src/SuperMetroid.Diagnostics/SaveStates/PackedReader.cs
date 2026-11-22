using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>Reads the elements of a packed run back (#1696), in the order <see cref="PackedWriter"/> wrote them.</summary>
/// <param name="input">The run's values.</param>
/// <param name="types">The registry.</param>
/// <param name="tables">The run's enum tables.</param>
/// <param name="objects">The run's table of the objects its elements hold.</param>
internal sealed class PackedReader(BinaryReader input, StateGraphTypes types, PackedEnumTables tables, PackedObjects objects) : IStateGraphReader
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
        if (!typeof(T).IsValueType)
        {
            int place = Input.Read7BitEncodedInt();
            object? held = place == 0 ? null : objects.At(place - 1);
            if (held is T typed)
                return typed;
            if (held is null)
                return default!;
            throw new InvalidDataException(
                $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: a packed run holds a {held.GetType().FullName} where this build stores {typeof(T).FullName}.");
        }
        if (!Input.ReadBoolean())
            return default(T) is null ? default! : throw new InvalidDataException($"A packed run holds no value where this build stores {typeof(T).FullName}.");
        Type stored = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        StateGraphType type = types.Registered(stored)
            ?? throw new InvalidDataException($"{DebuggerObjectGraphSerializer.UnsupportedVersion}: a packed run holds a {stored.FullName}, which this build does not name.");
        return (T)type.ReadPacked(this);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// An argument is read as the generated writer wrote it: a scalar as its raw value and an enum as its place in the run's
    /// table of its members, as <see cref="ReadScalar{T}"/> and <see cref="ReadEnum{TEnum}"/> read a field's; any other
    /// value as <see cref="Read{T}"/> does.
    /// </remarks>
    public T Argument<T>(int ordinal, int count, StateGraphField field, out T value)
    {
        if (types.Registered(typeof(T)) is PrimitiveStateType<T> scalar)
            value = scalar.ReadValue(Input);
        else if (typeof(T).IsEnum)
            value = tables.MemberOf<T>(Input.Read7BitEncodedInt());
        else
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
    public T ReadScalar<T>() where T : unmanaged => types.Scalar<T>().ReadValue(Input);

    /// <inheritdoc/>
    public TEnum ReadEnum<TEnum>() where TEnum : struct, Enum => tables.Member<TEnum>(Input.Read7BitEncodedInt());
}
