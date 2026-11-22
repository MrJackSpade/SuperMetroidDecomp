using System.Collections.ObjectModel;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A read-only collection of <typeparamref name="T"/>: its elements, which a restore wraps again.</summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class ReadOnlyCollectionStateType<T>(StateGraphTypes types) : StateGraphType(typeof(ReadOnlyCollection<T>), types)
{
    /// <inheritdoc/>
    protected override string Named() => "System.Collections.ObjectModel.ReadOnlyCollection`1<" + Types.NameOf(typeof(T)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var collection = (ReadOnlyCollection<T>)value;
        writer.Kind(PayloadKind.Sequence);
        writer.Output.Write(collection.Count);
        foreach (T element in collection)
            writer.WriteElement(element);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.Sequence)
            throw UnexpectedKind(kind);
        var elements = new T[reader.ReadLength("read-only collection")];
        var collection = new ReadOnlyCollection<T>(elements);
        reader.Register(referenceId, collection);
        for (int index = 0; index < elements.Length; index++)
            elements[index] = reader.ReadElement<T>();
        return collection;
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        var collection = (ReadOnlyCollection<T>)value;
        for (int index = 0; index < collection.Count; index++)
            children.Element(index, collection[index]);
    }
}
