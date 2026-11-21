using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Writes one debugger state (#2602): every value framed by a marker, a shared object once and then by its number, an object
/// the host bound into the session by its path among the bound objects (#1507), and an object the game's static members
/// hold by its path among them (#1623).
/// </summary>
/// <param name="output">The stream's writer.</param>
/// <param name="types">The registry naming every type the state holds.</param>
/// <param name="bound">The host's bound objects, or none.</param>
/// <param name="definitions">The objects the game's static members hold.</param>
internal sealed class GraphWriter(BinaryWriter output, StateGraphTypes types, BoundObjectGraph? bound, DefinitionObjects definitions) : IStateGraphWriter
{
    /// <summary>Each shared object written so far, with its number.</summary>
    private readonly Dictionary<object, int> references = new(ReferenceEqualityComparer.Instance);

    /// <summary>The number the next object written takes.</summary>
    private int nextReferenceId = 1;

    /// <summary>The stream's writer.</summary>
    internal BinaryWriter Output { get; } = output;

    /// <summary>The registry.</summary>
    internal StateGraphTypes Types { get; } = types;

    /// <summary>
    /// Writes <paramref name="value"/>; a type the state cannot hold fails naming each object on the way to it, innermost
    /// first, so the failure says where in the graph it lies.
    /// </summary>
    /// <exception cref="NotSupportedException">The graph holds a value of a type no registration names.</exception>
    public void Write(object? value)
    {
        try
        {
            WriteCore(value);
        }
        catch (NotSupportedException unsupported)
        {
            throw new NotSupportedException($"{unsupported.Message} <- {value?.GetType().FullName}", unsupported);
        }
    }

    private void WriteCore(object? value)
    {
        if (value is null)
        {
            Output.Write((byte)ObjectMarker.Null);
            return;
        }
        StateGraphType type = Types.For(value);
        if (type.IsValueType)
        {
            Output.Write((byte)ObjectMarker.New);
            Output.Write(0);
            Output.Write(type.EncodedName);
            type.Write(this, value);
            return;
        }
        if (references.TryGetValue(value, out int existing))
        {
            Output.Write((byte)ObjectMarker.Reference);
            Output.Write(existing);
            return;
        }
        if (bound is not null && bound.TryGetPath(value, out string? path))
        {
            int boundId = nextReferenceId++;
            references.Add(value, boundId);
            Output.Write((byte)ObjectMarker.Bound);
            Output.Write(boundId);
            Output.Write(path);
            return;
        }
        // A string is written by its text: an interned literal a static member also holds is no identity.
        if (value is not string && definitions.TryGetPath(value, out string? definitionPath))
        {
            int definitionId = nextReferenceId++;
            references.Add(value, definitionId);
            Output.Write((byte)ObjectMarker.Definition);
            Output.Write(definitionId);
            Output.Write(definitionPath);
            return;
        }
        int referenceId = nextReferenceId++;
        references.Add(value, referenceId);
        Output.Write((byte)ObjectMarker.New);
        Output.Write(referenceId);
        Output.Write(type.EncodedName);
        type.Write(this, value);
    }

    /// <summary>Writes an element of a collection of <typeparamref name="T"/>: a value of the game's value type without boxing it.</summary>
    internal void WriteElement<T>(T value)
    {
        if (typeof(T).IsValueType && Types.NodeOf<T>() is { } node && node.Type == typeof(T))
        {
            Output.Write((byte)ObjectMarker.New);
            Output.Write(0);
            Output.Write(node.EncodedName);
            Output.Write((byte)PayloadKind.Fields);
            node.WriteTyped(this, value);
            return;
        }
        Write(value);
    }

    /// <summary>Writes a payload kind.</summary>
    internal void Kind(PayloadKind kind) => Output.Write((byte)kind);

    /// <inheritdoc/>
    public void Fields(int count) => Output.Write(count);

    /// <inheritdoc/>
    public void Field(StateGraphField field) => Output.Write(Ensure.NotNull(field).Encoded.Span);

    /// <summary>Writes a scalar framed as a value of its type.</summary>
    private void Scalar<T>(T value)
    {
        PrimitiveStateType<T> type = Types.Scalar<T>();
        Output.Write(type.Prefix);
        type.WriteValue(Output, value);
    }

    /// <inheritdoc/>
    public void WriteBoolean(bool value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteByte(byte value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteSByte(sbyte value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteInt16(short value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteUInt16(ushort value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteInt32(int value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteUInt32(uint value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteInt64(long value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteUInt64(ulong value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteChar(char value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteSingle(float value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteDouble(double value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteDecimal(decimal value) => Scalar(value);

    /// <inheritdoc/>
    public void WriteEnum<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        EnumStateType<TEnum> type = Types.EnumOf<TEnum>();
        Output.Write(type.Prefix);
        Output.Write(type.EncodedMember(value));
    }
}
