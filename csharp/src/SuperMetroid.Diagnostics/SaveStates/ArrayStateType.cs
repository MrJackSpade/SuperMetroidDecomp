using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// An array of <typeparamref name="T"/>: a scalar's or a packed pixel's elements as their raw bytes (#1403, #1633), an
/// array of the game's value type whose fields are scalars, enums, such value types or objects as one packed run (#1696,
/// #2609), the objects held as places in the run's table of them, any other array element by element.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class ArrayStateType<T>(StateGraphTypes types) : StateGraphType(typeof(T[]), types)
{
    /// <summary>Whether the elements are written as their raw bytes: scalars and packed pixels.</summary>
    private static readonly bool Raw =
        typeof(T) == typeof(bool) || typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte) || typeof(T) == typeof(short) ||
        typeof(T) == typeof(ushort) || typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(long) ||
        typeof(T) == typeof(ulong) || typeof(T) == typeof(char) || typeof(T) == typeof(float) || typeof(T) == typeof(double) ||
        typeof(T) == typeof(RgbaPixel);

    /// <summary>Whether the elements are values whose fields a restore reads, where a zero enum is unused storage.</summary>
    private static readonly bool StructElements = typeof(T).IsValueType && !Raw && Nullable.GetUnderlyingType(typeof(T)) is null;

    /// <inheritdoc/>
    protected override string Named() => Types.NameOf(typeof(T)) + "[]";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value) => WriteArray(writer, (T[])value);

    /// <summary>Writes an array's payload kind and payload.</summary>
    internal void WriteArray(GraphWriter writer, T[] array)
    {
        if (Raw)
        {
            writer.Kind(PayloadKind.PrimitiveArray);
            writer.Output.Write(array.Length);
            Span<byte> bytes = Bytes(array);
            writer.Output.Write(bytes.Length);
            writer.Output.Write(bytes);
            return;
        }
        if (PackedNode(Types) is not null)
        {
            WritePacked(writer, array);
            return;
        }
        writer.Kind(PayloadKind.Array);
        writer.Output.Write(array.Length);
        foreach (T element in array)
            writer.WriteElement(element);
    }

    /// <summary>The element type's packed handling, or none when its arrays are written element by element.</summary>
    /// <param name="types">The registry naming the element type.</param>
    internal static NodeStateType<T>? PackedNode(StateGraphTypes types) =>
        typeof(T).IsValueType && types.NodeOf<T>() is { PackedSchema: not null } node ? node : null;

    /// <summary>
    /// Writes <paramref name="elements"/> as one packed run (<see cref="PayloadKind.PackedArray"/>): their count, the run's
    /// schema, its enum and object tables and its bytes. The element type packs (<see cref="PackedNode"/>).
    /// </summary>
    internal static void WritePacked(GraphWriter writer, ReadOnlySpan<T> elements)
    {
        NodeStateType<T> node = PackedNode(writer.Types) ?? throw new InvalidOperationException($"{typeof(T)} elements do not pack.");
        writer.Kind(PayloadKind.PackedArray);
        writer.Output.Write(elements.Length);
        writer.Output.Write(node.PackedSchema!);
        var tables = new PackedEnumTables(writer.Types);
        var objects = new PackedObjects();
        // Each run has its own buffer: an object in its table can hold runs of its own, written while this one is.
        using var values = new MemoryStream();
        using (var packed = new BinaryWriter(values, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            var packedWriter = new PackedWriter(packed, writer.Types, tables, objects);
            foreach (T element in elements)
                node.WriteTyped(packedWriter, element);
        }
        tables.Write(writer.Output);
        objects.Write(writer);
        writer.Output.Write(checked((int)values.Length));
        writer.Output.Write(values.GetBuffer(), 0, (int)values.Length);
    }

    /// <summary>Reads a packed run's elements, after its count, into <paramref name="elements"/>, which holds that many.</summary>
    /// <param name="reader">The state being read.</param>
    /// <param name="node">The element type's packed handling.</param>
    /// <param name="name">The collection's name in failures.</param>
    /// <param name="elements">The elements read.</param>
    internal static void ReadPacked(GraphReader reader, NodeStateType<T> node, string name, Span<T> elements)
    {
        string schema = reader.Input.ReadString();
        if (schema != node.PackedSchema)
        {
            throw new InvalidDataException(
                $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the state packs {name} as {schema}; this build packs it as {node.PackedSchema}.");
        }
        PackedEnumTables tables = PackedEnumTables.Read(reader);
        PackedObjects objects = PackedObjects.Read(reader);
        int byteCount = reader.ReadLength("packed element byte");
        byte[] bytes = reader.Input.ReadBytes(byteCount);
        if (bytes.Length != byteCount)
            throw new EndOfStreamException($"Debugger state ended inside the packed elements of {name}.");
        using var values = new BinaryReader(new MemoryStream(bytes, writable: false), System.Text.Encoding.UTF8);
        var packedReader = new PackedReader(values, reader.Types, tables, objects);
        for (int index = 0; index < elements.Length; index++)
            elements[index] = node.Create(packedReader);
        if (values.BaseStream.Position != byteCount)
            throw new InvalidDataException($"The packed elements of the saved {name} hold {byteCount} bytes; its {elements.Length} elements read {values.BaseStream.Position}.");
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        var array = new T[reader.ReadLength("array")];
        reader.Register(referenceId, array);
        if (kind == PayloadKind.PrimitiveArray && Raw)
        {
            {
                Span<byte> bytes = Bytes(array);
                int stored = reader.ReadLength("array byte");
                if (stored != bytes.Length)
                    throw new InvalidDataException($"{Name} declares {stored} bytes where this build stores {bytes.Length}.");
                reader.Input.BaseStream.ReadExactly(bytes);
                return array;
            }
        }
        if (kind == PayloadKind.PackedArray && PackedNode(Types) is { } node)
        {
            ReadPacked(reader, node, Name, array);
            return array;
        }
        if (kind != PayloadKind.Array)
            throw UnexpectedKind(kind);
        if (StructElements)
            reader.InStructElementsDo(() => ReadElements(reader, array));
        else
            ReadElements(reader, array);
        return array;
    }

    private static void ReadElements(GraphReader reader, T[] array)
    {
        for (int index = 0; index < array.Length; index++)
            array[index] = reader.ReadElement<T>();
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;
        var array = (T[])value;
        for (int index = 0; index < array.Length; index++)
            children.Element(index, array[index]);
    }

    /// <summary>The array's element storage viewed as bytes.</summary>
    private static Span<byte> Bytes(T[] array) =>
        MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(array)), checked(array.Length * Unsafe.SizeOf<T>()));
}
