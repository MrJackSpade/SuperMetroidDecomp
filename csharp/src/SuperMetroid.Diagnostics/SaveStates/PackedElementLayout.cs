using SuperMetroid.Core;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Expr = System.Linq.Expressions.Expression;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// How a debugger state writes an array whose elements are values with no identity of their own (#1696): enums, the
/// scalars that are not stored raw (<see cref="RawArrayElements"/>), structs whose every field is such a value, and
/// nullable values of them. Such an array is one packed run: a schema naming the element type and each nested field once,
/// a table of the members the run holds of each enum, then every element's values back to back, with no per-element
/// marker, type name or field header. A room's block arrays were written a framed object per block, over a kilobyte each.
/// </summary>
/// <remarks>
/// <para>
/// Packing is decided statically by the element type, never by the values: a value-type element holds exactly its own
/// type, so every element of a run has the schema's layout. An element type that can hold a reference (a class, an
/// interface, or a struct with a reference, pointer or inline-array field), that is a hash-keyed collection or that
/// reattaches shared content after a restore has no packed layout, and its arrays keep the framed form, element by
/// element, with every identity, shared reference, bound and definition path that form carries.
/// </para>
/// <para>
/// An enum is stored by its member's name, as a framed enum is (#1507): the run's table holds each distinct member's name
/// once, and an element holds the member's index in that table. A restore checks the schema against this build field by
/// field, as it checks a framed object's fields, before it reads a value.
/// </para>
/// </remarks>
internal sealed class PackedElementLayout
{
    /// <summary>How many elements of a vector repay compiling its element writer at once.</summary>
    private const int CompileForElements = 64;

    /// <summary>Each element type's packed layout, or null for a type whose arrays are written framed, worked out once.</summary>
    private static readonly ConcurrentDictionary<Type, PackedElementLayout?> layouts = new();

    /// <summary>The element's layout: its schema, its values' writers and readers.</summary>
    private readonly Node root;

    /// <summary>The enum each table slot holds members of, in the order the schema first names each.</summary>
    private readonly Type[] enumSlots;

    /// <summary>The member names of each slot's enum.</summary>
    private readonly DebuggerEnumNames[] enumNames;

    /// <summary>How many elements were written by reflection before the vector writer was compiled.</summary>
    private int reflectiveElements;

    /// <summary>The compiled writer of an element of a vector of the type, once compiled.</summary>
    private Action<BinaryWriter, EnumTables, Array, int>? vectorWriter;

    /// <summary>The layout of <paramref name="elementType"/>, whose values <paramref name="root"/> lays out.</summary>
    private PackedElementLayout(Type elementType, Node root, Type[] enumSlots)
    {
        ElementType = elementType;
        this.root = root;
        this.enumSlots = enumSlots;
        enumNames = [.. enumSlots.Select(slot => DebuggerEnumNames.For(slot) ??
            throw new InvalidOperationException($"Enum {slot.FullName} has no member names."))];
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            root.WriteSchema(writer);
        Schema = stream.ToArray();
    }

    /// <summary>The element type.</summary>
    internal Type ElementType { get; }

    /// <summary>The schema every run of the element type starts with, as written.</summary>
    internal byte[] Schema { get; }

    /// <summary>The packed layout of <paramref name="elementType"/>, or null when its arrays are written framed.</summary>
    internal static PackedElementLayout? For(Type elementType) =>
        layouts.GetOrAdd(Ensure.NotNull(elementType), static type =>
        {
            var slots = new List<Type>();
            return Build(type, slots) is { } node ? new PackedElementLayout(type, node, [.. slots]) : null;
        });

    /// <summary>
    /// The layout of a value of <paramref name="type"/>, adding each enum it holds to <paramref name="slots"/>; null when the
    /// type has none, so arrays of it are written framed.
    /// </summary>
    [ReflectionAudit("debugger state packing decides from a value type's serialized fields whether its arrays are packed")]
    private static Node? Build(Type type, List<Type> slots)
    {
        if (type.IsEnum)
        {
            if (DebuggerEnumNames.For(type) is not { } names)
                return null;
            int slot = slots.IndexOf(type);
            if (slot < 0)
            {
                slot = slots.Count;
                slots.Add(type);
            }
            return new EnumNode(type, names, slot);
        }
        if (DebuggerPrimitives.IsSupported(type))
            return new PrimitiveNode(type);
        if (Nullable.GetUnderlyingType(type) is { } underlying)
            return Build(underlying, slots) is { } value ? new NullableNode(type, value) : null;
        // A reference can be shared or name a bound or definition object; a hash-keyed collection is rebuilt from its
        // entries; shared content reattaches after its fields are read; an inline array declares one field for many
        // elements. Each of those keeps the framed form.
        if (!type.IsValueType || type.IsPrimitive || type.IsByRefLike || type.ContainsGenericParameters ||
            InlineArrayElements.Length(type) is not null || HashKeyedCollection.For(type) is not null ||
            typeof(SuperMetroid.AssetExtraction.Installation.IRestoredSharedContent).IsAssignableFrom(type))
            return null;
        FieldInfo[] fields = DebuggerObjectGraphSerializer.GetSerializableFields(type);
        var children = new Node[fields.Length];
        for (int index = 0; index < fields.Length; index++)
        {
            Type fieldType = fields[index].FieldType;
            if (fieldType.IsPointer || fieldType.IsFunctionPointer || fieldType.IsByRef || Build(fieldType, slots) is not { } child)
                return null;
            children[index] = child;
        }
        return new StructNode(type, fields, children);
    }

    /// <summary>Empty tables of the enum members a run of the element type holds.</summary>
    internal EnumTables NewTables() => new(enumSlots.Length);

    /// <summary>Writes each slot's table: its member count, then each member's name in the order the run first held it.</summary>
    internal void WriteTables(BinaryWriter writer, EnumTables tables) => tables.Write(writer, enumNames);

    /// <summary>Writes the values of the boxed element <paramref name="element"/>, through reflection.</summary>
    internal void WriteElement(BinaryWriter writer, EnumTables tables, object? element) => root.Write(writer, tables, element);

    /// <summary>
    /// The compiled writer of an element of a vector of the element type, writing exactly what <see cref="WriteElement"/>
    /// writes, compiled once an array of <paramref name="length"/> elements or the type's earlier reflective writes repay
    /// it; null before then.
    /// </summary>
    internal Action<BinaryWriter, EnumTables, Array, int>? VectorWriter(int length)
    {
        if (Volatile.Read(ref vectorWriter) is { } compiled)
            return compiled;
        if (length < CompileForElements && Interlocked.Add(ref reflectiveElements, length) < CompileForElements)
            return null;
        compiled = CompileVectorWriter();
        Volatile.Write(ref vectorWriter, compiled);
        return compiled;
    }

    /// <summary>Compiles the writer of element <c>index</c> of a vector of the element type.</summary>
    [ReflectionAudit("debugger state packing compiles a value type's element writer from its serialized field layout")]
    private Action<BinaryWriter, EnumTables, Array, int> CompileVectorWriter()
    {
        var output = Expr.Parameter(typeof(BinaryWriter), "output");
        var tables = Expr.Parameter(typeof(EnumTables), "tables");
        var array = Expr.Parameter(typeof(Array), "array");
        var index = Expr.Parameter(typeof(int), "index");
        var element = Expr.Variable(ElementType, "element");
        Expr body = Expr.Block([element],
            Expr.Assign(element, Expr.ArrayIndex(Expr.Convert(array, ElementType.MakeArrayType()), index)),
            root.WriteExpression(output, tables, element),
            Expr.Empty());
        return Expr.Lambda<Action<BinaryWriter, EnumTables, Array, int>>(body, output, tables, array, index).Compile();
    }

    /// <summary>
    /// Checks a saved run's schema against this build's layout of the element type, field by field.
    /// </summary>
    /// <exception cref="InvalidDataException">The saved schema differs from this build's layout.</exception>
    internal void CheckSchema(SchemaSource source) => root.CheckSchema(source);

    /// <summary>
    /// Reads each slot's table of members, each decoded and validated once, inside struct storage when
    /// <paramref name="zeroStorageSlot"/> (<see cref="DebuggerEnumValues.Decode"/>).
    /// </summary>
    internal object[][] ReadTables(BinaryReader reader, bool zeroStorageSlot)
    {
        var tables = new object[enumSlots.Length][];
        for (int slot = 0; slot < tables.Length; slot++)
        {
            int count = reader.ReadInt32();
            if (count < 0)
                throw new InvalidDataException($"Debugger state has a negative packed {enumSlots[slot].FullName} member count {count}.");
            var members = new object[count];
            for (int member = 0; member < count; member++)
                members[member] = DebuggerEnumValues.DecodeName(enumSlots[slot], reader.ReadString(), zeroStorageSlot);
            tables[slot] = members;
        }
        return tables;
    }

    /// <summary>Reads one boxed element from a run's values, its enums resolved through <paramref name="tables"/>.</summary>
    internal object? ReadElement(BinaryReader values, object[][] tables) => root.Read(values, tables);

    /// <summary>What a saved schema is read through: the state's reader, and the restore's name reader and type resolution.</summary>
    /// <param name="Reader">The state's reader.</param>
    /// <param name="ReadName">Reads one type or member name.</param>
    /// <param name="Resolve">Resolves a saved type name to this build's type, rejecting a type it does not have.</param>
    internal readonly record struct SchemaSource(BinaryReader Reader, Func<string> ReadName, Func<string, Type> Resolve);

    /// <summary>The distinct members of each enum one run holds, in the order the run first held each.</summary>
    internal sealed class EnumTables
    {
        /// <summary>Each slot's member indices, by member bits.</summary>
        private readonly Dictionary<ulong, int>[] indices;

        /// <summary>Each slot's member bits, in table order.</summary>
        private readonly List<ulong>[] members;

        /// <summary>Empty tables for <paramref name="slots"/> enums.</summary>
        internal EnumTables(int slots)
        {
            indices = new Dictionary<ulong, int>[slots];
            members = new List<ulong>[slots];
            for (int slot = 0; slot < slots; slot++)
            {
                indices[slot] = [];
                members[slot] = [];
            }
        }

        /// <summary>The index in slot <paramref name="slot"/>'s table of the member whose bits are <paramref name="bits"/>, added when new.</summary>
        internal int Index(int slot, ulong bits)
        {
            if (indices[slot].TryGetValue(bits, out int known))
                return known;
            int index = members[slot].Count;
            indices[slot].Add(bits, index);
            members[slot].Add(bits);
            return index;
        }

        /// <summary>Writes each slot's member count, then each member's encoded name from <paramref name="names"/>.</summary>
        internal void Write(BinaryWriter writer, DebuggerEnumNames[] names)
        {
            for (int slot = 0; slot < members.Length; slot++)
            {
                writer.Write(members[slot].Count);
                foreach (ulong bits in members[slot])
                    writer.Write(names[slot].Of(bits));
            }
        }
    }

    /// <summary>Which layout a schema node gives a value, written before its type name.</summary>
    private enum NodeKind : byte
    {
        /// <summary>A scalar written through its fixed representation.</summary>
        Primitive,
        /// <summary>An enum, written as its member's index in the run's table of that enum.</summary>
        Enum,
        /// <summary>A nullable value, written as whether it holds a value, then the value when it does.</summary>
        Nullable,
        /// <summary>A struct, written as its fields' values in their serialized order.</summary>
        Struct,
    }

    /// <summary>One value type's layout within a packed element: its schema, and how a value of it is written and read.</summary>
    /// <param name="type">The value type.</param>
    private abstract class Node(Type type)
    {
        /// <summary>The value type.</summary>
        internal Type Type { get; } = type;

        /// <summary>Which layout the node gives the value.</summary>
        internal abstract NodeKind Kind { get; }

        /// <summary>Writes the node's schema: its kind and type name, then what its kind adds.</summary>
        internal virtual void WriteSchema(BinaryWriter writer)
        {
            writer.Write((byte)Kind);
            writer.Write(DebuggerStateTypeIdentity.GetSerializedName(Type));
        }

        /// <summary>Reads a saved schema node and rejects one that is not this build's: another kind or another type.</summary>
        /// <exception cref="InvalidDataException">The saved node differs from this build's.</exception>
        internal virtual void CheckSchema(SchemaSource source)
        {
            var kind = (NodeKind)source.Reader.ReadByte();
            string name = source.ReadName();
            if (kind != Kind || source.Resolve(name) != Type)
            {
                throw new InvalidDataException(
                    $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: a saved packed run holds {kind} {name} " +
                    $"where this build packs {Kind} {Type.FullName}.");
            }
        }

        /// <summary>Writes the boxed <paramref name="value"/>, through reflection.</summary>
        internal abstract void Write(BinaryWriter writer, EnumTables tables, object? value);

        /// <summary>The expression writing <paramref name="value"/>, a value of the node's type, exactly as <see cref="Write"/> writes it.</summary>
        internal abstract Expr WriteExpression(Expr output, Expr tables, Expr value);

        /// <summary>Reads one boxed value from a run's values.</summary>
        internal abstract object? Read(BinaryReader values, object[][] tables);

        /// <summary>The <see cref="BinaryWriter"/> overload that writes one <paramref name="type"/>.</summary>
        protected static MethodInfo WriterOverload(Type type) => typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), [type])
            ?? throw new InvalidOperationException($"BinaryWriter has no write of {type.FullName}.");

        /// <summary>The value a non-nullable node writes, which reflection always gives.</summary>
        protected object Present(object? value) =>
            value ?? throw new InvalidOperationException($"A packed {Type.FullName} value is null.");
    }

    /// <summary>A scalar, written through its fixed representation (<see cref="DebuggerPrimitives"/>).</summary>
    /// <param name="type">The scalar type.</param>
    private sealed class PrimitiveNode(Type type) : Node(type)
    {
        /// <inheritdoc/>
        internal override NodeKind Kind => NodeKind.Primitive;

        /// <inheritdoc/>
        internal override void Write(BinaryWriter writer, EnumTables tables, object? value) =>
            DebuggerPrimitives.Write(writer, Type, Present(value));

        /// <inheritdoc/>
        [ReflectionAudit("debugger state packing compiles a scalar's write through its fixed representation")]
        internal override Expr WriteExpression(Expr output, Expr tables, Expr value)
        {
            if (Type == typeof(DateTime))
            {
                return Expr.Call(output, WriterOverload(typeof(long)), Expr.Call(value,
                    typeof(DateTime).GetMethod(nameof(DateTime.ToBinary), Type.EmptyTypes) ?? throw new MissingMethodException(nameof(DateTime), nameof(DateTime.ToBinary))));
            }
            if (Type == typeof(TimeSpan))
                return Expr.Call(output, WriterOverload(typeof(long)), Expr.Property(value, nameof(TimeSpan.Ticks)));
            if (Type == typeof(Guid))
            {
                return Expr.Call(output, WriterOverload(typeof(byte[])), Expr.Call(value,
                    typeof(Guid).GetMethod(nameof(Guid.ToByteArray), Type.EmptyTypes) ?? throw new MissingMethodException(nameof(Guid), nameof(Guid.ToByteArray))));
            }
            return Expr.Call(output, WriterOverload(Type), value);
        }

        /// <inheritdoc/>
        internal override object Read(BinaryReader values, object[][] tables) => DebuggerPrimitives.Read(values, Type);
    }

    /// <summary>An enum, written as its member's index in the run's table of the enum, which names each member once.</summary>
    /// <param name="type">The enum.</param>
    /// <param name="names">The enum's member names.</param>
    /// <param name="slot">The table the run keeps of the enum's members.</param>
    private sealed class EnumNode(Type type, DebuggerEnumNames names, int slot) : Node(type)
    {
        /// <inheritdoc/>
        internal override NodeKind Kind => NodeKind.Enum;

        /// <inheritdoc/>
        internal override void Write(BinaryWriter writer, EnumTables tables, object? value) =>
            writer.Write7BitEncodedInt(tables.Index(slot, names.BitsOf(Present(value))));

        /// <inheritdoc/>
        [ReflectionAudit("debugger state packing compiles an enum's write as its member's index in the run's table")]
        internal override Expr WriteExpression(Expr output, Expr tables, Expr value)
        {
            Expr bits = Expr.Convert(Expr.Convert(Expr.Convert(value, Enum.GetUnderlyingType(Type)), names.BitsType), typeof(ulong));
            MethodInfo index = typeof(EnumTables).GetMethod(nameof(EnumTables.Index), BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingMethodException(nameof(EnumTables), nameof(EnumTables.Index));
            MethodInfo write = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write7BitEncodedInt), [typeof(int)])
                ?? throw new MissingMethodException(nameof(BinaryWriter), nameof(BinaryWriter.Write7BitEncodedInt));
            return Expr.Call(output, write, Expr.Call(tables, index, Expr.Constant(slot), bits));
        }

        /// <inheritdoc/>
        internal override object Read(BinaryReader values, object[][] tables)
        {
            int index = values.Read7BitEncodedInt();
            object[] members = tables[slot];
            return (uint)index < (uint)members.Length
                ? members[index]
                : throw new InvalidDataException(
                    $"A packed {Type.FullName} names member {index} of a table of {members.Length}.");
        }
    }

    /// <summary>A nullable value, written as whether it holds a value, then its value when it does.</summary>
    /// <param name="type">The nullable type.</param>
    /// <param name="value">The layout of the value it holds.</param>
    private sealed class NullableNode(Type type, Node value) : Node(type)
    {
        /// <inheritdoc/>
        internal override NodeKind Kind => NodeKind.Nullable;

        /// <inheritdoc/>
        internal override void WriteSchema(BinaryWriter writer)
        {
            base.WriteSchema(writer);
            value.WriteSchema(writer);
        }

        /// <inheritdoc/>
        internal override void CheckSchema(SchemaSource source)
        {
            base.CheckSchema(source);
            value.CheckSchema(source);
        }

        /// <inheritdoc/>
        internal override void Write(BinaryWriter writer, EnumTables tables, object? held)
        {
            // A boxed nullable is null or its value.
            writer.Write(held is not null);
            if (held is not null)
                value.Write(writer, tables, held);
        }

        /// <inheritdoc/>
        [ReflectionAudit("debugger state packing compiles a nullable value's write as its presence, then its value")]
        internal override Expr WriteExpression(Expr output, Expr tables, Expr held) => Expr.Condition(
            Expr.Property(held, nameof(Nullable<int>.HasValue)),
            Expr.Block(
                Expr.Call(output, WriterOverload(typeof(bool)), Expr.Constant(true)),
                value.WriteExpression(output, tables, Expr.Property(held, nameof(Nullable<int>.Value)))),
            Expr.Call(output, WriterOverload(typeof(bool)), Expr.Constant(false)),
            typeof(void));

        /// <inheritdoc/>
        internal override object? Read(BinaryReader values, object[][] tables) =>
            values.ReadBoolean() ? value.Read(values, tables) : null;
    }

    /// <summary>A struct, written as each serialized field's value in the fields' serialized order.</summary>
    /// <param name="type">The struct.</param>
    /// <param name="fields">Its serialized fields, in order.</param>
    /// <param name="children">Each field's layout.</param>
    private sealed class StructNode(Type type, FieldInfo[] fields, Node[] children) : Node(type)
    {
        /// <inheritdoc/>
        internal override NodeKind Kind => NodeKind.Struct;

        /// <summary>Writes the field count, then each field's declaring type name, name and layout.</summary>
        internal override void WriteSchema(BinaryWriter writer)
        {
            base.WriteSchema(writer);
            writer.Write(fields.Length);
            for (int index = 0; index < fields.Length; index++)
            {
                writer.Write(DebuggerStateTypeIdentity.GetSerializedName(ReflectionBoundary.DeclaringType(fields[index])));
                writer.Write(fields[index].Name);
                children[index].WriteSchema(writer);
            }
        }

        /// <summary>Rejects a saved struct whose field count, any field's declaring type or name, or any field's layout differs.</summary>
        internal override void CheckSchema(SchemaSource source)
        {
            base.CheckSchema(source);
            int count = source.Reader.ReadInt32();
            if (count != fields.Length)
            {
                throw new InvalidDataException(
                    $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the saved packed {Type.FullName} has {count} fields; this build has {fields.Length}.");
            }
            for (int index = 0; index < fields.Length; index++)
            {
                string declaringName = source.ReadName();
                string fieldName = source.ReadName();
                Type declaring = ReflectionBoundary.DeclaringType(fields[index]);
                if (source.Resolve(declaringName) != declaring || fieldName != fields[index].Name)
                {
                    throw new InvalidDataException(
                        $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: the saved packed {Type.FullName} has field {declaringName}.{fieldName} " +
                        $"where this build has {declaring.FullName}.{fields[index].Name}.");
                }
                children[index].CheckSchema(source);
            }
        }

        /// <inheritdoc/>
        [ReflectionAudit("debugger state packing writes a struct element's serialized fields by reflection")]
        internal override void Write(BinaryWriter writer, EnumTables tables, object? value)
        {
            object owner = Present(value);
            for (int index = 0; index < fields.Length; index++)
                children[index].Write(writer, tables, fields[index].GetValue(owner));
        }

        /// <inheritdoc/>
        [ReflectionAudit("debugger state packing compiles a struct element's writer from its serialized fields")]
        internal override Expr WriteExpression(Expr output, Expr tables, Expr value)
        {
            if (fields.Length == 0)
                return Expr.Empty();
            var writes = new Expr[fields.Length];
            for (int index = 0; index < fields.Length; index++)
                writes[index] = children[index].WriteExpression(output, tables, Expr.Field(value, fields[index]));
            return Expr.Block(typeof(void), writes);
        }

        /// <summary>Rebuilds a boxed struct from its fields' saved values, as a framed struct is rebuilt, not by a constructor.</summary>
        [ReflectionAudit("debugger state restore: a packed struct element is rebuilt from its serialized fields, not by a constructor")]
        internal override object Read(BinaryReader values, object[][] tables)
        {
            object instance = RuntimeHelpers.GetUninitializedObject(Type);
            for (int index = 0; index < fields.Length; index++)
                fields[index].SetValue(instance, children[index].Read(values, tables));
            return instance;
        }
    }
}
