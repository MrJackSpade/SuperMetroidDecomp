namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A queue of <typeparamref name="T"/>: its elements in the order they leave it.</summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class QueueStateType<T>(StateGraphTypes types) : StateGraphType(typeof(Queue<T>), types)
{
    /// <inheritdoc/>
    protected override string Named() => "System.Collections.Generic.Queue`1<" + Types.NameOf(typeof(T)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var queue = (Queue<T>)value;
        writer.Kind(PayloadKind.Sequence);
        writer.Output.Write(queue.Count);
        foreach (T element in queue)
            writer.WriteElement(element);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.Sequence)
            throw UnexpectedKind(kind);
        int count = reader.ReadLength("queue");
        var queue = new Queue<T>(count);
        reader.Register(referenceId, queue);
        for (int index = 0; index < count; index++)
            queue.Enqueue(reader.ReadElement<T>());
        return queue;
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        int index = 0;
        foreach (T element in (Queue<T>)value)
            children.Element(index++, element);
    }

    /// <inheritdoc/>
    internal override int? Version(object value) => ((Queue<T>)value).Count;
}
