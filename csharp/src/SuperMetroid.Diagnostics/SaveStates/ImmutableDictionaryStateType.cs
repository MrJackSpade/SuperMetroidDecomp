using System.Collections.Immutable;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>An immutable dictionary, rebuilt from its key and value comparers and entries as soon as they are read (#1681).</summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal sealed class ImmutableDictionaryStateType<TKey, TValue>(StateGraphTypes types) : StateGraphType(typeof(ImmutableDictionary<TKey, TValue>), types)
    where TKey : notnull
{
    /// <inheritdoc/>
    protected override string Named() =>
        "System.Collections.Immutable.ImmutableDictionary`2<" + Types.NameOf(typeof(TKey)) + "," + Types.NameOf(typeof(TValue)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var dictionary = (ImmutableDictionary<TKey, TValue>)value;
        writer.Kind(PayloadKind.HashEntries);
        writer.Write(HashEntries.Saved(dictionary.KeyComparer));
        writer.Write(HashEntries.Saved(dictionary.ValueComparer));
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
        IEqualityComparer<TKey>? keys = reader.Read<IEqualityComparer<TKey>?>();
        IEqualityComparer<TValue>? values = reader.Read<IEqualityComparer<TValue>?>();
        ImmutableDictionary<TKey, TValue>.Builder builder = ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keys, values).ToBuilder();
        int count = reader.ReadLength("hash entry");
        for (int index = 0; index < count; index++)
        {
            TKey key = HashEntries.Key<TKey>(reader, Name);
            if (!builder.TryAdd(key, reader.ReadElement<TValue>()))
                throw HashEntries.Duplicate(Name, key);
        }
        return reader.Register(referenceId, builder.ToImmutable());
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        int index = 0;
        foreach ((TKey key, TValue entry) in (ImmutableDictionary<TKey, TValue>)value)
        {
            string place = index++.ToString(System.Globalization.CultureInfo.InvariantCulture);
            children.Part("#" + place + ":key", key);
            children.Part("#" + place + ":value", entry);
        }
    }
}
