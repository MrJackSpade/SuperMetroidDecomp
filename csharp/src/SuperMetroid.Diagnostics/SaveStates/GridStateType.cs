using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// A grid of <typeparamref name="T"/>, an array of two dimensions: its two lengths, then a scalar's elements as their raw
/// bytes or any other element by element, row by row.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class GridStateType<T>(StateGraphTypes types) : StateGraphType(typeof(T[,]), types)
{
    /// <inheritdoc/>
    internal override bool ReachesObjects => RuntimeHelpers.IsReferenceOrContainsReferences<T>();

    /// <summary>Whether the elements are written as their raw bytes.</summary>
    private static readonly bool Raw = !RuntimeHelpers.IsReferenceOrContainsReferences<T>() && typeof(T).IsPrimitive;

    /// <inheritdoc/>
    protected override string Named() => Types.NameOf(typeof(T)) + "[,]";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var grid = (T[,])value;
        writer.Kind(Raw ? PayloadKind.PrimitiveArray : PayloadKind.Array);
        writer.Output.Write(grid.GetLength(0));
        writer.Output.Write(grid.GetLength(1));
        if (Raw)
        {
            writer.Output.Write(Bytes(grid));
            return;
        }
        foreach (T element in grid)
            writer.WriteElement(element);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != (Raw ? PayloadKind.PrimitiveArray : PayloadKind.Array))
            throw UnexpectedKind(kind);
        var grid = new T[reader.ReadLength("grid row"), reader.ReadLength("grid column")];
        reader.Register(referenceId, grid);
        if (Raw)
        {
            reader.Input.BaseStream.ReadExactly(Bytes(grid));
            return grid;
        }
        for (int row = 0; row < grid.GetLength(0); row++)
            for (int column = 0; column < grid.GetLength(1); column++)
                grid[row, column] = reader.ReadElement<T>();
        return grid;
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;
        var grid = (T[,])value;
        for (int row = 0; row < grid.GetLength(0); row++)
            for (int column = 0; column < grid.GetLength(1); column++)
                children.Part("#" + row.ToString(CultureInfo.InvariantCulture) + "," + column.ToString(CultureInfo.InvariantCulture), grid[row, column]);
    }

    /// <summary>The grid's element storage viewed as bytes.</summary>
    private static Span<byte> Bytes(T[,] grid) =>
        MemoryMarshal.CreateSpan(ref MemoryMarshal.GetArrayDataReference((Array)grid), checked(grid.Length * Unsafe.SizeOf<T>()));
}
