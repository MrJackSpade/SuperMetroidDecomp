using System.Runtime.InteropServices;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// A read-only view of a run of <typeparamref name="T"/>: the array it views, written as any shared array is, and where in
/// it the view starts and how long it is.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class ReadOnlyMemoryStateType<T>(StateGraphTypes types) : StateGraphType(typeof(ReadOnlyMemory<T>), types)
{
    /// <inheritdoc/>
    protected override string Named() => "System.ReadOnlyMemory`1<" + Types.NameOf(typeof(T)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var memory = (ReadOnlyMemory<T>)value;
        if (!MemoryMarshal.TryGetArray(memory, out ArraySegment<T> segment))
            throw new NotSupportedException($"Debugger state cannot hold a {Name} over memory that is no array.");
        writer.Kind(PayloadKind.Sequence);
        writer.Write(segment.Array);
        writer.Output.Write(segment.Offset);
        writer.Output.Write(segment.Count);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.Sequence)
            throw UnexpectedKind(kind);
        T[]? array = reader.Read<T[]?>();
        int offset = reader.ReadLength("memory offset");
        int count = reader.ReadLength("memory length");
        return array is null ? default(ReadOnlyMemory<T>) : new ReadOnlyMemory<T>(array, offset, count);
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        if (MemoryMarshal.TryGetArray((ReadOnlyMemory<T>)value, out ArraySegment<T> segment))
            children.Part("=array", segment.Array);
    }
}
