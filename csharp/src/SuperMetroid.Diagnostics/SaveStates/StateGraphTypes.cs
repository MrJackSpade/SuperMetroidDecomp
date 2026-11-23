using System.Collections.Concurrent;
using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Every type a debugger state can hold, as the compiler registered it (#2602): the game's types through their generated
/// members, the collections, arrays and enums their fields are declared with, the methods delegates are made from, and the
/// definition catalogs. A state finds a value's type by its runtime type, or for a collection the framework specializes
/// (a frozen collection, the read-only list the compiler makes for a collection expression) by asking each registration
/// that accepts it; it names nothing it was not told about.
/// </summary>
/// <remarks>
/// A type is registered the first time a state needs it (#2619), not as the registry is built: a game object registers its
/// type when first met (<see cref="IStateGraphNode"/>), and any other lookup that misses first registers what the
/// top-level type declaring the type it asks about declares, through each assembly's generated lookup by name. Building
/// every type's handling first, some seventeen thousand of them, took seconds at startup. A state naming a type no lookup
/// has reached registers what its names' top-level types declare, and then every type.
/// </remarks>
internal sealed partial class StateGraphTypes : IStateGraphRegistry
{
    /// <summary>The process's registry: the game's assemblies, and whatever verification adds.</summary>
    internal static StateGraphTypes Shared => shared.Value;

    private static readonly Lazy<StateGraphTypes> shared = new(static () =>
    {
        var types = new StateGraphTypes();
        types.AddAssembly(SuperMetroid.Core.GeneratedStateGraph.RegisterDeclaredIn, SuperMetroid.Core.GeneratedStateGraph.Register);
        types.AddAssembly(SuperMetroid.AssetExtraction.GeneratedStateGraph.RegisterDeclaredIn, SuperMetroid.AssetExtraction.GeneratedStateGraph.Register);
        types.AddAssembly(SuperMetroid.Diagnostics.GeneratedStateGraph.RegisterDeclaredIn, SuperMetroid.Diagnostics.GeneratedStateGraph.Register);
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

    /// <summary>Each name's handling, indexed as restores ask for names (<see cref="Indexed"/>).</summary>
    private readonly Dictionary<string, StateGraphType> byName = new(StringComparer.Ordinal);

    /// <summary>Each name's type, indexed with <see cref="byName"/>.</summary>
    private readonly Dictionary<string, Type> typesByName = new(StringComparer.Ordinal);

    /// <summary>The registrations whose names are not indexed yet: handlings and probes, and named types with their names.</summary>
    private readonly List<object> unnamed = [];

    /// <summary>Each assembly's generated registrations: what a top-level type declares, by its name, and every type.</summary>
    private readonly List<(Func<string, IStateGraphRegistry, bool> DeclaredIn, Action<IStateGraphRegistry> Everything)> assemblies = [];

    /// <summary>The top-level type names whose declarations were registered, or were asked for and found in no assembly.</summary>
    private readonly HashSet<string> declaredInAsked = new(StringComparer.Ordinal);

    /// <summary>Whether every assembly's every type is registered.</summary>
    private bool everythingRegistered;

    /// <summary>Starts a registry holding the scalars, strings and the framework's own stateless objects.</summary>
    internal StateGraphTypes()
    {
        RegisterScalars();
        Put(new StringStateType(this));
        Put(new PlainObjectStateType(typeof(object), this, static () => new object()));
        Put(new PlainObjectStateType(typeof(Lock), this, static () => new Lock()));
        Array<object>();
        Probe(new SingletonProbe(new SingletonStateType(this, "System.StringComparer.Ordinal", StringComparer.Ordinal)));
        Probe(new SingletonProbe(new SingletonStateType(this, "System.StringComparer.OrdinalIgnoreCase", StringComparer.OrdinalIgnoreCase)));
        Probe(new SingletonProbe(new SingletonStateType(this, "System.Collections.Generic.ReferenceEqualityComparer", ReferenceEqualityComparer.Instance)));
        // A type value is the runtime's own specialization of Type, accepted by what it is rather than registered by type.
        Probe(new TypedProbe<Type>(new TypeIdentityStateType(this)));
    }

    /// <summary>The catalogs registered: each type, its name and its static read-only members holding objects.</summary>
    internal List<(Type Catalog, StateGraphName Type, IReadOnlyList<StateGraphCatalogMember> Members)> Catalogs { get; } = [];

    /// <summary>Runs a registration now, such as one naming a type no assembly's generated registrations declare.</summary>
    /// <param name="registration">The registration.</param>
    internal void Add(Action<IStateGraphRegistry> registration)
    {
        Ensure.NotNull(registration);
        lock (gate)
            registration(this);
    }

    /// <summary>Adds an assembly's generated registrations, none of which runs until a lookup needs one of its types.</summary>
    /// <param name="declaredIn">Registers what a top-level type declares, by the type's name; false for a name it does not declare.</param>
    /// <param name="everything">Registers every type of the assembly.</param>
    internal void AddAssembly(Func<string, IStateGraphRegistry, bool> declaredIn, Action<IStateGraphRegistry> everything)
    {
        Ensure.NotNull(declaredIn);
        Ensure.NotNull(everything);
        lock (gate)
        {
            assemblies.Add((declaredIn, everything));
            if (everythingRegistered)
                everything(this);
        }
    }

    /// <summary>Registers what the top-level type declaring <paramref name="type"/> declares, and its type arguments' and element type's.</summary>
    internal void RegisterDeclaring(Type type)
    {
        Ensure.NotNull(type);
        lock (gate)
            Declaring(type);
    }

    /// <summary>Registers what the top-level types the names within a state's <paramref name="name"/> name declare.</summary>
    internal void RegisterNamed(string name)
    {
        Ensure.NotNull(name);
        lock (gate)
            Named(name.AsSpan());
    }

    /// <summary>Registers every type of every assembly, as a lookup that must see them all needs.</summary>
    internal void RegisterEverything()
    {
        lock (gate)
            Everything();
    }

    /// <summary>The catalogs registered so far.</summary>
    internal IReadOnlyList<(Type Catalog, StateGraphName Type, IReadOnlyList<StateGraphCatalogMember> Members)> CatalogsRegistered()
    {
        lock (gate)
            return [.. Catalogs];
    }

    /// <summary>Registers what the top-level type declaring <paramref name="type"/> declares; callers hold the lock.</summary>
    private void Declaring(Type type)
    {
        while (type.HasElementType)
            type = type.GetElementType()!;
        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            foreach (Type argument in type.GetGenericArguments())
                Declaring(argument);
        }
        Type topLevel = type;
        while (topLevel.DeclaringType is { } outer)
            topLevel = outer;
        // A generic top-level type has no registration of its own: each construction registers itself when first met.
        if (!topLevel.IsGenericType && topLevel.FullName is { } name)
            DeclaredIn(name);
    }

    /// <summary>Registers what the top-level types named within <paramref name="name"/> declare; callers hold the lock.</summary>
    private void Named(ReadOnlySpan<char> name)
    {
        foreach (Range part in name.SplitAny("<>,[]():"))
        {
            ReadOnlySpan<char> named = name[part].Trim();
            int nested = named.IndexOf('+');
            if (nested >= 0)
                named = named[..nested];
            if (!named.IsEmpty)
                DeclaredIn(named.ToString());
        }
    }

    /// <summary>Registers what the top-level type <paramref name="name"/> declares unless asked for before; callers hold the lock.</summary>
    private void DeclaredIn(string name)
    {
        if (everythingRegistered || !declaredInAsked.Add(name))
            return;
        foreach ((Func<string, IStateGraphRegistry, bool> declaredIn, Action<IStateGraphRegistry> _) in assemblies)
        {
            if (declaredIn(name, this))
                return;
        }
    }

    /// <summary>Registers every type of every assembly; callers hold the lock.</summary>
    private void Everything()
    {
        if (everythingRegistered)
            return;
        everythingRegistered = true;
        foreach ((Func<string, IStateGraphRegistry, bool> _, Action<IStateGraphRegistry> everything) in assemblies)
            everything(this);
        // Registering a kind declares nothing further, so the declarations listed now are every one.
        foreach (IDeclaredCollectionKinds kinds in declaredKinds.ToArray())
            kinds.RegisterAll();
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
        // A construction of a generic type of the game made where no declaration names it registers itself. No probe accepts
        // an object of the game's own types, so one is not asked about each probe's framework type first.
        if (value is IStateGraphNode node)
        {
            lock (gate)
                node.Describe(this);
            if (byType.TryGetValue(type, out known))
                return known;
        }
        lock (gate)
        {
            Declaring(type);
            // A delegate's type is registered by the types declaring the methods it is made from.
            if (value is Delegate made)
            {
                foreach (Delegate call in made.GetInvocationList())
                    Declaring(call.Method.DeclaringType ?? type);
            }
            DeclaredKinds(type);
        }
        if (byType.TryGetValue(type, out known))
            return known;
        StateGraphType? found = null;
        // A framework collection is accepted only by a probe of a collection, which is not asked about any other object.
        bool enumerates = value is System.Collections.IEnumerable;
        lock (gate)
        {
            foreach (StateGraphProbe probe in probes)
            {
                if ((enumerates || !probe.AcceptsOnlyEnumerating) && probe.Accepts(value) && (found is null || probe.MoreSpecificThan(found)))
                    found = probe.Type;
            }
        }
        if (found is not null)
            probed.TryAdd(type, found);
        return found;
    }

    /// <summary>The registered handling of values of exactly <paramref name="type"/>, or none.</summary>
    internal StateGraphType? Registered(Type type)
    {
        if (byType.TryGetValue(type, out StateGraphType? known))
            return known;
        lock (gate)
        {
            // A type registered by no declaration has its declarations registered once; a later registration of it is found
            // above, and a declared collection kind registers whenever its declaration has.
            if (declaringAsked.TryAdd(type, true))
                Declaring(type);
            DeclaredKinds(type);
        }
        return byType.TryGetValue(type, out known) ? known : null;
    }

    /// <summary>The types <see cref="Registered"/> has registered the declarations of.</summary>
    private readonly ConcurrentDictionary<Type, bool> declaringAsked = new();

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
    internal StateGraphType Named(string name) =>
        Indexed(byName, name) ?? throw new InvalidDataException(
            $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the state names type '{name}', which this build does not have.");

    /// <summary>The type a state's type identity names (#2060).</summary>
    /// <exception cref="InvalidDataException">This build has no type of the name.</exception>
    internal Type TypeNamed(string name) =>
        Indexed(typesByName, name) ?? throw new InvalidDataException(
            $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the state names type '{name}', which this build does not have.");

    /// <summary>
    /// What <paramref name="index"/> holds under <paramref name="name"/>, indexing names until it is found: first the names
    /// of the registrations a state has already named, such as every type a state this process wrote holds, then every
    /// registration's, which a state written by another process needs. Working out a name takes those of its type
    /// arguments, so the registry's thousands of types are named only when a state names one no capture has named.
    /// </summary>
    private T? Indexed<T>(Dictionary<string, T> index, string name) where T : class
    {
        lock (gate)
        {
            if (index.TryGetValue(name, out T? known))
                return known;
            IndexNames(everything: false);
            if (index.TryGetValue(name, out known))
                return known;
            IndexNames(everything: true);
            if (index.TryGetValue(name, out known))
                return known;
            // A state written by another process names types no lookup here has reached (#2619).
            Named(name.AsSpan());
            IndexNames(everything: true);
            if (index.TryGetValue(name, out known))
                return known;
            Everything();
            IndexNames(everything: true);
            return index.TryGetValue(name, out known) ? known : null;
        }
    }

    /// <summary>
    /// Indexes the registrations not indexed yet: those whose names are worked out already, or every one; callers hold the
    /// lock.
    /// </summary>
    private void IndexNames(bool everything)
    {
        int kept = 0;
        for (int index = 0; index < unnamed.Count; index++)
        {
            object registration = unnamed[index];
            if (registration is StateGraphType type)
            {
                if (!everything && !type.IsNamed)
                {
                    unnamed[kept++] = registration;
                    continue;
                }
                IndexName(type);
            }
            else
            {
                IndexName(((Type Type, StateGraphName Name))registration);
            }
        }
        unnamed.RemoveRange(kept, unnamed.Count - kept);
    }

    /// <summary>Indexes a handling's name; callers hold the lock.</summary>
    /// <exception cref="InvalidOperationException">Another type's handling has the name.</exception>
    private void IndexName(StateGraphType type)
    {
        // A compiler-made list is written as the array it holds, under the array's name.
        if (type is not ICompilerReadOnlyList)
        {
            if (byName.TryGetValue(type.Name, out StateGraphType? other) && !ReferenceEquals(other, type) && other.Type != type.Type)
                throw new InvalidOperationException($"Debugger state types {other.Type.FullName} and {type.Type.FullName} share the name {type.Name}.");
            byName.TryAdd(type.Name, type);
            typesByName.TryAdd(type.Name, type.Type);
        }
        IndexFullName(type.Type);
    }

    /// <summary>Indexes a named type's name; callers hold the lock.</summary>
    private void IndexName((Type Type, StateGraphName Name) named)
    {
        typesByName.TryAdd(Format(named.Name), named.Type);
        IndexFullName(named.Type);
    }

    /// <summary>Indexes a constructed type by its full name, which a type value of it is named by (NameOf); callers hold the lock.</summary>
    private void IndexFullName(Type type)
    {
        if (type.IsGenericType && type.FullName is { } full)
            typesByName.TryAdd(full, type);
    }

    /// <summary>Registers a handling unless the type has one.</summary>
    private void Put(StateGraphType type)
    {
        if (byType.TryAdd(type.Type, type))
            unnamed.Add(type);
    }

    /// <summary>Registers a probe for values the framework makes of types it does not name.</summary>
    private void Probe(StateGraphProbe probe)
    {
        probes.Add(probe);
        unnamed.Add(probe.Type);
    }

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
        Registered(typeof(TEnum)) is { } type
            ? (EnumStateType<TEnum>)type
            : throw new NotSupportedException($"Debugger state cannot hold the enum {typeof(TEnum).FullName}: no registration names it.");

    /// <summary>The handling of the game's value type <typeparamref name="T"/>, or none.</summary>
    internal NodeStateType<T>? NodeOf<T>() => Registered(typeof(T)) as NodeStateType<T>;

    // ---------------------------------------------------------------- IStateGraphRegistry

    /// <inheritdoc/>
    public bool Begin(Type type) => begun.Add(Ensure.NotNull(type));

    /// <inheritdoc/>
    public void Name(Type type, StateGraphName name)
    {
        if (names.TryAdd(Ensure.NotNull(type), Ensure.NotNull(name)))
            unnamed.Add((type, name));
    }

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
            Probe(new TypedProbe<System.Collections.Frozen.FrozenDictionary<TKey, TValue>>(new FrozenDictionaryStateType<TKey, TValue>(this), enumerating: true));
    }

    /// <inheritdoc/>
    public void FrozenSet<T>()
    {
        if (begun.Add(typeof(System.Collections.Frozen.FrozenSet<T>)))
            Probe(new TypedProbe<System.Collections.Frozen.FrozenSet<T>>(new FrozenSetStateType<T>(this), enumerating: true));
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
    /// <remarks>Each kind is registered the first time a state meets a value of it (<see cref="DeclaredKinds"/>, #2619).</remarks>
    public void Sequence<T>()
    {
        if (begun.Add(typeof(SequenceKinds<T>)))
            Declare(sequences, typeof(T), new SequenceKinds<T>(this));
    }

    /// <inheritdoc/>
    /// <remarks>Each kind is registered the first time a state meets a value of it (<see cref="DeclaredKinds"/>, #2619).</remarks>
    public void Map<TKey, TValue>() where TKey : notnull
    {
        if (begun.Add(typeof(MapKinds<TKey, TValue>)))
            Declare(maps, (typeof(TKey), typeof(TValue)), new MapKinds<TKey, TValue>(this));
    }

    /// <inheritdoc/>
    /// <remarks>Each kind is registered the first time a state meets a value of it (<see cref="DeclaredKinds"/>, #2619).</remarks>
    public void Set<T>()
    {
        if (begun.Add(typeof(SetKinds<T>)))
            Declare(sets, typeof(T), new SetKinds<T>(this));
    }

    /// <summary>Registers the read-only lists the compiler makes for collection expressions of <typeparamref name="T"/>.</summary>
    internal void CompilerList<T>()
    {
        Array<T>();
        if (begun.Add(typeof(CompilerReadOnlyList<T>)))
            Probe(new CompilerReadOnlyListProbe<T>(new CompilerReadOnlyListStateType<T>(this)));
    }

    /// <summary>The declared sequences by element type, whose kinds register as a state first meets each.</summary>
    private readonly Dictionary<Type, IDeclaredCollectionKinds> sequences = [];

    /// <summary>The declared maps by key and value type, whose kinds register as a state first meets each.</summary>
    private readonly Dictionary<(Type Key, Type Value), IDeclaredCollectionKinds> maps = [];

    /// <summary>The declared sets by member type, whose kinds register as a state first meets each.</summary>
    private readonly Dictionary<Type, IDeclaredCollectionKinds> sets = [];

    /// <summary>Every declared sequence, map and set, in the order declared.</summary>
    private readonly List<IDeclaredCollectionKinds> declaredKinds = [];

    /// <summary>Records a declaration whose kinds register as a state first meets each; callers hold the lock.</summary>
    private void Declare<TKey>(Dictionary<TKey, IDeclaredCollectionKinds> index, TKey key, IDeclaredCollectionKinds kinds) where TKey : notnull
    {
        index.Add(key, kinds);
        declaredKinds.Add(kinds);
        if (everythingRegistered)
            kinds.RegisterAll();
    }

    /// <summary>
    /// Registers the handling of <paramref name="runtime"/> when it is a kind a declared sequence, map or set may hold and
    /// no state has met one before (#2619): an array, list, immutable array or read-only collection of a sequence's element
    /// type, the compiler's read-only list of a type a sequence's element type is assignable from, or a dictionary, hash
    /// set, immutable or frozen one of a map's or set's types; callers hold the lock.
    /// </summary>
    private void DeclaredKinds(Type runtime)
    {
        if (runtime.IsSZArray)
        {
            if (sequences.TryGetValue(runtime.GetElementType()!, out IDeclaredCollectionKinds? sequence))
                sequence.Register(typeof(Array));
            return;
        }
        if (!runtime.IsGenericType)
            return;
        Type[] arguments = runtime.GetGenericArguments();
        Type kind = runtime.GetGenericTypeDefinition();
        // A frozen collection is the framework's specialization of FrozenDictionary or FrozenSet, its own type internal.
        for (Type? frozen = runtime.BaseType; frozen is not null; frozen = frozen.BaseType)
        {
            if (frozen.IsGenericType && frozen.GetGenericTypeDefinition() is var definition &&
                (definition == typeof(System.Collections.Frozen.FrozenDictionary<,>) || definition == typeof(System.Collections.Frozen.FrozenSet<>)))
            {
                kind = definition;
                arguments = frozen.GetGenericArguments();
                break;
            }
        }
        if (arguments.Length == 2)
        {
            if (maps.TryGetValue((arguments[0], arguments[1]), out IDeclaredCollectionKinds? map))
                map.Register(kind);
            return;
        }
        if (arguments.Length != 1)
            return;
        if (sets.TryGetValue(arguments[0], out IDeclaredCollectionKinds? set) && set.Register(kind))
            return;
        if (sequences.TryGetValue(arguments[0], out IDeclaredCollectionKinds? declared) && declared.Register(kind))
            return;
        // The compiler's list of an element type is a list of every type the element type is assignable to.
        if (runtime.Name.StartsWith("<>z__", StringComparison.Ordinal))
        {
            foreach ((Type element, IDeclaredCollectionKinds kinds) in sequences)
            {
                if (element.IsAssignableFrom(arguments[0]))
                    kinds.Register(typeof(CompilerListKind));
            }
        }
    }

    /// <inheritdoc/>
    public void Method<TDelegate>(string name, Func<object?, TDelegate?> create) where TDelegate : Delegate
    {
        if (!byType.TryGetValue(typeof(TDelegate), out StateGraphType? type))
            Put(type = new DelegateStateType<TDelegate>(this));
        ((DelegateStateType<TDelegate>)type).Add(name, create);
    }

    /// <inheritdoc/>
    public void Catalog(Type catalog, StateGraphName type, IReadOnlyList<StateGraphCatalogMember> members) =>
        Catalogs.Add((Ensure.NotNull(catalog), type, members));

    /// <summary>The type standing for the read-only lists the compiler makes for collection expressions of <typeparamref name="T"/>.</summary>
    private sealed class CompilerReadOnlyList<T>;
}
