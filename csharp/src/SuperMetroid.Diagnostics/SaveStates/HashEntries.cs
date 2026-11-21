namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// How a debugger state holds a collection that keys its members by hash (#1681): its comparers and its entries in its own
/// enumeration order, never its buckets. A key's hash can depend on the process that computed it, so a restore adds each
/// saved entry to a new collection with the saved comparers, and every key lands where this process hashes it. A comparer
/// that is its key type's default is saved as none, so a restore uses this process's default.
/// </summary>
internal static class HashEntries
{
    /// <summary><paramref name="comparer"/> as saved: none when it is <typeparamref name="T"/>'s default comparer.</summary>
    internal static object? Saved<T>(IEqualityComparer<T> comparer) =>
        ReferenceEquals(comparer, EqualityComparer<T>.Default) ? null : comparer;

    /// <summary>The failure for a saved collection that holds one key twice under this process's hashing.</summary>
    internal static InvalidDataException Duplicate(string collection, object? key) =>
        new($"A saved {collection} holds the key {key} twice once rebuilt in this process.");

    /// <summary>The failure for a key a collection built at once must hash while its fields are still being read.</summary>
    internal static InvalidDataException Enclosing(string collection, object key) =>
        new($"Debugger state cannot rebuild the saved {collection}: its key {key.GetType().FullName} encloses it, so the key's fields are still being read when the collection must hash it.");

    /// <summary>Reads <paramref name="count"/> keys of a collection built at once, each whole by the time it is read.</summary>
    internal static TKey Key<TKey>(GraphReader reader, string collection)
    {
        TKey key = reader.ReadElement<TKey>();
        if (key is not null && reader.IsRestoring(key))
            throw Enclosing(collection, key);
        return key;
    }
}
