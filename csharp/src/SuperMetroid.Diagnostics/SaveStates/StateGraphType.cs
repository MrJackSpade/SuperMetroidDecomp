namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// How a debugger state holds every value of one runtime type (#2602): the type's name, how a value's payload is written
/// and read back, and what a value holds that can reach another object.
/// </summary>
/// <param name="type">The runtime type.</param>
/// <param name="types">The registry the type's name and its parts' types are resolved through.</param>
internal abstract class StateGraphType(Type type, StateGraphTypes types)
{
    /// <summary>The cached name, worked out on first use, once every type it names is registered.</summary>
    private string? name;

    /// <summary>The cached encoded name.</summary>
    private byte[]? encodedName;

    /// <summary>The runtime type.</summary>
    internal Type Type { get; } = type;

    /// <summary>Whether values of the type are values, which a state never shares.</summary>
    internal bool IsValueType { get; } = type.IsValueType;

    /// <summary>The registry.</summary>
    protected StateGraphTypes Types { get; } = types;

    /// <summary>The name a state gives the type.</summary>
    internal string Name => name ??= Named();

    /// <summary>The name as a state stores it.</summary>
    internal byte[] EncodedName => encodedName ??= StateGraphNames.Encode(Name);

    /// <summary>Works out the type's name.</summary>
    protected abstract string Named();

    /// <summary>Writes a value's payload kind and payload, after its marker and name.</summary>
    /// <param name="writer">The state being written.</param>
    /// <param name="value">The value, of this type.</param>
    internal abstract void Write(GraphWriter writer, object value);

    /// <summary>Reads a value's payload of <paramref name="kind"/>, registering an object under <paramref name="referenceId"/> before what it holds.</summary>
    /// <param name="reader">The state being read.</param>
    /// <param name="referenceId">The object's number; zero for a value.</param>
    /// <param name="kind">The payload kind the state stores.</param>
    internal abstract object Read(GraphReader reader, int referenceId, PayloadKind kind);

    /// <summary>Hands each value a value of the type holds that can reach another object to <paramref name="children"/>.</summary>
    /// <param name="value">The value.</param>
    /// <param name="children">Receives the values.</param>
    internal virtual void Visit(object value, IStateGraphChildren children)
    {
    }

    /// <summary>
    /// For a collection content fills on demand, a number every change to it advances; none for any other type.
    /// </summary>
    /// <param name="value">The value.</param>
    internal virtual int? Version(object value) => null;

    /// <summary>For a lazily loaded part, its value: loaded now when <paramref name="force"/>, else only when already loaded.</summary>
    /// <param name="value">The part.</param>
    /// <param name="force">Whether to load the value.</param>
    internal virtual object? LazyValue(object value, bool force) => null;

    /// <summary>Whether values of the type are lazily loaded parts.</summary>
    internal virtual bool IsLazy => false;

    /// <summary>Writes a value inside a packed run (#1696): a scalar's bytes, an enum member's place, a value type's fields.</summary>
    /// <exception cref="NotSupportedException">Values of the type are never packed.</exception>
    internal virtual void WritePacked(PackedWriter writer, object value) =>
        throw new NotSupportedException($"Debugger state cannot pack a {Name}.");

    /// <summary>Reads a value inside a packed run.</summary>
    /// <exception cref="InvalidDataException">Values of the type are never packed.</exception>
    internal virtual object ReadPacked(PackedReader reader) =>
        throw new InvalidDataException($"{DebuggerObjectGraphSerializer.UnsupportedVersion}: a packed run holds a {Name}, which this build never packs.");

    /// <summary>Rejects a payload kind this type does not store.</summary>
    protected InvalidDataException UnexpectedKind(PayloadKind kind) => new(
        $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the state stores {Name} as {kind}, which this build does not.");
}
