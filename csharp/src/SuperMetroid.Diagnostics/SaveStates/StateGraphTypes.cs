using System.Collections.Concurrent;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Every type a debugger state can hold, as the compiler registered it (#2602): the game's types through their generated
/// members, the collections, arrays and enums their fields are declared with, the methods delegates are made from, and the
/// definition catalogs. A state finds a value's type by its runtime type, or for a collection the framework specializes
/// (a frozen collection, the read-only list the compiler makes for a collection expression) by asking each registration
/// that accepts it; it names nothing it was not told about.
/// </summary>
internal sealed partial class StateGraphTypes : IStateGraphRegistry
{
    /// <summary>The process's registry: the game's assemblies, and whatever verification adds.</summary>
    internal static StateGraphTypes Shared => shared.Value;

    private static readonly Lazy<StateGraphTypes> shared = new(static () =>
    {
        var types = new StateGraphTypes();
        types.Add(SuperMetroid.Core.GeneratedStateGraph.Register);
        types.Add(SuperMetroid.AssetExtraction.GeneratedStateGraph.Register);
        types.Add(SuperMetroid.Diagnostics.GeneratedStateGraph.Register);
        return types;
    });

    /// <summary>Guards registration and the indexes built from it.</summary>
    private readonly Lock gate = new();

    /// <summary>Each registered runtime type's handling.</summary>
    private readonly ConcurrentDictionary<Type, StateGraphType> byType = new();

    /// <summary>The names of types no value of which is written by its own handling: static and abstract types.</summary>
    private readonly ConcurrentDictionary<Type, StateGraphName> names = new();

    /// <summary>The types whose registration has begun.</summary>
    private readonly HashSet<Type> begun = [];

    /// <summary>The registrations that accept values of types the framework makes, asked in turn when a runtime type is not registered.</summary>
    private readonly List<StateGraphProbe> probes = [];

    /// <summary>Each runtime type a probe accepted, with its handling.</summary>
    private readonly ConcurrentDictionary<Type, StateGraphType> probed = new();

    /// <summary>Each name's handling, built on the first restore after a registration.</summary>
    private Dictionary<string, StateGraphType>? byName;

    /// <summary>Each name's type, built with <see cref="byName"/>.</summary>
    private Dictionary<string, Type>? typesByName;

    /// <summary>Starts a registry holding the scalars, strings and the framework's own stateless objects.</summary>
    internal StateGraphTypes()
    {
        RegisterScalars();
        Put(new StringStateType(this));
        Put(new PlainObjectStateType(typeof(object), this, static () => new object()));
        Put(new PlainObjectStateType(typeof(Lock), this, static () => new Lock()));
        Array<object>();
        probes.Add(new SingletonProbe(new SingletonStateType(this, "System.StringComparer.Ordinal", StringComparer.Ordinal)));
        probes.Add(new SingletonProbe(new SingletonStateType(this, "System.StringComparer.OrdinalIgnoreCase", StringComparer.OrdinalIgnoreCase)));
        probes.Add(new SingletonProbe(new SingletonStateType(this, "System.Collections.Generic.ReferenceEqualityComparer", ReferenceEqualityComparer.Instance)));
        // A type value is the runtime's own specialization of Type, accepted by what it is rather than registered by type.
        probes.Add(new TypedProbe<Type>(new TypeIdentityStateType(this)));
    }

    /// <summary>The catalogs registered: each type's name and its static read-only members holding objects.</summary>
    internal List<(StateGraphName Type, IReadOnlyList<StateGraphCatalogMember> Members)> Catalogs { get; } = [];

    /// <summary>The declared types the catalogs' members reach, each with the test of whether an object is of it (#1716).</summary>
    internal Dictionary<string, (Func<int, object, bool> Probe, int Index)> DeclaredProbes { get; } = new(StringComparer.Ordinal);

    /// <summary>What a location declared with each declared type holds (#1716).</summary>
    internal Dictionary<string, (bool Anything, IReadOnlyList<string> Held)> DeclaredHolds { get; } = new(StringComparer.Ordinal);

    /// <summary>Adds an assembly's registration.</summary>
    /// <param name="registration">The assembly's generated registration.</param>
    internal void Add(Action<IStateGraphRegistry> registration)
    {
        Ensure.NotNull(registration);
        lock (gate)
        {
            registration(this);
            byName = null;
            typesByName = null;
        }
    }

    /// <summary>The handling of <paramref name="value"/>'s type.</summary>
    /// <exception cref="NotSupportedException">No registration names the value's type.</exception>
    internal StateGraphType For(object value) => Find(value) ?? throw new NotSupportedException(value is Delegate
        ? $"Debugger state cannot hold a {value.GetType().FullName}: no type registers a method it is made from, so it calls a lambda or another assembly's method"
        : $"Debugger state cannot hold {value.GetType().FullName}: no generated members or registered rebuild name it");

    /// <summary>The handling of <paramref name="value"/>'s type, or none when no registration names it.</summary>
    internal StateGraphType? Find(object value)
    {
        Type type = value.GetType();
        if (byType.TryGetValue(type, out StateGraphType? known) || probed.TryGetValue(type, out known))
            return known;
        StateGraphType? found = null;
        lock (gate)
        {
            foreach (StateGraphProbe probe in probes)
            {
                if (probe.Accepts(value) && (found is null || probe.MoreSpecificThan(found)))
                    found = probe.Type;
            }
        }
        if (found is not null)
        {
            probed.TryAdd(type, found);
            return found;
        }
        // A construction of a generic type of the game made where no declaration names it registers itself.
        if (value is IStateGraphNode node)
        {
            lock (gate)
            {
                node.Describe(this);
                byName = null;
                typesByName = null;
            }
            return byType.TryGetValue(type, out known) ? known : null;
        }
        return null;
    }

    /// <summary>The registered handling of values of exactly <paramref name="type"/>, or none.</summary>
    internal StateGraphType? Registered(Type type) => byType.TryGetValue(type, out StateGraphType? known) ? known : null;

    /// <summary>The name a state gives <paramref name="type"/>.</summary>
    /// <exception cref="NotSupportedException">No registration names the type.</exception>
    internal string NameOf(Type type)
    {
        // A constructed type, or one no registration names (an interface a collection is declared over), is named by its
        // full name, the same whenever and wherever its construction is registered.
        if (type.IsGenericType || (!byType.ContainsKey(type) && !names.ContainsKey(type)))
            return type.FullName ?? throw new NotSupportedException($"Debugger state cannot name {type}: it has no full name.");
        if (byType.TryGetValue(type, out StateGraphType? known))
            return known.Name;
        return Format(names[type]);
    }

    /// <summary>A registered name with its type arguments' names.</summary>
    internal string Format(StateGraphName name) => name.Arguments.Count == 0
        ? name.Declared
        : name.Declared + "<" + string.Join(",", name.Arguments.Select(NameOf)) + ">";

    /// <summary>The handling a state's type name names.</summary>
    /// <exception cref="InvalidDataException">This build has no type of the name.</exception>
    internal StateGraphType Named(string name)
    {
        BuildNames();
        return byName!.TryGetValue(name, out StateGraphType? type)
            ? type
            : throw new InvalidDataException(
                $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the state names type '{name}', which this build does not have.");
    }

    /// <summary>The type a state's type identity names (#2060).</summary>
    /// <exception cref="InvalidDataException">This build has no type of the name.</exception>
    internal Type TypeNamed(string name)
    {
        BuildNames();
        return typesByName!.TryGetValue(name, out Type? type)
            ? type
            : throw new InvalidDataException(
                $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the state names type '{name}', which this build does not have.");
    }

    private void BuildNames()
    {
        if (byName is not null && typesByName is not null)
            return;
        lock (gate)
        {
            var handled = new Dictionary<string, StateGraphType>(StringComparer.Ordinal);
            var typed = new Dictionary<string, Type>(StringComparer.Ordinal);
            foreach (StateGraphType type in byType.Values.Concat(probes.Select(probe => probe.Type)))
            {
                // A compiler-made list is written as the array it holds, under the array's name.
                if (type is ICompilerReadOnlyList)
                    continue;
                if (handled.TryGetValue(type.Name, out StateGraphType? other) && !ReferenceEquals(other, type) && other.Type != type.Type)
                    throw new InvalidOperationException($"Debugger state types {other.Type.FullName} and {type.Type.FullName} share the name {type.Name}.");
                handled.TryAdd(type.Name, type);
                typed.TryAdd(type.Name, type.Type);
            }
            foreach ((Type type, StateGraphName name) in names)
                typed.TryAdd(Format(name), type);
            // A type value of a constructed type is named by its full name (NameOf).
            foreach (Type type in byType.Keys.Concat(names.Keys))
                if (type.IsGenericType && type.FullName is { } full)
                    typed.TryAdd(full, type);
            typesByName = typed;
            byName = handled;
        }
    }

    /// <summary>Registers a handling unless the type has one.</summary>
    private void Put(StateGraphType type) => byType.TryAdd(type.Type, type);

    /// <summary>Registers a probe for values the framework makes of types it does not name.</summary>
    private void Probe(StateGraphProbe probe) => probes.Add(probe);

    private void RegisterScalars()
    {
        Put(new PrimitiveStateType<bool>(this, "System.Boolean", static (writer, value) => writer.Write(value), static reader => reader.ReadBoolean()));
        Put(new PrimitiveStateType<byte>(this, "System.Byte", static (writer, value) => writer.Write(value), static reader => reader.ReadByte()));
        Put(new PrimitiveStateType<sbyte>(this, "System.SByte", static (writer, value) => writer.Write(value), static reader => reader.ReadSByte()));
        Put(new PrimitiveStateType<short>(this, "System.Int16", static (writer, value) => writer.Write(value), static reader => reader.ReadInt16()));
        Put(new PrimitiveStateType<ushort>(this, "System.UInt16", static (writer, value) => writer.Write(value), static reader => reader.ReadUInt16()));
        Put(new PrimitiveStateType<int>(this, "System.Int32", static (writer, value) => writer.Write(value), static reader => reader.ReadInt32()));
        Put(new PrimitiveStateType<uint>(this, "System.UInt32", static (writer, value) => writer.Write(value), static reader => reader.ReadUInt32()));
        Put(new PrimitiveStateType<long>(this, "System.Int64", static (writer, value) => writer.Write(value), static reader => reader.ReadInt64()));
        Put(new PrimitiveStateType<ulong>(this, "System.UInt64", static (writer, value) => writer.Write(value), static reader => reader.ReadUInt64()));
        Put(new PrimitiveStateType<char>(this, "System.Char", static (writer, value) => writer.Write(value), static reader => reader.ReadChar()));
        Put(new PrimitiveStateType<float>(this, "System.Single", static (writer, value) => writer.Write(value), static reader => reader.ReadSingle()));
        Put(new PrimitiveStateType<double>(this, "System.Double", static (writer, value) => writer.Write(value), static reader => reader.ReadDouble()));
        Put(new PrimitiveStateType<decimal>(this, "System.Decimal", static (writer, value) => writer.Write(value), static reader => reader.ReadDecimal()));
        Put(new PrimitiveStateType<DateTime>(this, "System.DateTime", static (writer, value) => writer.Write(value.ToBinary()), static reader => DateTime.FromBinary(reader.ReadInt64())));
        Put(new PrimitiveStateType<TimeSpan>(this, "System.TimeSpan", static (writer, value) => writer.Write(value.Ticks), static reader => TimeSpan.FromTicks(reader.ReadInt64())));
        Put(new PrimitiveStateType<Guid>(this, "System.Guid", static (writer, value) => writer.Write(value.ToByteArray()), static reader => new Guid(reader.ReadBytes(16))));
    }

    /// <summary>The handling of the scalar <typeparamref name="T"/>.</summary>
    internal PrimitiveStateType<T> Scalar<T>() => (PrimitiveStateType<T>)byType[typeof(T)];

    /// <summary>The handling of the enum <typeparamref name="TEnum"/>.</summary>
    /// <exception cref="NotSupportedException">The enum is not registered.</exception>
    internal EnumStateType<TEnum> EnumOf<TEnum>() where TEnum : struct, Enum =>
        byType.TryGetValue(typeof(TEnum), out StateGraphType? type)
            ? (EnumStateType<TEnum>)type
            : throw new NotSupportedException($"Debugger state cannot hold the enum {typeof(TEnum).FullName}: no registration names it.");

    /// <summary>The handling of the game's value type <typeparamref name="T"/>, or none.</summary>
    internal NodeStateType<T>? NodeOf<T>() => byType.TryGetValue(typeof(T), out StateGraphType? type) ? type as NodeStateType<T> : null;

    // ---------------------------------------------------------------- IStateGraphRegistry

    /// <inheritdoc/>
    public bool Begin(Type type) => begun.Add(Ensure.NotNull(type));

    /// <inheritdoc/>
    public void Name(Type type, StateGraphName name) => names.TryAdd(Ensure.NotNull(type), Ensure.NotNull(name));

    /// <inheritdoc/>
    public void Node<T>(StateGraphName name, Action<IStateGraphWriter, T> write, Func<IStateGraphReader, T>? create,
        Action<T, IStateGraphVisitor> visit, string? packedSchema) =>
        Put(new NodeStateType<T>(this, name, write, create, visit, packedSchema));

    /// <inheritdoc/>
    public void Enum<TEnum>(StateGraphName name, bool flags) where TEnum : struct, Enum => Put(new EnumStateType<TEnum>(this, name, flags));

    /// <inheritdoc/>
    public void Array<T>() => Put(new ArrayStateType<T>(this));

    /// <inheritdoc/>
    public void Grid<T>() => Put(new GridStateType<T>(this));

    /// <inheritdoc/>
    public void ReadOnlyMemory<T>() => Put(new ReadOnlyMemoryStateType<T>(this));

    /// <inheritdoc/>
    public void List<T>() => Put(new ListStateType<T>(this));

    /// <inheritdoc/>
    public void Queue<T>() => Put(new QueueStateType<T>(this));

    /// <inheritdoc/>
    public void Stack<T>() => Put(new StackStateType<T>(this));

    /// <inheritdoc/>
    public void HashSet<T>() => Put(new HashSetStateType<T>(this));

    /// <inheritdoc/>
    public void Dictionary<TKey, TValue>() where TKey : notnull => Put(new DictionaryStateType<TKey, TValue>(this));

    /// <inheritdoc/>
    public void ConcurrentDictionary<TKey, TValue>() where TKey : notnull => Put(new ConcurrentDictionaryStateType<TKey, TValue>(this));

    /// <inheritdoc/>
    public void ImmutableArray<T>() => Put(new ImmutableArrayStateType<T>(this));

    /// <inheritdoc/>
    public void ImmutableDictionary<TKey, TValue>() where TKey : notnull => Put(new ImmutableDictionaryStateType<TKey, TValue>(this));

    /// <inheritdoc/>
    public void ImmutableHashSet<T>() => Put(new ImmutableHashSetStateType<T>(this));

    /// <inheritdoc/>
    public void FrozenDictionary<TKey, TValue>() where TKey : notnull
    {
        if (begun.Add(typeof(System.Collections.Frozen.FrozenDictionary<TKey, TValue>)))
            Probe(new TypedProbe<System.Collections.Frozen.FrozenDictionary<TKey, TValue>>(new FrozenDictionaryStateType<TKey, TValue>(this)));
    }

    /// <inheritdoc/>
    public void FrozenSet<T>()
    {
        if (begun.Add(typeof(System.Collections.Frozen.FrozenSet<T>)))
            Probe(new TypedProbe<System.Collections.Frozen.FrozenSet<T>>(new FrozenSetStateType<T>(this)));
    }

    /// <inheritdoc/>
    public void ReadOnlyCollection<T>() => Put(new ReadOnlyCollectionStateType<T>(this));

    /// <inheritdoc/>
    public void Lazy<T>() => Put(new LazyStateType<T>(this));

    /// <inheritdoc/>
    public void Unheld<T>() where T : class => Put(new UnheldStateType(typeof(T), this));

    /// <inheritdoc/>
    public void Comparer<T>()
    {
        if (begun.Add(typeof(IComparer<T>)))
            Probe(new TypedProbe<IComparer<T>>(new UnheldStateType(typeof(IComparer<T>), this)));
        if (begun.Add(typeof(IEqualityComparer<T>)))
            Probe(new TypedProbe<IEqualityComparer<T>>(new UnheldStateType(typeof(IEqualityComparer<T>), this)));
    }

    /// <inheritdoc/>
    public void KeyValuePair<TKey, TValue>() => Put(new KeyValuePairStateType<TKey, TValue>(this));

    /// <inheritdoc/>
    public void Sequence<T>()
    {
        Array<T>();
        List<T>();
        ImmutableArray<T>();
        ReadOnlyCollection<T>();
        if (begun.Add(typeof(CompilerReadOnlyList<T>)))
            Probe(new CompilerReadOnlyListProbe<T>(new CompilerReadOnlyListStateType<T>(this)));
    }

    /// <inheritdoc/>
    public void Map<TKey, TValue>() where TKey : notnull
    {
        Dictionary<TKey, TValue>();
        FrozenDictionary<TKey, TValue>();
        ImmutableDictionary<TKey, TValue>();
    }

    /// <inheritdoc/>
    public void Set<T>()
    {
        HashSet<T>();
        FrozenSet<T>();
        ImmutableHashSet<T>();
    }

    /// <inheritdoc/>
    public void Method<TDelegate>(string name, Func<object?, TDelegate?> create) where TDelegate : Delegate
    {
        if (!byType.TryGetValue(typeof(TDelegate), out StateGraphType? type))
            byType[typeof(TDelegate)] = type = new DelegateStateType<TDelegate>(this);
        ((DelegateStateType<TDelegate>)type).Add(name, create);
    }

    /// <inheritdoc/>
    public void Catalog(StateGraphName type, IReadOnlyList<StateGraphCatalogMember> members) => Catalogs.Add((type, members));

    /// <inheritdoc/>
    public void DeclaredTypes(IReadOnlyList<string> keys, Func<int, object, bool> probe)
    {
        for (int index = 0; index < keys.Count; index++)
            DeclaredProbes.TryAdd(keys[index], (probe, index));
    }

    /// <inheritdoc/>
    public void Holds(string key, bool anything, IReadOnlyList<string> held) => DeclaredHolds.TryAdd(key, (anything, held));

    /// <summary>The type standing for the read-only lists the compiler makes for collection expressions of <typeparamref name="T"/>.</summary>
    private sealed class CompilerReadOnlyList<T>;
}
