using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// A type of the game, written and restored through the members the compiler gave it (#2602): its levels of fields, each
/// field's value after its name, and restored by its restore constructor.
/// </summary>
/// <typeparam name="T">The type.</typeparam>
internal sealed class NodeStateType<T>(StateGraphTypes types, StateGraphName name, Action<IStateGraphWriter, T> write,
    Func<IStateGraphReader, T>? create, Action<T, IStateGraphVisitor> visit, string? packedSchema) : StateGraphType(typeof(T), types)
{
    /// <summary>
    /// For a value type whose fields are all scalars, enums or such value types, its schema, which an array of it is
    /// packed with; none otherwise.
    /// </summary>
    internal string? PackedSchema { get; } = packedSchema;

    /// <inheritdoc/>
    protected override string Named() => Types.Format(name);

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        writer.Kind(PayloadKind.Fields);
        write(writer, (T)value);
    }

    /// <summary>Writes a value's fields through <paramref name="writer"/>, without boxing it.</summary>
    internal void WriteTyped(IStateGraphWriter writer, T value) => write(writer, value);

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.Fields)
            throw UnexpectedKind(kind);
        reader.BeginCreate(referenceId, Name);
        object created = Create(reader)!;
        return reader.EndCreate(created);
    }

    /// <summary>Restores a value through <paramref name="reader"/>.</summary>
    /// <exception cref="InvalidDataException">The type is abstract, so no value of it is ever restored.</exception>
    internal T Create(IStateGraphReader reader) => create is not null
        ? create(reader)
        : throw new InvalidDataException($"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the state holds an object of the abstract {Name}.");

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children) => visit((T)value, children);

    /// <inheritdoc/>
    internal override void WritePacked(PackedWriter writer, object value) => write(writer, (T)value);

    /// <inheritdoc/>
    internal override object ReadPacked(PackedReader reader) => Create(reader)!;
}
