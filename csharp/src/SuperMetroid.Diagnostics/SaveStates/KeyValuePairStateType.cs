namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A key and value pair: its key, then its value.</summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal sealed class KeyValuePairStateType<TKey, TValue>(StateGraphTypes types) : StateGraphType(typeof(KeyValuePair<TKey, TValue>), types)
{
    /// <inheritdoc/>
    protected override string Named() =>
        "System.Collections.Generic.KeyValuePair`2<" + Types.NameOf(typeof(TKey)) + "," + Types.NameOf(typeof(TValue)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var pair = (KeyValuePair<TKey, TValue>)value;
        writer.Kind(PayloadKind.Sequence);
        writer.WriteElement(pair.Key);
        writer.WriteElement(pair.Value);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.Sequence)
            throw UnexpectedKind(kind);
        TKey key = reader.ReadElement<TKey>();
        return new KeyValuePair<TKey, TValue>(key, reader.ReadElement<TValue>());
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        var pair = (KeyValuePair<TKey, TValue>)value;
        children.Part("key", pair.Key);
        children.Part("value", pair.Value);
    }
}
