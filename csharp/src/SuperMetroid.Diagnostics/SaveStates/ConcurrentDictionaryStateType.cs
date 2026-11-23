using System.Collections.Concurrent;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A concurrent dictionary, rebuilt from its comparer and entries once the whole state is read (#1681).</summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal sealed class ConcurrentDictionaryStateType<TKey, TValue>(StateGraphTypes types) : StateGraphType(typeof(ConcurrentDictionary<TKey, TValue>), types)
    where TKey : notnull
{
    /// <inheritdoc/>
    protected override string Named() =>
        "System.Collections.Concurrent.ConcurrentDictionary`2<" + Types.NameOf(typeof(TKey)) + "," + Types.NameOf(typeof(TValue)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var dictionary = (ConcurrentDictionary<TKey, TValue>)value;
        KeyValuePair<TKey, TValue>[] pairs = dictionary.ToArray();
        writer.Kind(PayloadKind.HashEntries);
        writer.Write(HashEntries.Saved(dictionary.Comparer));
        writer.Output.Write(pairs.Length);
        foreach ((TKey key, TValue entry) in pairs)
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
        var dictionary = new ConcurrentDictionary<TKey, TValue>(reader.Read<IEqualityComparer<TKey>?>());
        reader.Register(referenceId, dictionary);
        int count = reader.ReadLength("hash entry");
        var keys = new TKey[count];
        var values = new TValue[count];
        for (int index = 0; index < count; index++)
        {
            keys[index] = reader.ReadElement<TKey>();
            values[index] = reader.ReadElement<TValue>();
        }
        reader.Defer(() =>
        {
            for (int index = 0; index < count; index++)
            {
                if (!dictionary.TryAdd(keys[index], values[index]))
                    throw HashEntries.Duplicate(Name, keys[index]);
            }
        });
        return dictionary;
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        int index = 0;
        foreach ((TKey key, TValue entry) in ((ConcurrentDictionary<TKey, TValue>)value).ToArray())
        {
            string place = index++.ToString(System.Globalization.CultureInfo.InvariantCulture);
            children.Part("#" + place + ":key", key);
            children.Part("#" + place + ":value", entry);
        }
    }
}
