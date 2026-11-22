namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A stack of <typeparamref name="T"/>: its elements from the top down.</summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class StackStateType<T>(StateGraphTypes types) : StateGraphType(typeof(Stack<T>), types)
{
    /// <inheritdoc/>
    protected override string Named() => "System.Collections.Generic.Stack`1<" + Types.NameOf(typeof(T)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var stack = (Stack<T>)value;
        writer.Kind(PayloadKind.Sequence);
        writer.Output.Write(stack.Count);
        foreach (T element in stack)
            writer.WriteElement(element);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.Sequence)
            throw UnexpectedKind(kind);
        int count = reader.ReadLength("stack");
        var stack = new Stack<T>(count);
        reader.Register(referenceId, stack);
        var elements = new T[count];
        for (int index = 0; index < count; index++)
            elements[index] = reader.ReadElement<T>();
        for (int index = count - 1; index >= 0; index--)
            stack.Push(elements[index]);
        return stack;
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        int index = 0;
        foreach (T element in (Stack<T>)value)
            children.Element(index++, element);
    }

    /// <inheritdoc/>
    internal override int? Version(object value) => ((Stack<T>)value).Count;
}
