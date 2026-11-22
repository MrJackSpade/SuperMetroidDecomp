using System.Collections.Frozen;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// A frozen dictionary, whatever specialization the framework made of it, rebuilt from its comparer and entries as soon as
/// they are read (#1681).
/// </summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal sealed class FrozenDictionaryStateType<TKey, TValue>(StateGraphTypes types) : StateGraphType(typeof(FrozenDictionary<TKey, TValue>), types)
    where TKey : notnull
{
    /// <inheritdoc/>
    protected override string Named() =>
        "System.Collections.Frozen.FrozenDictionary`2<" + Types.NameOf(typeof(TKey)) + "," + Types.NameOf(typeof(TValue)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var dictionary = (FrozenDictionary<TKey, TValue>)value;
        writer.Kind(PayloadKind.HashEntries);
        writer.Write(HashEntries.Saved(dictionary.Comparer));
        writer.Output.Write(dictionary.Count);
        foreach ((TKey key, TValue entry) in dictionary)
        {
            writer.WriteElement(key);
            writer.WriteElement(entry);
        }
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.HashEntries)
            throw UnexpectedKind(kind);
        IEqualityComparer<TKey>? comparer = reader.Read<IEqualityComparer<TKey>?>();
        int count = reader.ReadLength("hash entry");
        var dictionary = new Dictionary<TKey, TValue>(count, comparer);
        for (int index = 0; index < count; index++)
        {
            TKey key = HashEntries.Key<TKey>(reader, Name);
            if (!dictionary.TryAdd(key, reader.ReadElement<TValue>()))
                throw HashEntries.Duplicate(Name, key);
        }
        return reader.Register(referenceId, dictionary.ToFrozenDictionary(comparer));
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        int index = 0;
        foreach ((TKey key, TValue entry) in (FrozenDictionary<TKey, TValue>)value)
        {
            string place = index++.ToString(System.Globalization.CultureInfo.InvariantCulture);
            children.Part("#" + place + ":key", key);
            children.Part("#" + place + ":value", entry);
        }
    }
}
