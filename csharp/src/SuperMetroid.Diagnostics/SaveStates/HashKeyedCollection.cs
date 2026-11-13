using SuperMetroid.Core;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// How a debugger state holds a collection that keys its members by hash (#1681): as its comparers and its entries, in
/// the collection's own enumeration order, never as its buckets. A key's hash can depend on the process that computed it
/// (a record or <see cref="HashCode.Combine{T1}(T1)"/> over strings under randomized hashing, an object's reference
/// identity), so a bucket layout restored in another process, or holding restored objects in place of the captured ones,
/// misses its own keys. A restore therefore adds each saved entry to a new collection with the saved comparers, so every
/// key lands where this process hashes it.
/// </summary>
/// <remarks>
/// Each kind the serializer rebuilds has its own rebuild here. A kind that keys by hash and has none is refused when it is
/// captured, rather than written by its layout: a subclass of a rebuilt kind (whose own fields the rebuild would drop), and
/// the framework's other hash-keyed collections. A comparer that is its key type's default comparer is saved as none, so
/// a restore uses this process's default.
/// </remarks>
internal abstract class HashKeyedCollection
{
    /// <summary>Each type's rebuild, or null for a type that keys nothing by hash, worked out once.</summary>
    private static readonly ConcurrentDictionary<Type, HashKeyedCollection?> kinds = new();

    /// <summary>
    /// The framework collections that key their members by hash but have no rebuild here, each refused when captured.
    /// </summary>
    private static readonly Type[] UnrebuiltKinds =
    [
        typeof(System.Collections.Hashtable),
        typeof(System.Collections.Specialized.OrderedDictionary),
        typeof(System.Collections.Specialized.HybridDictionary),
        typeof(System.Collections.Specialized.NameObjectCollectionBase),
        typeof(System.Collections.Specialized.StringDictionary),
        typeof(OrderedDictionary<,>),
        typeof(System.Runtime.CompilerServices.ConditionalWeakTable<,>),
        typeof(System.Linq.Lookup<,>),
        typeof(ImmutableDictionary<,>.Builder),
        typeof(ImmutableHashSet<>.Builder),
    ];

    /// <summary>The generic collections with a rebuild here, which a subclass of cannot be rebuilt by it.</summary>
    private static readonly Type[] RebuiltKinds =
    [
        typeof(Dictionary<,>),
        typeof(HashSet<>),
        typeof(ConcurrentDictionary<,>),
        typeof(ImmutableDictionary<,>),
        typeof(ImmutableHashSet<>),
    ];

    /// <summary>
    /// The rebuild of collections of exactly <paramref name="type"/>, or null when the type keys nothing by hash. A type
    /// that keys by hash without a rebuild here gets one that refuses it (<see cref="Unrebuilt"/>).
    /// </summary>
    internal static HashKeyedCollection? For(Type type) => kinds.GetOrAdd(Ensure.NotNull(type), Classify);

    /// <summary>How many comparers the collection is saved with, each written before its entries.</summary>
    internal abstract int ComparerCount { get; }

    /// <summary>How many values each entry is saved as: its key, then its value for a dictionary.</summary>
    internal abstract int EntryWidth { get; }

    /// <summary>
    /// The comparers <paramref name="collection"/> keys by, in saved order, each null where it is its type's default.
    /// </summary>
    /// <exception cref="NotSupportedException">The collection's kind has no rebuild here.</exception>
    internal abstract object?[] Comparers(object collection);

    /// <summary>Every entry of <paramref name="collection"/>, in its enumeration order, each as <see cref="EntryWidth"/> values.</summary>
    /// <exception cref="NotSupportedException">The collection's kind has no rebuild here.</exception>
    internal abstract object?[] Entries(object collection);

    /// <summary>Works out the rebuild of <paramref name="type"/>.</summary>
    [ReflectionAudit("debugger states rebuild each hash-keyed collection through its own key and value types")]
    private static HashKeyedCollection? Classify(Type type)
    {
        if (type.IsGenericType)
        {
            Type definition = type.GetGenericTypeDefinition();
            Type[] arguments = type.GetGenericArguments();
            if (definition == typeof(Dictionary<,>))
                return Bind(typeof(DictionaryRebuild<,>), arguments);
            if (definition == typeof(HashSet<>))
                return Bind(typeof(HashSetRebuild<>), arguments);
            if (definition == typeof(ConcurrentDictionary<,>))
                return Bind(typeof(ConcurrentDictionaryRebuild<,>), arguments);
            if (definition == typeof(ImmutableDictionary<,>))
                return Bind(typeof(ImmutableDictionaryRebuild<,>), arguments);
            if (definition == typeof(ImmutableHashSet<>))
                return Bind(typeof(ImmutableHashSetRebuild<>), arguments);
        }
        // A frozen collection's runtime type is one of the framework's own specializations of its public base.
        if (GenericBase(type, typeof(FrozenDictionary<,>)) is { } frozenDictionary)
            return Bind(typeof(FrozenDictionaryRebuild<,>), frozenDictionary.GetGenericArguments());
        if (GenericBase(type, typeof(FrozenSet<>)) is { } frozenSet)
            return Bind(typeof(FrozenSetRebuild<>), frozenSet.GetGenericArguments());
        if (RebuiltKinds.Any(kind => GenericBase(type, kind) is not null) ||
            UnrebuiltKinds.Any(kind => kind.IsGenericTypeDefinition ? GenericBase(type, kind) is not null : kind.IsAssignableFrom(type)))
            return new Unrebuilt(type);
        return null;
    }

    /// <summary>The closed <paramref name="definition"/> that <paramref name="type"/> is or derives from, or null.</summary>
    private static Type? GenericBase(Type type, Type definition)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == definition)
                return current;
        }
        return null;
    }

    /// <summary>The rebuild <paramref name="rebuild"/> closed over <paramref name="arguments"/>.</summary>
    [ReflectionAudit("debugger states rebuild each hash-keyed collection through its own key and value types")]
    private static HashKeyedCollection Bind(Type rebuild, Type[] arguments) =>
        (HashKeyedCollection)(Activator.CreateInstance(rebuild.MakeGenericType(arguments), nonPublic: true)
            ?? throw new InvalidOperationException($"The debugger state rebuild {rebuild.Name} could not be made."));

    /// <summary><paramref name="comparer"/> as saved: none when it is <typeparamref name="T"/>'s default comparer.</summary>
    private static object? Saved<T>(IEqualityComparer<T> comparer) =>
        ReferenceEquals(comparer, EqualityComparer<T>.Default) ? null : comparer;

    /// <summary>The comparer a saved collection keyed <typeparamref name="T"/> by; null for its default.</summary>
    /// <exception cref="InvalidDataException">The saved comparer does not compare <typeparamref name="T"/>.</exception>
    private static IEqualityComparer<T>? Restored<T>(object? comparer) => comparer switch
    {
        null => null,
        IEqualityComparer<T> typed => typed,
        _ => throw new InvalidDataException(
            $"A saved hash-keyed collection of {typeof(T).FullName} is keyed by {comparer.GetType().FullName}, which does not compare it."),
    };

    /// <summary>A saved entry's value as <typeparamref name="T"/>.</summary>
    /// <exception cref="InvalidDataException">The saved value is not a <typeparamref name="T"/>.</exception>
    private static T Entry<T>(object? value)
    {
        if (value is T typed)
            return typed;
        if (value is null && default(T) is null)
            return default!;
        throw new InvalidDataException(
            $"A saved hash-keyed collection holds {value?.GetType().FullName ?? "null"} where it stores {typeof(T).FullName}.");
    }

    /// <summary>The failure for a saved collection that holds one key twice under this process's hashing.</summary>
    private static InvalidDataException Duplicate(Type collection, object? key) =>
        new($"A saved {collection.FullName} holds the key {key} twice once rebuilt in this process.");

    /// <summary>
    /// A collection a restore makes empty with its saved comparers and fills once the whole state is read, so every key's
    /// own fields are restored before it is hashed, and the objects inside the state that hold the collection already
    /// hold it while it is read.
    /// </summary>
    internal abstract class Mutable : HashKeyedCollection
    {
        /// <summary>A new, empty collection keyed by <paramref name="comparers"/>.</summary>
        internal abstract object Empty(object?[] comparers);

        /// <summary>Adds every saved entry of <paramref name="entries"/> to <paramref name="collection"/>, in order.</summary>
        /// <exception cref="InvalidDataException">An entry does not fit the collection, or a key repeats.</exception>
        internal abstract void Fill(object collection, object?[] entries);
    }

    /// <summary>
    /// A collection that cannot change once made, built from its saved entries as soon as they are read; its keys must
    /// be whole by then (<see cref="DebuggerObjectGraphSerializer"/> refuses a key still being restored).
    /// </summary>
    internal abstract class Immutable : HashKeyedCollection
    {
        /// <summary>The collection keyed by <paramref name="comparers"/> holding <paramref name="entries"/>.</summary>
        /// <exception cref="InvalidDataException">An entry does not fit the collection, or a key repeats.</exception>
        internal abstract object Build(object?[] comparers, object?[] entries);
    }

    /// <summary>A hash-keyed collection type with no rebuild here, refused whenever a capture reaches one.</summary>
    /// <param name="type">The refused type.</param>
    private sealed class Unrebuilt(Type type) : HashKeyedCollection
    {
        /// <inheritdoc/>
        internal override int ComparerCount => throw Refused();

        /// <inheritdoc/>
        internal override int EntryWidth => throw Refused();

        /// <inheritdoc/>
        internal override object?[] Comparers(object collection) => throw Refused();

        /// <inheritdoc/>
        internal override object?[] Entries(object collection) => throw Refused();

        /// <summary>The refusal naming the type.</summary>
        private NotSupportedException Refused() => new(
            $"Debugger state cannot hold the hash-keyed {type.FullName}: it has no rebuild from its entries, and its bucket layout would not survive a restore in another process.");
    }

    /// <summary>Rebuilds a <see cref="Dictionary{TKey, TValue}"/>.</summary>
    private sealed class DictionaryRebuild<TKey, TValue> : Mutable where TKey : notnull
    {
        /// <inheritdoc/>
        internal override int ComparerCount => 1;

        /// <inheritdoc/>
        internal override int EntryWidth => 2;

        /// <inheritdoc/>
        internal override object?[] Comparers(object collection) => [Saved(((Dictionary<TKey, TValue>)collection).Comparer)];

        /// <inheritdoc/>
        internal override object?[] Entries(object collection)
        {
            var dictionary = (Dictionary<TKey, TValue>)collection;
            var entries = new object?[dictionary.Count * 2];
            int index = 0;
            foreach ((TKey key, TValue value) in dictionary)
            {
                entries[index++] = key;
                entries[index++] = value;
            }
            return entries;
        }

        /// <inheritdoc/>
        internal override object Empty(object?[] comparers) => new Dictionary<TKey, TValue>(Restored<TKey>(comparers[0]));

        /// <inheritdoc/>
        internal override void Fill(object collection, object?[] entries)
        {
            var dictionary = (Dictionary<TKey, TValue>)collection;
            dictionary.EnsureCapacity(entries.Length / 2);
            for (int index = 0; index < entries.Length; index += 2)
            {
                if (!dictionary.TryAdd(Entry<TKey>(entries[index]), Entry<TValue>(entries[index + 1])))
                    throw Duplicate(collection.GetType(), entries[index]);
            }
        }
    }

    /// <summary>Rebuilds a <see cref="HashSet{T}"/>.</summary>
    private sealed class HashSetRebuild<T> : Mutable
    {
        /// <inheritdoc/>
        internal override int ComparerCount => 1;

        /// <inheritdoc/>
        internal override int EntryWidth => 1;

        /// <inheritdoc/>
        internal override object?[] Comparers(object collection) => [Saved(((HashSet<T>)collection).Comparer)];

        /// <inheritdoc/>
        internal override object?[] Entries(object collection) => [.. ((HashSet<T>)collection).Select(static member => (object?)member)];

        /// <inheritdoc/>
        internal override object Empty(object?[] comparers) => new HashSet<T>(Restored<T>(comparers[0]));

        /// <inheritdoc/>
        internal override void Fill(object collection, object?[] entries)
        {
            var set = (HashSet<T>)collection;
            set.EnsureCapacity(entries.Length);
            foreach (object? member in entries)
            {
                if (!set.Add(Entry<T>(member)))
                    throw Duplicate(collection.GetType(), member);
            }
        }
    }

    /// <summary>Rebuilds a <see cref="ConcurrentDictionary{TKey, TValue}"/>.</summary>
    private sealed class ConcurrentDictionaryRebuild<TKey, TValue> : Mutable where TKey : notnull
    {
        /// <inheritdoc/>
        internal override int ComparerCount => 1;

        /// <inheritdoc/>
        internal override int EntryWidth => 2;

        /// <inheritdoc/>
        internal override object?[] Comparers(object collection) => [Saved(((ConcurrentDictionary<TKey, TValue>)collection).Comparer)];

        /// <inheritdoc/>
        internal override object?[] Entries(object collection)
        {
            KeyValuePair<TKey, TValue>[] pairs = ((ConcurrentDictionary<TKey, TValue>)collection).ToArray();
            var entries = new object?[pairs.Length * 2];
            for (int index = 0; index < pairs.Length; index++)
            {
                entries[index * 2] = pairs[index].Key;
                entries[(index * 2) + 1] = pairs[index].Value;
            }
            return entries;
        }

        /// <inheritdoc/>
        internal override object Empty(object?[] comparers) => new ConcurrentDictionary<TKey, TValue>(Restored<TKey>(comparers[0]));

        /// <inheritdoc/>
        internal override void Fill(object collection, object?[] entries)
        {
            var dictionary = (ConcurrentDictionary<TKey, TValue>)collection;
            for (int index = 0; index < entries.Length; index += 2)
            {
                if (!dictionary.TryAdd(Entry<TKey>(entries[index]), Entry<TValue>(entries[index + 1])))
                    throw Duplicate(collection.GetType(), entries[index]);
            }
        }
    }

    /// <summary>Rebuilds an <see cref="ImmutableDictionary{TKey, TValue}"/>, with its key and value comparers.</summary>
    private sealed class ImmutableDictionaryRebuild<TKey, TValue> : Immutable where TKey : notnull
    {
        /// <inheritdoc/>
        internal override int ComparerCount => 2;

        /// <inheritdoc/>
        internal override int EntryWidth => 2;

        /// <inheritdoc/>
        internal override object?[] Comparers(object collection)
        {
            var dictionary = (ImmutableDictionary<TKey, TValue>)collection;
            return [Saved(dictionary.KeyComparer), Saved(dictionary.ValueComparer)];
        }

        /// <inheritdoc/>
        internal override object?[] Entries(object collection)
        {
            var dictionary = (ImmutableDictionary<TKey, TValue>)collection;
            var entries = new object?[dictionary.Count * 2];
            int index = 0;
            foreach ((TKey key, TValue value) in dictionary)
            {
                entries[index++] = key;
                entries[index++] = value;
            }
            return entries;
        }

        /// <inheritdoc/>
        internal override object Build(object?[] comparers, object?[] entries)
        {
            ImmutableDictionary<TKey, TValue>.Builder builder = ImmutableDictionary<TKey, TValue>.Empty
                .WithComparers(Restored<TKey>(comparers[0]), Restored<TValue>(comparers[1])).ToBuilder();
            for (int index = 0; index < entries.Length; index += 2)
            {
                TKey key = Entry<TKey>(entries[index]);
                if (builder.ContainsKey(key))
                    throw Duplicate(typeof(ImmutableDictionary<TKey, TValue>), key);
                builder.Add(key, Entry<TValue>(entries[index + 1]));
            }
            return builder.ToImmutable();
        }
    }

    /// <summary>Rebuilds an <see cref="ImmutableHashSet{T}"/>.</summary>
    private sealed class ImmutableHashSetRebuild<T> : Immutable
    {
        /// <inheritdoc/>
        internal override int ComparerCount => 1;

        /// <inheritdoc/>
        internal override int EntryWidth => 1;

        /// <inheritdoc/>
        internal override object?[] Comparers(object collection) => [Saved(((ImmutableHashSet<T>)collection).KeyComparer)];

        /// <inheritdoc/>
        internal override object?[] Entries(object collection) => [.. ((ImmutableHashSet<T>)collection).Select(static member => (object?)member)];

        /// <inheritdoc/>
        internal override object Build(object?[] comparers, object?[] entries)
        {
            ImmutableHashSet<T>.Builder builder = ImmutableHashSet<T>.Empty.WithComparer(Restored<T>(comparers[0])).ToBuilder();
            foreach (object? member in entries)
            {
                if (!builder.Add(Entry<T>(member)))
                    throw Duplicate(typeof(ImmutableHashSet<T>), member);
            }
            return builder.ToImmutable();
        }
    }

    /// <summary>Rebuilds a <see cref="FrozenDictionary{TKey, TValue}"/>, whatever specialization the framework chose for it.</summary>
    private sealed class FrozenDictionaryRebuild<TKey, TValue> : Immutable where TKey : notnull
    {
        /// <inheritdoc/>
        internal override int ComparerCount => 1;

        /// <inheritdoc/>
        internal override int EntryWidth => 2;

        /// <inheritdoc/>
        internal override object?[] Comparers(object collection) => [Saved(((FrozenDictionary<TKey, TValue>)collection).Comparer)];

        /// <inheritdoc/>
        internal override object?[] Entries(object collection)
        {
            var dictionary = (FrozenDictionary<TKey, TValue>)collection;
            var entries = new object?[dictionary.Count * 2];
            int index = 0;
            foreach ((TKey key, TValue value) in dictionary)
            {
                entries[index++] = key;
                entries[index++] = value;
            }
            return entries;
        }

        /// <inheritdoc/>
        internal override object Build(object?[] comparers, object?[] entries)
        {
            IEqualityComparer<TKey>? comparer = Restored<TKey>(comparers[0]);
            var dictionary = new Dictionary<TKey, TValue>(entries.Length / 2, comparer);
            for (int index = 0; index < entries.Length; index += 2)
            {
                if (!dictionary.TryAdd(Entry<TKey>(entries[index]), Entry<TValue>(entries[index + 1])))
                    throw Duplicate(typeof(FrozenDictionary<TKey, TValue>), entries[index]);
            }
            return dictionary.ToFrozenDictionary(comparer);
        }
    }

    /// <summary>Rebuilds a <see cref="FrozenSet{T}"/>, whatever specialization the framework chose for it.</summary>
    private sealed class FrozenSetRebuild<T> : Immutable
    {
        /// <inheritdoc/>
        internal override int ComparerCount => 1;

        /// <inheritdoc/>
        internal override int EntryWidth => 1;

        /// <inheritdoc/>
        internal override object?[] Comparers(object collection) => [Saved(((FrozenSet<T>)collection).Comparer)];

        /// <inheritdoc/>
        internal override object?[] Entries(object collection) => [.. ((FrozenSet<T>)collection).Select(static member => (object?)member)];

        /// <inheritdoc/>
        internal override object Build(object?[] comparers, object?[] entries)
        {
            IEqualityComparer<T>? comparer = Restored<T>(comparers[0]);
            var set = new HashSet<T>(entries.Length, comparer);
            foreach (object? member in entries)
            {
                if (!set.Add(Entry<T>(member)))
                    throw Duplicate(typeof(FrozenSet<T>), member);
            }
            return set.ToFrozenSet(comparer);
        }
    }
}
