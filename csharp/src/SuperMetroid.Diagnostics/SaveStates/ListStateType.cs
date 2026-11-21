namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A list of <typeparamref name="T"/>: its capacity, then its elements.</summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class ListStateType<T>(StateGraphTypes types) : StateGraphType(typeof(List<T>), types)
{
    /// <inheritdoc/>
    protected override string Named() => "System.Collections.Generic.List`1<" + Types.NameOf(typeof(T)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var list = (List<T>)value;
        writer.Kind(PayloadKind.Sequence);
        writer.Output.Write(list.Capacity);
        writer.Output.Write(list.Count);
        foreach (T element in list)
            writer.WriteElement(element);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.Sequence)
            throw UnexpectedKind(kind);
        var list = new List<T>(reader.ReadLength("list capacity"));
        reader.Register(referenceId, list);
        int count = reader.ReadLength("list");
        for (int index = 0; index < count; index++)
            list.Add(reader.ReadElement<T>());
        return list;
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        var list = (List<T>)value;
        for (int index = 0; index < list.Count; index++)
            children.Element(index, list[index]);
    }

    /// <inheritdoc/>
    internal override int? Version(object value) => ((List<T>)value).Count;
}
