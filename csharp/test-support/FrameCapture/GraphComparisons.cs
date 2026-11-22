using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SuperMetroid.Core;

namespace SuperMetroid.TestSupport.FrameRendering;

/// <summary>
/// How two values of each type a published graph holds are compared (#2602), learned from the game's generated state-graph
/// registration: the game's types through their generated field writers, the collections and arrays their fields are
/// declared with, and the methods delegates are made from; enums by their values. Types are learned as the comparisons
/// meet them: each object of the game registers its own type and what its fields are declared with, so a comparison of a
/// few frames registers only what those frames hold. A value nothing met so far registers, such as a framework collection
/// reached through a field declared as an object, or a delegate made from a method of a type not met yet, has its
/// comparison once the whole game is registered; a value of a type nothing registers has none, and two such values are not
/// the same.
/// </summary>
internal sealed class GraphComparisons : IStateGraphRegistry
{
    /// <summary>Compares two values of one runtime type, queueing the pairs of values they hold on the walk.</summary>
    internal delegate bool ValueComparison(object left, object right, GraphWalk walk);

    /// <summary>The process's comparisons: the game's simulation and import assemblies.</summary>
    internal static GraphComparisons Shared => shared.Value;

    private static readonly Lazy<GraphComparisons> shared = new(static () => new GraphComparisons());

    /// <summary>Guards registration.</summary>
    private readonly Lock gate = new();

    /// <summary>Whether the whole game is registered, after a value no object met so far registered.</summary>
    private bool completed;

    /// <summary>Each registered runtime type's comparison.</summary>
    private readonly ConcurrentDictionary<Type, ValueComparison> byType = new();

    /// <summary>The comparisons of values of types the framework makes, asked in turn when a runtime type is not registered.</summary>
    private readonly List<(Func<object, bool> Accepts, ValueComparison Compare)> probes = [];

    /// <summary>Each runtime type a probe accepted, with its comparison.</summary>
    private readonly ConcurrentDictionary<Type, ValueComparison> probed = new();

    /// <summary>The types whose registration has begun.</summary>
    private readonly HashSet<Type> begun = [];

    /// <summary>Starts with the scalars, strings, type identities and the framework's stateless objects.</summary>
    private GraphComparisons()
    {
        Scalar<bool>();
        Scalar<byte>();
        Scalar<sbyte>();
        Scalar<short>();
        Scalar<ushort>();
        Scalar<int>();
        Scalar<uint>();
        Scalar<long>();
        Scalar<ulong>();
        Scalar<char>();
        Scalar<float>();
        Scalar<double>();
        Scalar<decimal>();
        Scalar<DateTime>();
        Scalar<TimeSpan>();
        Scalar<Guid>();
        Put(typeof(string), static (left, right, _) => string.Equals((string)left, (string)right, StringComparison.Ordinal));
        Put(typeof(object), static (_, _, _) => true);
        Put(typeof(Lock), static (_, _, _) => true);
        probes.Add((static value => value is Type, static (left, right, _) => left.Equals(right)));
        Array<object>();
    }

    /// <summary>The comparison of <paramref name="value"/>'s type, or none when nothing registers it.</summary>
    internal ValueComparison? For(object value)
    {
        Type type = value.GetType();
        if (byType.TryGetValue(type, out ValueComparison? known) || probed.TryGetValue(type, out known))
            return known;
        // Two enum values of one type are the same when their values are.
        if (type.IsEnum)
            return byType.GetOrAdd(type, static _ => static (left, right, _) => left.Equals(right));
        lock (gate)
            return Learned(value, type) ?? (CompleteRegistration() ? Learned(value, type) : null);
    }

    /// <summary>The comparison of <paramref name="value"/>'s type as far as the registration reaches; callers hold the lock.</summary>
    private ValueComparison? Learned(object value, Type type)
    {
        if (byType.TryGetValue(type, out ValueComparison? known) || probed.TryGetValue(type, out known))
            return known;
        // An object of the game registers its own type, and is compared by all its fields rather than as whatever framework
        // collection it also is.
        if (value is IStateGraphNode node)
        {
            node.Describe(this);
            if (byType.TryGetValue(type, out known))
                return known;
        }
        foreach ((Func<object, bool> accepts, ValueComparison compare) in probes)
        {
            if (accepts(value))
            {
                probed.TryAdd(type, compare);
                return compare;
            }
        }
        return null;
    }

    /// <summary>Registers the whole game unless it is registered already; true when it registered it now.</summary>
    private bool CompleteRegistration()
    {
        lock (gate)
        {
            if (completed)
                return false;
            completed = true;
            SuperMetroid.Core.GeneratedStateGraph.Register(this);
            SuperMetroid.AssetExtraction.GeneratedStateGraph.Register(this);
            return true;
        }
    }

    private void Put(Type type, ValueComparison compare) => byType.TryAdd(type, compare);

    private void Probe<TBase>(ValueComparison compare) => probes.Add((static value => value is TBase, compare));

    private void Scalar<T>() where T : unmanaged => Put(typeof(T), static (left, right, _) => SameBits((T)left, (T)right));

    /// <summary>Whether two unmanaged values hold the same bits.</summary>
    private static bool SameBits<T>(T left, T right) where T : unmanaged =>
        MemoryMarshal.AsBytes(new ReadOnlySpan<T>(in left)).SequenceEqual(MemoryMarshal.AsBytes(new ReadOnlySpan<T>(in right)));

    /// <summary>
    /// Whether two runs hold the same elements: bit for bit when the element type holds no reference, otherwise each pair
    /// of elements queued.
    /// </summary>
    private static bool SameRuns<T>(ReadOnlySpan<T> left, ReadOnlySpan<T> right, GraphWalk walk)
    {
        if (left.Length != right.Length)
            return false;
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return Bytes(left).SequenceEqual(Bytes(right));
        // A run of the game's values holding references is compared element by element as it is met, through the values'
        // own field writer, so no element is boxed and only the references they hold are queued. Two values with the same
        // bits hold the same scalars and the same objects, so they are the same without being written; a run whose bits
        // all match is the same as a whole.
        if (typeof(T).IsValueType && ValueFields<T>.Write is { } write)
        {
            if (Bytes(left).SequenceEqual(Bytes(right)))
                return true;
            for (int index = 0; index < left.Length; index++)
            {
                if (!Bytes(left.Slice(index, 1)).SequenceEqual(Bytes(right.Slice(index, 1))) &&
                    !walk.SameFields(write, left[index], right[index]))
                    return false;
            }
            return true;
        }
        for (int index = 0; index < left.Length; index++)
            walk.Push(left[index], right[index]);
        return true;
    }

    /// <summary>The generated field writer of each of the game's value types, once the type is registered.</summary>
    internal static class ValueFields<T>
    {
        /// <summary>Writes a value's fields; none until the type is registered.</summary>
        internal static Action<IStateGraphWriter, T>? Write { get; set; }
    }

    /// <summary>The bytes of a run of values that hold no reference.</summary>
    private static ReadOnlySpan<byte> Bytes<T>(ReadOnlySpan<T> run) =>
        MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(run)), checked(run.Length * Unsafe.SizeOf<T>()));

    /// <summary>Whether two sequences hold the same elements in the same order, each pair queued.</summary>
    private static bool SameSequences<T>(IEnumerable<T> left, IEnumerable<T> right, GraphWalk walk)
    {
        using IEnumerator<T> mine = left.GetEnumerator(), theirs = right.GetEnumerator();
        while (true)
        {
            bool more = mine.MoveNext();
            if (more != theirs.MoveNext())
                return false;
            if (!more)
                return true;
            walk.Push(mine.Current, theirs.Current);
        }
    }

    /// <summary>
    /// Whether two dictionaries hold the same entries in the same order: each entry's key and value are queued in pairs, so
    /// an entry is compared by what it holds rather than as a pair no registration names.
    /// </summary>
    private static bool SameEntries<TKey, TValue>(IReadOnlyCollection<KeyValuePair<TKey, TValue>> left,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>> right, GraphWalk walk)
    {
        if (left.Count != right.Count)
            return false;
        using IEnumerator<KeyValuePair<TKey, TValue>> mine = left.GetEnumerator(), theirs = right.GetEnumerator();
        while (mine.MoveNext())
        {
            if (!theirs.MoveNext())
                return false;
            walk.Push(mine.Current.Key, theirs.Current.Key);
            walk.Push(mine.Current.Value, theirs.Current.Value);
        }
        return !theirs.MoveNext();
    }

    /// <summary>Whether two sequences of counted collections hold the same elements in the same order.</summary>
    private static bool SameCollections<T>(IReadOnlyCollection<T> left, IReadOnlyCollection<T> right, GraphWalk walk) =>
        left.Count == right.Count && SameSequences(left, right, walk);

    // ---------------------------------------------------------------- IStateGraphRegistry

    /// <inheritdoc/>
    public bool Begin(Type type) => begun.Add(Ensure.NotNull(type));

    /// <inheritdoc/>
    public void Name(Type type, StateGraphName name)
    {
        // A type no value of which a graph holds needs no comparison.
    }

    /// <inheritdoc/>
    public void Node<T>(StateGraphName name, Action<IStateGraphWriter, T> write, Func<IStateGraphReader, T>? create,
        Action<T, IStateGraphVisitor> visit, string? packedSchema)
    {
        if (typeof(T).IsValueType)
            ValueFields<T>.Write = write;
        Put(typeof(T), (left, right, walk) => walk.SameFields(write, (T)left, (T)right));
    }

    /// <inheritdoc/>
    public void Enum<TEnum>(StateGraphName name, bool flags) where TEnum : struct, Enum
    {
        // Every enum is compared by its value wherever a graph holds one (For), so none needs a comparison of its own.
    }

    /// <inheritdoc/>
    public void Array<T>() => Put(typeof(T[]), static (left, right, walk) => SameRuns<T>((T[])left, (T[])right, walk));

    /// <inheritdoc/>
    public void Grid<T>() => Put(typeof(T[,]), static (left, right, walk) =>
    {
        var mine = (T[,])left;
        var theirs = (T[,])right;
        if (mine.GetLength(0) != theirs.GetLength(0) || mine.GetLength(1) != theirs.GetLength(1))
            return false;
        return SameRuns(
            MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(mine)), mine.Length),
            MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(theirs)), theirs.Length), walk);
    });

    /// <inheritdoc/>
    public void ReadOnlyMemory<T>() => Put(typeof(ReadOnlyMemory<T>), static (left, right, walk) =>
    {
        // Two views of one run of one array hold the same values; others are compared element by element.
        ReadOnlyMemory<T> mine = (ReadOnlyMemory<T>)left, theirs = (ReadOnlyMemory<T>)right;
        return mine.Equals(theirs) || SameRuns(mine.Span, theirs.Span, walk);
    });

    /// <inheritdoc/>
    public void List<T>() => Put(typeof(List<T>), static (left, right, walk) =>
        SameRuns<T>(CollectionsMarshal.AsSpan((List<T>)left), CollectionsMarshal.AsSpan((List<T>)right), walk));

    /// <inheritdoc/>
    public void Queue<T>() => Put(typeof(Queue<T>), static (left, right, walk) => SameCollections((Queue<T>)left, (Queue<T>)right, walk));

    /// <inheritdoc/>
    public void Stack<T>() => Put(typeof(Stack<T>), static (left, right, walk) => SameCollections((Stack<T>)left, (Stack<T>)right, walk));

    /// <inheritdoc/>
    public void HashSet<T>() => Put(typeof(HashSet<T>), static (left, right, walk) => SameCollections((HashSet<T>)left, (HashSet<T>)right, walk));

    /// <inheritdoc/>
    public void Dictionary<TKey, TValue>() where TKey : notnull => Put(typeof(Dictionary<TKey, TValue>), static (left, right, walk) =>
        SameEntries((Dictionary<TKey, TValue>)left, (Dictionary<TKey, TValue>)right, walk));

    /// <inheritdoc/>
    public void ConcurrentDictionary<TKey, TValue>() where TKey : notnull => Put(typeof(ConcurrentDictionary<TKey, TValue>), static (left, right, walk) =>
        SameEntries((ConcurrentDictionary<TKey, TValue>)left, (ConcurrentDictionary<TKey, TValue>)right, walk));

    /// <inheritdoc/>
    public void ImmutableArray<T>() => Put(typeof(ImmutableArray<T>), static (left, right, walk) =>
    {
        ImmutableArray<T> mine = (ImmutableArray<T>)left, theirs = (ImmutableArray<T>)right;
        // Two views of one array hold the same values; others are compared element by element.
        return mine.IsDefault || theirs.IsDefault ? mine.IsDefault == theirs.IsDefault : mine == theirs || SameRuns(mine.AsSpan(), theirs.AsSpan(), walk);
    });

    /// <inheritdoc/>
    public void ImmutableDictionary<TKey, TValue>() where TKey : notnull => Put(typeof(ImmutableDictionary<TKey, TValue>), static (left, right, walk) =>
        SameEntries((ImmutableDictionary<TKey, TValue>)left, (ImmutableDictionary<TKey, TValue>)right, walk));

    /// <inheritdoc/>
    public void ImmutableHashSet<T>() => Put(typeof(ImmutableHashSet<T>), static (left, right, walk) =>
        SameCollections((ImmutableHashSet<T>)left, (ImmutableHashSet<T>)right, walk));

    /// <inheritdoc/>
    public void FrozenDictionary<TKey, TValue>() where TKey : notnull
    {
        if (begun.Add(typeof(FrozenDictionary<TKey, TValue>)))
            Probe<FrozenDictionary<TKey, TValue>>(static (left, right, walk) =>
                SameEntries((FrozenDictionary<TKey, TValue>)left, (FrozenDictionary<TKey, TValue>)right, walk));
    }

    /// <inheritdoc/>
    public void FrozenSet<T>()
    {
        if (begun.Add(typeof(FrozenSet<T>)))
            Probe<FrozenSet<T>>(static (left, right, walk) => SameCollections((FrozenSet<T>)left, (FrozenSet<T>)right, walk));
    }

    /// <inheritdoc/>
    public void ReadOnlyCollection<T>() => Put(typeof(ReadOnlyCollection<T>), static (left, right, walk) =>
        SameCollections((ReadOnlyCollection<T>)left, (ReadOnlyCollection<T>)right, walk));

    /// <inheritdoc/>
    public void Lazy<T>() => Put(typeof(Lazy<T>), static (left, right, walk) =>
    {
        // Two parts not yet loaded are compared by nothing they hold, so they are not known to be the same.
        Lazy<T> mine = (Lazy<T>)left, theirs = (Lazy<T>)right;
        if (!mine.IsValueCreated || !theirs.IsValueCreated)
            return false;
        walk.Push(mine.Value, theirs.Value);
        return true;
    });

    /// <inheritdoc/>
    public void Unheld<T>() where T : class => Put(typeof(T), static (_, _, _) => false);

    /// <inheritdoc/>
    public void Comparer<T>()
    {
        if (begun.Add(typeof(IComparer<T>)))
            Probe<IComparer<T>>(static (left, right, _) => left.Equals(right));
        if (begun.Add(typeof(IEqualityComparer<T>)))
            Probe<IEqualityComparer<T>>(static (left, right, _) => left.Equals(right));
    }

    /// <inheritdoc/>
    public void KeyValuePair<TKey, TValue>() => Put(typeof(KeyValuePair<TKey, TValue>), static (left, right, walk) =>
    {
        KeyValuePair<TKey, TValue> mine = (KeyValuePair<TKey, TValue>)left, theirs = (KeyValuePair<TKey, TValue>)right;
        walk.Push(mine.Key, theirs.Key);
        walk.Push(mine.Value, theirs.Value);
        return true;
    });

    /// <inheritdoc/>
    public void Sequence<T>()
    {
        Array<T>();
        List<T>();
        ImmutableArray<T>();
        ReadOnlyCollection<T>();
        // The read-only lists the compiler makes for collection expressions are named only inside the assembly that made
        // them, so they are recognized by the compiler's prefix and compared element by element.
        if (begun.Add(typeof(IReadOnlyList<T>)))
            probes.Add((static value => value.GetType().Name.StartsWith("<>z__", StringComparison.Ordinal) && value is IReadOnlyList<T>,
                static (left, right, walk) => SameCollections((IReadOnlyList<T>)left, (IReadOnlyList<T>)right, walk)));
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
        if (!byType.ContainsKey(typeof(TDelegate)))
        {
            var methods = new DelegateMethods<TDelegate>(this);
            byType[typeof(TDelegate)] = methods.Compare;
            delegates[typeof(TDelegate)] = methods;
        }
        ((DelegateMethods<TDelegate>)delegates[typeof(TDelegate)]).Add(name, create);
    }

    /// <summary>Each delegate type's registered methods.</summary>
    private readonly Dictionary<Type, object> delegates = [];

    /// <inheritdoc/>
    public void Catalog(StateGraphName type, IReadOnlyList<StateGraphCatalogMember> members)
    {
        // A published graph is compared by its values; where a catalog member lives does not change them.
    }

    /// <inheritdoc/>
    public void DeclaredTypes(IReadOnlyList<string> keys, Func<int, object, bool> probe, IReadOnlyList<Type?> exact, IReadOnlyList<bool> enumerates)
    {
        // The declared types index a restore's candidates; a comparison holds both values already.
    }

    /// <inheritdoc/>
    public void Holds(string key, bool anything, IReadOnlyList<string> held)
    {
        // The declared types index a restore's candidates; a comparison holds both values already.
    }

    /// <summary>
    /// The methods delegates of <typeparamref name="TDelegate"/> are made from, each by its name: two delegates are the same
    /// when they call the same methods in the same order on targets that are the same.
    /// </summary>
    private sealed class DelegateMethods<TDelegate>(GraphComparisons owner) where TDelegate : Delegate
    {
        /// <summary>Each method's name and how it is made for a target.</summary>
        private readonly List<(string Name, Func<object?, TDelegate?> Create)> methods = [];

        /// <summary>Registers a method.</summary>
        internal void Add(string name, Func<object?, TDelegate?> create) => methods.Add((name, create));

        /// <summary>Compares two delegates call by call, queueing their targets in pairs.</summary>
        internal bool Compare(object left, object right, GraphWalk walk)
        {
            Delegate[] mine = ((Delegate)left).GetInvocationList(), theirs = ((Delegate)right).GetInvocationList();
            if (mine.Length != theirs.Length)
                return false;
            for (int index = 0; index < mine.Length; index++)
            {
                if (MethodOf(mine[index]) is not { } method || !string.Equals(method, MethodOf(theirs[index]), StringComparison.Ordinal))
                    return false;
                walk.Push(mine[index].Target, theirs[index].Target);
            }
            return true;
        }

        /// <summary>
        /// The name of the registered method <paramref name="call"/> calls, or none when no registered method does once the
        /// whole game is registered: a method is registered with the type declaring it, which may not have been met yet.
        /// </summary>
        private string? MethodOf(Delegate call)
        {
            lock (owner.gate)
                return Registered(call) ?? (owner.CompleteRegistration() ? Registered(call) : null);
        }

        /// <summary>The name of the method <paramref name="call"/> calls among those registered so far; callers hold the lock.</summary>
        private string? Registered(Delegate call)
        {
            foreach ((string name, Func<object?, TDelegate?> create) in methods)
            {
                if (create(call.Target) is { } made && made.Equals(call))
                    return name;
            }
            return null;
        }
    }
}
