using System.Collections.Immutable;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>An immutable array of <typeparamref name="T"/>: whether it is the default, then its elements.</summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class ImmutableArrayStateType<T>(StateGraphTypes types) : StateGraphType(typeof(ImmutableArray<T>), types)
{
    /// <inheritdoc/>
    protected override string Named() => "System.Collections.Immutable.ImmutableArray`1<" + Types.NameOf(typeof(T)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var array = (ImmutableArray<T>)value;
        writer.Kind(PayloadKind.Sequence);
        writer.Output.Write(array.IsDefault);
        if (array.IsDefault)
            return;
        writer.Output.Write(array.Length);
        foreach (T element in array)
            writer.WriteElement(element);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.Sequence)
            throw UnexpectedKind(kind);
        if (reader.Input.ReadBoolean())
            return default(ImmutableArray<T>);
        int count = reader.ReadLength("immutable array");
        ImmutableArray<T>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<T>(count);
        for (int index = 0; index < count; index++)
            builder.Add(reader.ReadElement<T>());
        return builder.MoveToImmutable();
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        var array = (ImmutableArray<T>)value;
        if (array.IsDefault)
            return;
        for (int index = 0; index < array.Length; index++)
            children.Element(index, array[index]);
    }
}
