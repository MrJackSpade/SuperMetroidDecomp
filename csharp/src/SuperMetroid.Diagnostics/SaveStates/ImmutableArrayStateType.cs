using System.Runtime.CompilerServices;
using System.Collections.Immutable;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// An immutable array of <typeparamref name="T"/>: the default as such; elements of the game's value type that pack as one
/// packed run, as an array's do (<see cref="ArrayStateType{T}.WritePacked"/>); any other element by element.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class ImmutableArrayStateType<T>(StateGraphTypes types) : StateGraphType(typeof(ImmutableArray<T>), types)
{
    /// <inheritdoc/>
    internal override bool ReachesObjects => RuntimeHelpers.IsReferenceOrContainsReferences<T>();

    /// <inheritdoc/>
    protected override string Named() => "System.Collections.Immutable.ImmutableArray`1<" + Types.NameOf(typeof(T)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var array = (ImmutableArray<T>)value;
        if (!array.IsDefault && ArrayStateType<T>.PackedNode(Types) is not null)
        {
            ArrayStateType<T>.WritePacked(writer, array.AsSpan());
            return;
        }
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
        if (kind == PayloadKind.PackedArray && ArrayStateType<T>.PackedNode(Types) is { } node)
        {
            var elements = new T[reader.ReadLength("immutable array")];
            ArrayStateType<T>.ReadPacked(reader, node, Name, elements);
            return System.Collections.Immutable.ImmutableArray.Create(elements);
        }
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
