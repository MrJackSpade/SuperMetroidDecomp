namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A dictionary, rebuilt from its comparer and entries once the whole state is read (#1681).</summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal sealed class DictionaryStateType<TKey, TValue>(StateGraphTypes types) : StateGraphType(typeof(Dictionary<TKey, TValue>), types)
    where TKey : notnull
{
    /// <inheritdoc/>
    protected override string Named() =>
        "System.Collections.Generic.Dictionary`2<" + Types.NameOf(typeof(TKey)) + "," + Types.NameOf(typeof(TValue)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var dictionary = (Dictionary<TKey, TValue>)value;
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
        var dictionary = new Dictionary<TKey, TValue>(reader.Read<IEqualityComparer<TKey>?>());
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
            dictionary.EnsureCapacity(count);
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
        foreach ((TKey key, TValue entry) in (Dictionary<TKey, TValue>)value)
        {
            string place = index++.ToString(System.Globalization.CultureInfo.InvariantCulture);
            children.Part("#" + place + ":key", key);
            children.Part("#" + place + ":value", entry);
        }
    }

    /// <inheritdoc/>
    internal override int? Version(object value) => ((Dictionary<TKey, TValue>)value).Count;
}
