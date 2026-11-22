namespace SuperMetroid.Core;

/// <summary>
/// Learns how a game snapshot holds each type of the game (#2602). Each marked project's registration is generated: every
/// type's own members, the collections, arrays and enums its fields are declared with, the methods its delegates are made
/// from, and the definition catalogs its static members hold, so a snapshot names every type, field and method it holds in
/// code rather than looking them up while the game runs.
/// </summary>
public interface IStateGraphRegistry
{
    /// <summary>
    /// Whether <paramref name="type"/> is registered for the first time now, so its registration follows; false when it
    /// was registered before, which ends a registration that reaches it again.
    /// </summary>
    /// <param name="type">The type.</param>
    bool Begin(Type type);

    /// <summary>Names a type no object of which a snapshot holds, such as a static class a snapshot can name as a value.</summary>
    /// <param name="type">The type.</param>
    /// <param name="name">Its name.</param>
    void Name(Type type, StateGraphName name);

    /// <summary>Registers a type of the game whose fields a snapshot holds through its generated members.</summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="name">Its name.</param>
    /// <param name="write">Writes a value's fields.</param>
    /// <param name="create">Restores a value from its fields; none for an abstract type.</param>
    /// <param name="visit">Hands each field that can reach another object to a visitor.</param>
    /// <param name="packedSchema">
    /// For a value type whose fields are all scalars, enums or such value types, its fields' names and types, which an
    /// array of it is written with once before its values are packed back to back; none otherwise.
    /// </param>
    void Node<T>(StateGraphName name, Action<IStateGraphWriter, T> write, Func<IStateGraphReader, T>? create,
        Action<T, IStateGraphVisitor> visit, string? packedSchema);

    /// <summary>Registers an enum, which a snapshot holds by member name.</summary>
    /// <typeparam name="TEnum">The enum.</typeparam>
    /// <param name="name">Its name.</param>
    /// <param name="flags">Whether its members are flags a value combines, so any combination of them is a value.</param>
    void Enum<TEnum>(StateGraphName name, bool flags) where TEnum : struct, Enum;

    /// <summary>Registers arrays of <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    void Array<T>();

    /// <summary>Registers grids of <typeparamref name="T"/>: arrays of two dimensions.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    void Grid<T>();

    /// <summary>Registers read-only views of a run of <typeparamref name="T"/>, held as the array they view and where.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    void ReadOnlyMemory<T>();

    /// <summary>Registers lists of <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    void List<T>();

    /// <summary>Registers queues of <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    void Queue<T>();

    /// <summary>Registers hash sets of <typeparamref name="T"/>, which a snapshot rebuilds from their members (#1681).</summary>
    /// <typeparam name="T">The member type.</typeparam>
    void HashSet<T>();

    /// <summary>Registers dictionaries, which a snapshot rebuilds from their entries (#1681).</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    void Dictionary<TKey, TValue>() where TKey : notnull;

    /// <summary>Registers concurrent dictionaries, which a snapshot rebuilds from their entries (#1681).</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    void ConcurrentDictionary<TKey, TValue>() where TKey : notnull;

    /// <summary>Registers immutable arrays of <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    void ImmutableArray<T>();

    /// <summary>Registers immutable dictionaries, which a snapshot rebuilds from their entries (#1681).</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    void ImmutableDictionary<TKey, TValue>() where TKey : notnull;

    /// <summary>Registers immutable hash sets, which a snapshot rebuilds from their members (#1681).</summary>
    /// <typeparam name="T">The member type.</typeparam>
    void ImmutableHashSet<T>();

    /// <summary>Registers frozen dictionaries, which a snapshot rebuilds from their entries (#1681).</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    void FrozenDictionary<TKey, TValue>() where TKey : notnull;

    /// <summary>Registers frozen sets, which a snapshot rebuilds from their members (#1681).</summary>
    /// <typeparam name="T">The member type.</typeparam>
    void FrozenSet<T>();

    /// <summary>Registers read-only collections of <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    void ReadOnlyCollection<T>();

    /// <summary>
    /// Registers a framework object a snapshot never holds, such as a cache a definition keeps beside its data: it reaches
    /// nothing a snapshot names, and a snapshot of a game holding one is refused.
    /// </summary>
    /// <typeparam name="T">The object's type.</typeparam>
    void Unheld<T>() where T : class;

    /// <summary>
    /// Registers the framework's comparers of <typeparamref name="T"/> a field declared as a comparer may hold, such as one
    /// made from a comparison: a snapshot never holds one, and it reaches nothing a snapshot names.
    /// </summary>
    /// <typeparam name="T">The type compared.</typeparam>
    void Comparer<T>();

    /// <summary>Registers lazily loaded parts of <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    void Lazy<T>();

    /// <summary>Registers key and value pairs.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    void KeyValuePair<TKey, TValue>();

    /// <summary>
    /// Registers what a field declared as a sequence of <typeparamref name="T"/> (a read-only list, a collection or an
    /// enumerable) may hold: arrays, lists, immutable arrays, read-only collections, and the read-only lists a collection
    /// expression makes, which a snapshot holds as arrays.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    void Sequence<T>();

    /// <summary>Registers what a field declared as a read-only or other dictionary interface may hold.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    void Map<TKey, TValue>() where TKey : notnull;

    /// <summary>Registers what a field declared as a read-only or other set interface may hold.</summary>
    /// <typeparam name="T">The member type.</typeparam>
    void Set<T>();

    /// <summary>
    /// Registers a method a delegate of <typeparamref name="TDelegate"/> is made from, so a snapshot names the method a
    /// delegate calls by <paramref name="name"/> and remakes it for the restored target.
    /// </summary>
    /// <typeparam name="TDelegate">The delegate type.</typeparam>
    /// <param name="name">The method's name, after the name of the type declaring it, with what it takes.</param>
    /// <param name="create">Makes the delegate for a target, or answers none when the target is not one the method takes.</param>
    void Method<TDelegate>(string name, Func<object?, TDelegate?> create) where TDelegate : Delegate;

    /// <summary>Registers a definition catalog: a type's static read-only members holding objects (#1623).</summary>
    /// <param name="type">The type's name.</param>
    /// <param name="members">Its members, in their names' ordinal order.</param>
    void Catalog(StateGraphName type, IReadOnlyList<StateGraphCatalogMember> members);

    /// <summary>
    /// Registers types the locations the catalogs' members reach are declared with (#1716), each named by its key, with the
    /// test of whether an object is of the type at a place among them.
    /// </summary>
    /// <param name="keys">The declared types' names.</param>
    /// <param name="probe">Whether an object is of the declared type at a place among <paramref name="keys"/>.</param>
    /// <param name="exact">
    /// For each declared type an object of which has exactly that runtime type (a value type, a sealed class), the type, so
    /// an object is matched to it by its runtime type rather than tested; none for any other.
    /// </param>
    /// <param name="enumerates">
    /// For each declared type, whether only an object that enumerates can be of it (an array, a collection interface), so
    /// an object that does not is not tested against it.
    /// </param>
    void DeclaredTypes(IReadOnlyList<string> keys, Func<int, object, bool> probe, IReadOnlyList<Type?> exact, IReadOnlyList<bool> enumerates);

    /// <summary>
    /// Registers what a location declared with the type <paramref name="key"/> names holds (#1716): the declared types of
    /// the locations it holds in turn, or anything at all.
    /// </summary>
    /// <param name="key">The declared type's name.</param>
    /// <param name="anything">Whether the location can hold an object of any type.</param>
    /// <param name="held">The names of the declared types it holds.</param>
    void Holds(string key, bool anything, IReadOnlyList<string> held);
}
