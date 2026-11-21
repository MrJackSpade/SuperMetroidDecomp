using System.Buffers;
using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Reads one debugger state back (#2602) against this build's types and exact field layout: every type name, field name,
/// field count and payload kind is checked, so a state of another layout is refused as an unsupported version rather than
/// misread. An object is registered under its number as soon as it exists, so objects inside it that hold it are restored
/// holding it; hash-keyed collections are filled once the whole state is read, every key whole by then (#1681).
/// </summary>
/// <param name="input">The stream's reader.</param>
/// <param name="types">The registry naming every type the state may hold.</param>
/// <param name="bound">The host's bound objects, or none.</param>
/// <param name="definitions">The objects the game's static members hold.</param>
internal sealed class GraphReader(BinaryReader input, StateGraphTypes types, BoundObjectGraph? bound, DefinitionObjects definitions) : IStateGraphReader
{
    /// <summary>Each object read so far, by its number.</summary>
    private readonly Dictionary<int, object> references = [];

    /// <summary>The numbers of the objects being created, innermost last, each with whether it is registered yet.</summary>
    private readonly List<(int Id, bool Registered, string Name)> creating = [];

    /// <summary>The objects whose fields are still being read, which a collection built at once cannot hash yet.</summary>
    private readonly HashSet<object> restoring = new(ReferenceEqualityComparer.Instance);

    /// <summary>The fills of hash-keyed collections, run in order once the whole state is read.</summary>
    private readonly List<Action> unfilled = [];

    /// <summary>The single instance of each name read during this restore.</summary>
    private readonly Dictionary<string, string> names = new(StringComparer.Ordinal);

    /// <summary>Each name the state has stored, by its number.</summary>
    private readonly List<string> numberedNames = [];

    /// <summary>Each name instance resolved, with its handling.</summary>
    private readonly Dictionary<string, StateGraphType> resolved = new(ReferenceEqualityComparer.Instance);

    /// <summary>How deep the read is inside elements of arrays of value types, where a zero enum is unused storage.</summary>
    private int structElementDepth;

    /// <summary>The stream's reader.</summary>
    internal BinaryReader Input { get; } = input;

    /// <summary>The registry.</summary>
    internal StateGraphTypes Types { get; } = types;

    /// <summary>Whether the read is inside an element of an array of value types.</summary>
    internal bool InStructElements => structElementDepth > 0;

    /// <summary>Reads a whole state, then fills its hash-keyed collections from their saved entries.</summary>
    internal object? ReadState()
    {
        object? root = Read();
        foreach (Action fill in unfilled)
            fill();
        unfilled.Clear();
        return root;
    }

    /// <summary>Defers a hash-keyed collection's fill until the whole state is read.</summary>
    internal void Defer(Action fill) => unfilled.Add(fill);

    /// <summary>Whether <paramref name="value"/>'s fields are still being read.</summary>
    internal bool IsRestoring(object value) => restoring.Contains(value);

    /// <summary>Reads one value, resolving references and rejecting unknown markers.</summary>
    internal object? Read()
    {
        var marker = (ObjectMarker)Input.ReadByte();
        switch (marker)
        {
            case ObjectMarker.Null:
                return null;
            case ObjectMarker.Reference:
            {
                int id = Input.ReadInt32();
                if (references.TryGetValue(id, out object? existing))
                    return existing;
                string inside = creating.Find(entry => entry.Id == id).Name;
                throw new InvalidDataException(inside is null
                    ? $"Debugger state references object {id} before its definition."
                    : $"Debugger state cannot rebuild the saved {inside}: an argument its constructor takes holds the object itself, which does not exist until that constructor runs.");
            }
            case ObjectMarker.Bound:
            {
                int id = Input.ReadInt32();
                string path = Input.ReadString();
                return Register(id, (bound ?? throw new InvalidDataException(
                    "Debugger state names a bound object, but no bound content was supplied to restore it.")).Resolve(path));
            }
            case ObjectMarker.Definition:
            {
                int id = Input.ReadInt32();
                return Register(id, definitions.Resolve(Input.ReadString()));
            }
            case ObjectMarker.New:
            {
                int id = Input.ReadInt32();
                StateGraphType type = Resolve(ReadName());
                var kind = (PayloadKind)Input.ReadByte();
                return type.Read(this, id, kind);
            }
            default:
                throw new InvalidDataException($"Unknown debugger-state object marker {(byte)marker}.");
        }
    }

    /// <summary>Begins creating an object of <paramref name="name"/> numbered <paramref name="id"/>.</summary>
    internal void BeginCreate(int id, string name) => creating.Add((id, false, name));

    /// <summary>Ends creating the innermost object: it must have been registered unless it is a value.</summary>
    internal object EndCreate(object created)
    {
        (int id, bool registered, string name) = creating[^1];
        creating.RemoveAt(creating.Count - 1);
        if (id != 0 && !registered)
            throw new InvalidOperationException($"The restore of {name} never recorded the object it made.");
        if (id != 0)
            restoring.Remove(created);
        if (created is SuperMetroid.AssetExtraction.Installation.IRestoredSharedContent shared)
            shared.ReattachSharedContent();
        return created;
    }

    /// <inheritdoc/>
    public void Created(object instance)
    {
        Ensure.NotNull(instance);
        (int id, bool registered, string name) = creating[^1];
        if (registered)
            return;
        creating[^1] = (id, true, name);
        if (id == 0)
            return;
        Register(id, instance);
        restoring.Add(instance);
    }

    /// <summary>Publishes a unique positive number so later references can resolve to it.</summary>
    internal object Register(int id, object value)
    {
        if (id <= 0 || !references.TryAdd(id, value))
            throw new InvalidDataException($"Debugger state has invalid or duplicate object ID {id}.");
        return value;
    }

    /// <summary>
    /// Reads a name: its number, one more than the number of a name the state stored before, or zero and then its text,
    /// which takes the next number. Returns the restore's single instance of that text.
    /// </summary>
    /// <exception cref="InvalidDataException">The state names a name it has not stored.</exception>
    internal string ReadName()
    {
        int reference = Input.Read7BitEncodedInt();
        if (reference != 0)
        {
            return reference > 0 && reference <= numberedNames.Count
                ? numberedNames[reference - 1]
                : throw new InvalidDataException($"Debugger state names name {reference - 1}, but it has stored {numberedNames.Count}.");
        }
        string name = ReadNameText();
        numberedNames.Add(name);
        return name;
    }

    /// <summary>Reads a name's stored text, returning the restore's single instance of that text.</summary>
    private string ReadNameText()
    {
        int byteCount = Input.Read7BitEncodedInt();
        if (byteCount < 0)
            throw new InvalidDataException("Debugger state has a negative name length.");
        byte[] rented = ArrayPool<byte>.Shared.Rent(byteCount);
        try
        {
            Span<byte> utf8 = rented.AsSpan(0, byteCount);
            Input.BaseStream.ReadExactly(utf8);
            int charCount = System.Text.Encoding.UTF8.GetCharCount(utf8);
            char[] rentedChars = ArrayPool<char>.Shared.Rent(charCount);
            try
            {
                Span<char> text = rentedChars.AsSpan(0, charCount);
                System.Text.Encoding.UTF8.GetChars(utf8, text);
                Dictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> lookup = names.GetAlternateLookup<ReadOnlySpan<char>>();
                if (lookup.TryGetValue(text, out string? known))
                    return known;
                string name = new(text);
                names.Add(name, name);
                return name;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(rentedChars);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private StateGraphType Resolve(string name)
    {
        if (resolved.TryGetValue(name, out StateGraphType? known))
            return known;
        StateGraphType type = Types.Named(name);
        resolved.Add(name, type);
        return type;
    }

    /// <summary>Reads a count stored as a non-negative integer.</summary>
    internal int ReadLength(string label)
    {
        int value = Input.ReadInt32();
        return value >= 0 ? value : throw new InvalidDataException($"Debugger state has negative {label} length {value}.");
    }

    /// <summary>Reads the elements of an array of value types, where a zero enum is unused storage.</summary>
    internal void InStructElementsDo(Action read)
    {
        structElementDepth++;
        try
        {
            read();
        }
        finally
        {
            structElementDepth--;
        }
    }

    /// <summary>Reads an element of a collection of <typeparamref name="T"/>.</summary>
    internal T ReadElement<T>() => Read<T>();

    /// <inheritdoc/>
    public T Read<T>()
    {
        object? value = Read();
        if (value is T typed)
            return typed;
        if (value is null && default(T) is null)
            return default!;
        throw new InvalidDataException(
            $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the state holds {value?.GetType().FullName ?? "nothing"} where this build stores {typeof(T).FullName}.");
    }

    /// <inheritdoc/>
    public void Fields(int count)
    {
        int stored = Input.ReadInt32();
        if (stored != count)
        {
            throw new InvalidDataException(
                $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the saved {(creating.Count > 0 ? creating[^1].Name : "object")} has {stored} fields where this build has {count}.");
        }
    }

    /// <inheritdoc/>
    public void Field(StateGraphField field)
    {
        Ensure.NotNull(field);
        string declaring = ReadName();
        string name = ReadName();
        if (declaring != field.DeclaringType || name != field.Name)
        {
            throw new InvalidDataException(
                $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the saved state has field {declaring}::{name} where this build has {field}.");
        }
    }

    /// <inheritdoc/>
    public T Argument<T>(int ordinal, int count, StateGraphField field, out T value)
    {
        if (ordinal == 0)
            Fields(count);
        Field(field);
        value = Read<T>();
        return value;
    }

    /// <inheritdoc/>
    public void Fixed<T>(T current)
    {
        T saved = Read<T>();
        if (!EqualityComparer<T>.Default.Equals(saved, current))
        {
            throw new InvalidDataException(
                $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the saved {(creating.Count > 0 ? creating[^1].Name : "object")} holds a value only its construction sets, and rebuilding it set another.");
        }
    }

    /// <summary>Reads a scalar framed as a value of its type, refusing a value of another type.</summary>
    private T Scalar<T>()
    {
        PrimitiveStateType<T> expected = Types.Scalar<T>();
        var marker = (ObjectMarker)Input.ReadByte();
        if (marker != ObjectMarker.New)
            throw Retyped(expected.Name, marker.ToString());
        _ = Input.ReadInt32();
        string name = ReadName();
        if (name != expected.Name)
            throw Retyped(expected.Name, name);
        var kind = (PayloadKind)Input.ReadByte();
        if (kind != PayloadKind.Primitive)
            throw Retyped(expected.Name, kind.ToString());
        return expected.ReadValue(Input);
    }

    private static InvalidDataException Retyped(string expected, string found) => new(
        $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the state holds {found} where this build stores {expected}.");

    /// <inheritdoc/>
    public T ReadScalar<T>() where T : unmanaged => Scalar<T>();

    /// <inheritdoc/>
    public TEnum ReadEnum<TEnum>() where TEnum : struct, Enum
    {
        EnumStateType<TEnum> expected = Types.EnumOf<TEnum>();
        var marker = (ObjectMarker)Input.ReadByte();
        if (marker != ObjectMarker.New)
            throw Retyped(expected.Name, marker.ToString());
        _ = Input.ReadInt32();
        string name = ReadName();
        if (name != expected.Name)
            throw Retyped(expected.Name, name);
        var kind = (PayloadKind)Input.ReadByte();
        if (kind != PayloadKind.EnumName)
            throw Retyped(expected.Name, kind.ToString());
        return expected.Decode(ReadName(), InStructElements);
    }
}
