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

    /// <summary>Each name written so far, with its number: its first use stores its text, every later use its number.</summary>
    private readonly Dictionary<string, int> names = new(StringComparer.Ordinal);

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
            WriteName(type.Name);
            type.Write(this, value);
            return;
        }
        if (references.TryGetValue(value, out int existing))
        {
            Output.Write((byte)ObjectMarker.Reference);
            Output.Write(existing);
            return;
        }
        // A framework singleton, such as a comparer, is the same object in every process, so a restore holds this process's
        // instance: it is never looked for among the bound objects or the catalogs, where any collection keyed by it holds it.
        bool singleton = type is SingletonStateType;
        if (!singleton && bound is not null && bound.TryGetPath(value, out string? path))
        {
            int boundId = nextReferenceId++;
            references.Add(value, boundId);
            Output.Write((byte)ObjectMarker.Bound);
            Output.Write(boundId);
            Output.Write(path);
            return;
        }
        // A string is written by its text: an interned literal a static member also holds is no identity.
        if (!singleton && value is not string && definitions.TryGetPath(value, out string? definitionPath))
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
        WriteName(type.Name);
        type.Write(this, value);
    }

    /// <summary>
    /// Writes a name: the first time the state names it, zero and then its text, which takes the next number; every later
    /// time, one more than its number (#2602), so a type, field or enum member a state holds many of costs its text once.
    /// </summary>
    internal void WriteName(string name)
    {
        if (names.TryGetValue(name, out int number))
        {
            Output.Write7BitEncodedInt(number + 1);
            return;
        }
        names.Add(name, names.Count);
        Output.Write7BitEncodedInt(0);
        Output.Write(name);
    }

    /// <summary>Writes an element of a collection of <typeparamref name="T"/>: a value of the game's value type without boxing it.</summary>
    internal void WriteElement<T>(T value)
    {
        if (typeof(T).IsValueType && Types.NodeOf<T>() is { } node && node.Type == typeof(T))
        {
            Output.Write((byte)ObjectMarker.New);
            Output.Write(0);
            WriteName(node.Name);
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
}
