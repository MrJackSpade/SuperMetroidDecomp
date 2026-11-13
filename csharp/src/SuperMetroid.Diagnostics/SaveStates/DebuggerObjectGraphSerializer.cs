using SuperMetroid.Core;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Shared layout-checked binary serializer for the managed emulator graph used by debugger states.
/// It preserves private fields, reference identity, cycles, arrays, and internal delegates;
/// this is intentionally not a general interchange format. Build changes are permitted,
/// but fields and delegate identities must still resolve against the current layout. A collection keyed by hash is held
/// as its entries and rebuilt from them, never by its buckets (<see cref="HashKeyedCollection"/>, #1681). An array of
/// values with no identity is written as one packed run of its elements' values (<see cref="PackedElementLayout"/>, #1696).
/// </summary>
internal static class DebuggerObjectGraphSerializer
{
    /// <summary>
    /// Writes a debugger snapshot while preserving cycles and private field layout. An object <paramref name="bound"/> holds
    /// is written as its path among the host's bound objects, not as its contents (#1507), and an object the game's static
    /// members hold as its path among them (<see cref="DefinitionObjects"/>, #1623).
    /// </summary>
    public static void Serialize(Stream destination, object root, BoundObjectGraph? bound = null)
    {
        Ensure.NotNull(destination);
        Ensure.NotNull(root);
        using var writer = new BinaryWriter(destination, System.Text.Encoding.UTF8, leaveOpen: true);
        new GraphWriter(writer, bound).Write(root);
    }

    /// <summary>The message prefix of every rejection of a state this build cannot restore exactly.</summary>
    internal const string UnsupportedVersion = "Unsupported debugger state version";

    /// <summary>
    /// Restores a snapshot only when its root and serialized field identities match this build, resolving each bound
    /// object's path against <paramref name="bound"/> (#1507) and each static member's object's path to the object this
    /// build's member holds (#1623).
    /// </summary>
    public static T Deserialize<T>(Stream source, BoundObjectGraph? bound = null) where T : class
    {
        Ensure.NotNull(source);
        using var reader = new BinaryReader(source, System.Text.Encoding.UTF8, leaveOpen: true);
        object? root = new GraphReader(reader, bound).ReadState();
        return root as T ?? throw new InvalidDataException(
            $"Debugger state root is {root?.GetType().FullName ?? "null"}, expected {typeof(T).FullName}.");
    }

    /// <summary>Distinguishes absent values, previous references, and newly defined objects.</summary>
    private enum ObjectMarker : byte
    {
        /// <summary>No object or boxed value follows.</summary>
        Null,
        /// <summary>An earlier positive object ID follows.</summary>
        Reference,
        /// <summary>A new object ID, type identity, and payload follow.</summary>
        New,
        /// <summary>A new object ID and the path of an object the host bound into the session follow.</summary>
        Bound,
        /// <summary>A new object ID and the path of an object the game's static members hold follow (#1623).</summary>
        Definition,
    }
    /// <summary>Selects the decoder for a newly defined debugger graph value.</summary>
    private enum PayloadKind : byte
    {
        /// <summary>A scalar or enum encoded through its underlying primitive.</summary>
        Primitive,
        /// <summary>A UTF-8 string encoded by BinaryWriter.</summary>
        String,
        /// <summary>Raw element bytes after array shape metadata.</summary>
        PrimitiveArray,
        /// <summary>Recursively encoded elements after array shape metadata.</summary>
        Array,
        /// <summary>Invocation list with method identities and targets.</summary>
        Delegate,
        /// <summary>Ordered private and public instance fields.</summary>
        Fields,
        /// <summary>Every element of an inline-array struct, after their count.</summary>
        InlineArray,
        /// <summary>An enum member's name, or names joined for flags, so stored states do not depend on member numbering.</summary>
        EnumName,
        /// <summary>
        /// A hash-keyed collection's comparers, its entry count and its entries in enumeration order, which a restore adds
        /// to a new collection instead of restoring the captured bucket layout (#1681).
        /// </summary>
        HashEntries,
        /// <summary>
        /// The elements of an array of values with no identity, after array shape metadata, as one packed run: their schema
        /// once, their enums' member tables, then their values back to back (<see cref="PackedElementLayout"/>, #1696).
        /// </summary>
        PackedArray,
    }

    /// <summary>Serializes one graph with reference IDs and strict type/member identities.</summary>
    private sealed class GraphWriter(BinaryWriter writer, BoundObjectGraph? bound)
    {
        /// <summary>Preserves identity for shared objects and cycles.</summary>
        private readonly Dictionary<object, int> references =
            new(ReferenceEqualityComparer.Instance);
        /// <summary>Next positive ID reserved for a reference object.</summary>
        private int nextReferenceId = 1;

        /// <summary>
        /// Writes <paramref name="value"/>; a type the graph cannot hold fails naming each object on the way to it, innermost
        /// first, so the failure says where in the graph it lies.
        /// </summary>
        /// <exception cref="NotSupportedException">The graph holds a value of a type it cannot represent.</exception>
        public void Write(object? value)
        {
            try
            {
                WriteCore(value);
            }
            catch (NotSupportedException unsupported)
            {
                throw new NotSupportedException($"{unsupported.Message} <- {value?.GetType().FullName}", unsupported);
            }
        }

        /// <summary>Writes <paramref name="value"/>: its marker, then its reference, bound path or contents.</summary>
        private void WriteCore(object? value)
        {
            if (value is null)
            {
                writer.Write((byte)ObjectMarker.Null);
                return;
            }

            TypeLayout layout = Layout(value.GetType());
            if (layout.IsValueType)
            {
                // A value is never shared: its marker, reference ID zero and type name are one run of bytes per type.
                writer.Write(layout.ValuePrefix);
            }
            else
            {
                if (references.TryGetValue(value, out int existingId))
                {
                    writer.Write((byte)ObjectMarker.Reference);
                    writer.Write(existingId);
                    return;
                }

                if (bound is not null && bound.TryGetPath(value, out string? path))
                {
                    int boundId = nextReferenceId++;
                    references.Add(value, boundId);
                    writer.Write((byte)ObjectMarker.Bound);
                    writer.Write(boundId);
                    writer.Write(path);
                    return;
                }

                // A catalog's member or other object a static member holds is named by its path, so a restore holds the
                // catalog's own object instead of a copy (#1623). A string is written by its text: an interned literal
                // that a static member also holds is no identity.
                if (layout.Kind != LayoutKind.String && DefinitionObjects.TryGetPath(value, out string? definitionPath))
                {
                    int definitionId = nextReferenceId++;
                    references.Add(value, definitionId);
                    writer.Write((byte)ObjectMarker.Definition);
                    writer.Write(definitionId);
                    writer.Write(definitionPath);
                    return;
                }

                writer.Write((byte)ObjectMarker.New);
                int referenceId = nextReferenceId++;
                writer.Write(referenceId);
                writer.Write(layout.Name);
                references.Add(value, referenceId);
            }

            switch (layout.Kind)
            {
                case LayoutKind.Enum:
                    writer.Write((byte)PayloadKind.EnumName);
                    writer.Write(value.ToString() ?? throw new InvalidOperationException($"Enum {layout.Type.FullName} has no name."));
                    return;
                case LayoutKind.Primitive:
                    writer.Write((byte)PayloadKind.Primitive);
                    DebuggerPrimitives.Write(writer, layout.Type, value);
                    return;
                case LayoutKind.String:
                    writer.Write((byte)PayloadKind.String);
                    writer.Write((string)value);
                    return;
                case LayoutKind.Array:
                    WriteArray((Array)value, layout.Type);
                    return;
                case LayoutKind.Delegate:
                    WriteDelegate((Delegate)value);
                    return;
                case LayoutKind.InlineArray:
                    object?[] elements = InlineArrayElements.Read(value);
                    writer.Write((byte)PayloadKind.InlineArray);
                    writer.Write(elements.Length);
                    foreach (object? element in elements)
                        Write(element);
                    return;
                case LayoutKind.Fields:
                    writer.Write((byte)PayloadKind.Fields);
                    writer.Write(layout.Fields.Length);
                    WriteFields(layout, value);
                    return;
                case LayoutKind.HashKeyed:
                    WriteHashEntries(layout.HashKeyed ?? throw new InvalidOperationException(
                        $"Debugger state has no hash-keyed rebuild for {layout.Type.FullName}."), value);
                    return;
                default:
                    throw new InvalidOperationException($"Debugger state has no layout kind {layout.Kind} for {layout.Type.FullName}.");
            }
        }

        /// <summary>
        /// Writes each field of <paramref name="value"/>, its header then its value: through the type's compiled writer once
        /// the type has been written often enough to repay compiling it, by reflection before then.
        /// </summary>
        private void WriteFields(TypeLayout layout, object value)
        {
            if (layout.FieldWriter() is { } compiled)
            {
                compiled(this, writer, value);
                return;
            }
            FieldInfo[] fields = layout.Fields;
            byte[][] headers = layout.Headers;
            for (int index = 0; index < fields.Length; index++)
            {
                // The field's declaring type and name, as two strings written the way the reader reads them.
                writer.Write(headers[index]);
                Write(fields[index].GetValue(value));
            }
        }

        /// <summary>
        /// Writes a hash-keyed collection as its comparers, its entry count and its entries in enumeration order (#1681); a
        /// kind with no rebuild is refused here.
        /// </summary>
        private void WriteHashEntries(HashKeyedCollection kind, object collection)
        {
            object?[] comparers = kind.Comparers(collection);
            object?[] entries = kind.Entries(collection);
            writer.Write((byte)PayloadKind.HashEntries);
            foreach (object? comparer in comparers)
                Write(comparer);
            writer.Write(entries.Length / kind.EntryWidth);
            foreach (object? entry in entries)
                Write(entry);
        }

        /// <summary>
        /// Writes raw primitive storage, a packed run of values with no identity (#1696), or each element framed: an array
        /// whose element type can hold a reference or identity is written element by element.
        /// </summary>
        private void WriteArray(Array array, Type type)
        {
            Type elementType = type.GetElementType()
                ?? throw new InvalidOperationException($"Array {type.FullName} has no element type.");
            if (RawArrayElements.IsRaw(elementType))
            {
                writer.Write((byte)PayloadKind.PrimitiveArray);
                WriteArrayShape(array);
                Span<byte> bytes = RawArrayElements.Bytes(array);
                writer.Write(bytes.Length);
                writer.Write(bytes);
                return;
            }
            if (PackedElementLayout.For(elementType) is { } packed)
            {
                WritePackedArray(array, type, packed);
                return;
            }

            writer.Write((byte)PayloadKind.Array);
            WriteArrayShape(array);
            if (!IsVector(array))
            {
                foreach (int[] indices in EnumerateArrayIndices(array))
                    Write(array.GetValue(indices));
                return;
            }
            TypeLayout element = Layout(elementType);
            if (type.IsSZArray && element.ElementWriter(array.Length) is { } elements)
            {
                // Each struct element is written as Write writes the boxed element, without boxing it (#1532).
                for (int index = 0; index < array.Length; index++)
                    elements(this, writer, array, index);
                return;
            }
            for (int index = 0; index < array.Length; index++) Write(array.GetValue(index));
        }

        /// <summary>
        /// The values of the run being packed, gathered before they are written so the run's enum member tables, which its
        /// values index, can precede them; reused for every run of the capture.
        /// </summary>
        private MemoryStream? packedValues;

        /// <summary>The writer of <see cref="packedValues"/>.</summary>
        private BinaryWriter? packedWriter;

        /// <summary>
        /// Writes an array of values with no identity as one packed run (#1696): its shape, its element schema, each of its
        /// enums' member tables, then its values' byte count and the values back to back.
        /// </summary>
        private void WritePackedArray(Array array, Type type, PackedElementLayout packed)
        {
            writer.Write((byte)PayloadKind.PackedArray);
            WriteArrayShape(array);
            writer.Write(packed.Schema);
            MemoryStream values = packedValues ??= new MemoryStream();
            BinaryWriter output = packedWriter ??= new BinaryWriter(values, System.Text.Encoding.UTF8, leaveOpen: true);
            values.SetLength(0);
            PackedElementLayout.EnumTables tables = packed.NewTables();
            if (type.IsSZArray && packed.VectorWriter(array.Length) is { } elements)
            {
                for (int index = 0; index < array.Length; index++)
                    elements(output, tables, array, index);
            }
            else if (IsVector(array))
            {
                for (int index = 0; index < array.Length; index++)
                    packed.WriteElement(output, tables, array.GetValue(index));
            }
            else
            {
                foreach (int[] indices in EnumerateArrayIndices(array))
                    packed.WriteElement(output, tables, array.GetValue(indices));
            }
            output.Flush();
            packed.WriteTables(writer, tables);
            int length = checked((int)values.Length);
            writer.Write(length);
            writer.Write(values.GetBuffer(), 0, length);
        }

        /// <summary>Records rank, lengths, and lower bounds for exact array reconstruction.</summary>
        private void WriteArrayShape(Array array)
        {
            writer.Write(array.Rank);
            for (int dimension = 0; dimension < array.Rank; dimension++)
            {
                writer.Write(array.GetLength(dimension));
                writer.Write(array.GetLowerBound(dimension));
            }
        }

        /// <summary>Stores each invocation method identity and its graph-encoded target.</summary>
        private void WriteDelegate(Delegate callback)
        {
            writer.Write((byte)PayloadKind.Delegate);
            Delegate[] calls = callback.GetInvocationList();
            writer.Write(calls.Length);
            foreach (Delegate call in calls)
            {
                MethodInfo method = call.Method;
                writer.Write(DebuggerStateTypeIdentity.GetSerializedName(method.DeclaringType ??
                    throw new InvalidOperationException($"Delegate method {method.Name} has no declaring type.")));
                DebuggerDelegateIdentity.Write(writer, method);
                Write(call.Target);
            }
        }
    }

    /// <summary>How <see cref="GraphWriter.Write"/> encodes a value of one type, in the order it tests for each.</summary>
    private enum LayoutKind : byte
    {
        /// <summary>An enum, written by member name.</summary>
        Enum,
        /// <summary>A scalar written through its fixed primitive representation.</summary>
        Primitive,
        /// <summary>A string.</summary>
        String,
        /// <summary>An array.</summary>
        Array,
        /// <summary>A delegate.</summary>
        Delegate,
        /// <summary>An inline-array struct.</summary>
        InlineArray,
        /// <summary>An object or struct written field by field.</summary>
        Fields,
        /// <summary>A collection keyed by hash, written as its comparers and entries (#1681).</summary>
        HashKeyed,
    }

    /// <summary>Each type's serialized layout, worked out once.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, TypeLayout> layouts = new();

    /// <summary>The serialized layout of <paramref name="type"/>.</summary>
    private static TypeLayout Layout(Type type) => layouts.GetOrAdd(type, static owner => new TypeLayout(owner));

    /// <summary>
    /// What a capture writes for every value of one type, worked out once (#1532): how it is encoded, its encoded name, a
    /// value type's marker run, and a written-field type's fields and their headers. A type written many times compiles a
    /// writer of its fields that writes each primitive and enum field without boxing it.
    /// </summary>
    private sealed class TypeLayout
    {
        /// <summary>How many reflective writes of a type's fields repay compiling its field writer.</summary>
        private const int CompileAfterWrites = 64;

        /// <summary>How many elements of a struct array repay compiling its element writer at once.</summary>
        private const int CompileForElements = 64;

        /// <summary>Works out the layout of <paramref name="type"/>.</summary>
        internal TypeLayout(Type type)
        {
            Type = type;
            IsValueType = type.IsValueType;
            Name = EncodedTypeName(type);
            ValuePrefix = IsValueType ? Encode(writer =>
            {
                writer.Write((byte)ObjectMarker.New);
                writer.Write(0);
                writer.Write(Name);
            }) : [];
            HashKeyed = HashKeyedCollection.For(type);
            Kind = HashKeyed is not null ? LayoutKind.HashKeyed : KindOf(type);
            EnumPrefix = Kind == LayoutKind.Enum ? [.. ValuePrefix, (byte)PayloadKind.EnumName] : [];
            Fields = Kind == LayoutKind.Fields ? GetSerializableFields(type) : [];
            Headers = Kind == LayoutKind.Fields ? EncodedFieldHeaders(type) : [];
            // A pointer field's value is read only through reflection, which boxes it as a pointer object.
            compilable = Kind == LayoutKind.Fields &&
                Fields.All(field => !field.FieldType.IsPointer && !field.FieldType.IsFunctionPointer && !field.FieldType.IsByRef);
        }

        /// <summary>Whether a writer of the type's fields can be compiled: it writes fields and none holds a pointer.</summary>
        private readonly bool compilable;

        /// <summary>The type.</summary>
        internal Type Type { get; }

        /// <summary>Whether values of the type are values, which a graph never shares.</summary>
        internal bool IsValueType { get; }

        /// <summary>The type's serialized name as <see cref="BinaryWriter.Write(string)"/> writes it.</summary>
        internal byte[] Name { get; }

        /// <summary>For a value type, the bytes every value of it starts with: its marker, reference ID zero and name.</summary>
        internal byte[] ValuePrefix { get; }

        /// <summary>How values of the type are encoded.</summary>
        internal LayoutKind Kind { get; }

        /// <summary>For a collection keyed by hash, how it is written as its entries and rebuilt from them (#1681).</summary>
        internal HashKeyedCollection? HashKeyed { get; }

        /// <summary>For an enum, the bytes every member starts with: its value prefix and the enum-name payload kind.</summary>
        internal byte[] EnumPrefix { get; }

        /// <summary>For a written-field type, its serialized fields in order.</summary>
        internal FieldInfo[] Fields { get; }

        /// <summary>For a written-field type, each field's encoded header.</summary>
        internal byte[][] Headers { get; }

        /// <summary>How many times the type's fields were written by reflection.</summary>
        private int reflectiveWrites;

        /// <summary>The compiled writer of the type's fields, once compiled.</summary>
        private Action<GraphWriter, BinaryWriter, object>? fieldWriter;

        /// <summary>The compiled writer of an element of a vector of the type, once compiled.</summary>
        private Action<GraphWriter, BinaryWriter, Array, int>? elementWriter;

        /// <summary>
        /// The compiled writer of the type's fields, compiled once the type's fields have been written by reflection often
        /// enough to repay it; null before then.
        /// </summary>
        internal Action<GraphWriter, BinaryWriter, object>? FieldWriter()
        {
            if (Volatile.Read(ref fieldWriter) is { } compiled)
                return compiled;
            if (!compilable || Interlocked.Increment(ref reflectiveWrites) < CompileAfterWrites)
                return null;
            compiled = CompiledWriters.Fields(this);
            Volatile.Write(ref fieldWriter, compiled);
            return compiled;
        }

        /// <summary>
        /// For a written-field value type, the compiled writer of an element of a vector of it, compiled once an array of
        /// <paramref name="length"/> elements or the type's own writes repay it; null otherwise.
        /// </summary>
        internal Action<GraphWriter, BinaryWriter, Array, int>? ElementWriter(int length)
        {
            if (!compilable || !IsValueType || Nullable.GetUnderlyingType(Type) is not null)
                return null;
            if (Volatile.Read(ref elementWriter) is { } compiled)
                return compiled;
            if (length < CompileForElements && Interlocked.Add(ref reflectiveWrites, length) < CompileAfterWrites)
                return null;
            compiled = CompiledWriters.Element(this);
            Volatile.Write(ref elementWriter, compiled);
            return compiled;
        }

        /// <summary>How <see cref="GraphWriter.Write"/> encodes a value of <paramref name="type"/>, tested in its order.</summary>
        private static LayoutKind KindOf(Type type)
        {
            if (type.IsEnum)
                return LayoutKind.Enum;
            if (type.IsPrimitive || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(Guid))
                return LayoutKind.Primitive;
            if (type == typeof(string))
                return LayoutKind.String;
            if (type.IsArray)
                return LayoutKind.Array;
            if (typeof(Delegate).IsAssignableFrom(type))
                return LayoutKind.Delegate;
            if (InlineArrayElements.Length(type) is not null)
                return LayoutKind.InlineArray;
            return LayoutKind.Fields;
        }
    }

    /// <summary>
    /// Compiled writers of a type's fields (#1532), writing exactly the bytes the reflective loop writes: each field's header,
    /// then its value as <see cref="GraphWriter.Write"/> writes it. A primitive or enum field, whose value is always of the
    /// field's own type, is written from the field without boxing it; every other field goes through
    /// <see cref="GraphWriter.Write"/>.
    /// </summary>
    private static class CompiledWriters
    {
        /// <summary>The primitive field types written straight from the field, each through the writer overload the reflective path uses.</summary>
        private static readonly HashSet<Type> DirectPrimitives =
        [
            typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
            typeof(long), typeof(ulong), typeof(char), typeof(float), typeof(double), typeof(decimal),
        ];

        /// <summary>A writer of the fields of a value of <paramref name="layout"/>'s type, after their count.</summary>
        [ReflectionAudit("debugger state capture compiles a type's field writer from its serialized field layout")]
        internal static Action<GraphWriter, BinaryWriter, object> Fields(TypeLayout layout)
        {
            var graph = System.Linq.Expressions.Expression.Parameter(typeof(GraphWriter), "graph");
            var output = System.Linq.Expressions.Expression.Parameter(typeof(BinaryWriter), "output");
            var value = System.Linq.Expressions.Expression.Parameter(typeof(object), "value");
            var owner = System.Linq.Expressions.Expression.Variable(layout.Type, "owner");
            var body = new List<System.Linq.Expressions.Expression>
            {
                System.Linq.Expressions.Expression.Assign(owner, layout.IsValueType
                    ? System.Linq.Expressions.Expression.Unbox(value, layout.Type)
                    : System.Linq.Expressions.Expression.Convert(value, layout.Type)),
            };
            AddFields(layout, owner, graph, output, body);
            body.Add(System.Linq.Expressions.Expression.Empty());
            return System.Linq.Expressions.Expression.Lambda<Action<GraphWriter, BinaryWriter, object>>(
                System.Linq.Expressions.Expression.Block([owner], body), graph, output, value).Compile();
        }

        /// <summary>
        /// A writer of element <c>index</c> of a vector of <paramref name="layout"/>'s value type, as
        /// <see cref="GraphWriter.Write"/> writes the boxed element: its value prefix, its field count, then its fields.
        /// </summary>
        [ReflectionAudit("debugger state capture compiles a struct array's element writer from its serialized field layout")]
        internal static Action<GraphWriter, BinaryWriter, Array, int> Element(TypeLayout layout)
        {
            var graph = System.Linq.Expressions.Expression.Parameter(typeof(GraphWriter), "graph");
            var output = System.Linq.Expressions.Expression.Parameter(typeof(BinaryWriter), "output");
            var array = System.Linq.Expressions.Expression.Parameter(typeof(Array), "array");
            var index = System.Linq.Expressions.Expression.Parameter(typeof(int), "index");
            var owner = System.Linq.Expressions.Expression.Variable(layout.Type, "owner");
            byte[] start = Encode(writer =>
            {
                writer.Write(layout.ValuePrefix);
                writer.Write((byte)PayloadKind.Fields);
                writer.Write(layout.Fields.Length);
            });
            var body = new List<System.Linq.Expressions.Expression>
            {
                System.Linq.Expressions.Expression.Assign(owner, System.Linq.Expressions.Expression.ArrayIndex(
                    System.Linq.Expressions.Expression.Convert(array, layout.Type.MakeArrayType()), index)),
                WriteBytes(output, start),
            };
            AddFields(layout, owner, graph, output, body);
            body.Add(System.Linq.Expressions.Expression.Empty());
            return System.Linq.Expressions.Expression.Lambda<Action<GraphWriter, BinaryWriter, Array, int>>(
                System.Linq.Expressions.Expression.Block([owner], body), graph, output, array, index).Compile();
        }

        /// <summary>Adds the write of each of <paramref name="layout"/>'s fields of <paramref name="owner"/>, header then value, to <paramref name="body"/>.</summary>
        [ReflectionAudit("debugger state capture compiles a type's field writer from its serialized field layout")]
        private static void AddFields(TypeLayout layout, System.Linq.Expressions.Expression owner,
            System.Linq.Expressions.ParameterExpression graph, System.Linq.Expressions.ParameterExpression output,
            List<System.Linq.Expressions.Expression> body)
        {
            MethodInfo writeValue = typeof(GraphWriter).GetMethod(nameof(GraphWriter.Write), [typeof(object)])
                ?? throw new InvalidOperationException("The graph writer's value write is missing.");
            MethodInfo encodedName = typeof(DebuggerEnumNames).GetMethod(nameof(DebuggerEnumNames.Of), BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("The enum names' encoded name is missing.");
            for (int field = 0; field < layout.Fields.Length; field++)
            {
                FieldInfo info = layout.Fields[field];
                Type fieldType = info.FieldType;
                System.Linq.Expressions.Expression read = System.Linq.Expressions.Expression.Field(owner, info);
                if (DirectPrimitives.Contains(fieldType))
                {
                    body.Add(WriteBytes(output, [.. layout.Headers[field], .. Layout(fieldType).ValuePrefix, (byte)PayloadKind.Primitive]));
                    body.Add(System.Linq.Expressions.Expression.Call(output, WriterOverload(fieldType), read));
                    continue;
                }
                if (fieldType.IsEnum && DebuggerEnumNames.For(fieldType) is { } names)
                {
                    body.Add(WriteBytes(output, [.. layout.Headers[field], .. Layout(fieldType).EnumPrefix]));
                    System.Linq.Expressions.Expression bits = System.Linq.Expressions.Expression.Convert(
                        System.Linq.Expressions.Expression.Convert(
                            System.Linq.Expressions.Expression.Convert(read, Enum.GetUnderlyingType(fieldType)), names.BitsType),
                        typeof(ulong));
                    body.Add(System.Linq.Expressions.Expression.Call(output, WriterOverload(typeof(byte[])),
                        System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Constant(names), encodedName, bits)));
                    continue;
                }
                body.Add(WriteBytes(output, layout.Headers[field]));
                body.Add(System.Linq.Expressions.Expression.Call(graph, writeValue,
                    System.Linq.Expressions.Expression.Convert(read, typeof(object))));
            }
        }

        /// <summary>The write of the constant bytes <paramref name="bytes"/> to <paramref name="output"/>.</summary>
        private static System.Linq.Expressions.MethodCallExpression WriteBytes(System.Linq.Expressions.ParameterExpression output, byte[] bytes) =>
            System.Linq.Expressions.Expression.Call(output, WriterOverload(typeof(byte[])), System.Linq.Expressions.Expression.Constant(bytes));

        /// <summary>The <see cref="BinaryWriter"/> overload that writes one <paramref name="type"/>.</summary>
        private static MethodInfo WriterOverload(Type type) => typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), [type])
            ?? throw new InvalidOperationException($"BinaryWriter has no write of {type.FullName}.");
    }

    /// <summary>The bytes <paramref name="write"/> writes through a UTF-8 <see cref="BinaryWriter"/>.</summary>
    private static byte[] Encode(Action<BinaryWriter> write)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            write(writer);
        return stream.ToArray();
    }

    /// <summary>Restores objects against this build's allowed types and exact field layout.</summary>
    private sealed class GraphReader(BinaryReader reader, BoundObjectGraph? bound)
    {
        /// <summary>Objects registered before child fields are decoded so cycles can close.</summary>
        private readonly Dictionary<int, object> references = [];
        /// <summary>Depth in struct array storage, where zero enum fields can be unused capacity.</summary>
        private int structElementDepth;
        /// <summary>Interns repeated serialized type and field names during one restore.</summary>
        private readonly Dictionary<string, string> names = new(StringComparer.Ordinal);

        /// <summary>
        /// Reads a <see cref="BinaryWriter.Write(string)"/> type or member name, returning the
        /// restore's single instance of that text instead of a new string per occurrence.
        /// </summary>
        private string ReadName()
        {
            int byteCount = reader.Read7BitEncodedInt();
            if (byteCount < 0)
                throw new InvalidDataException("Debugger state has a negative name length.");
            byte[]? rented = byteCount > 256 ? System.Buffers.ArrayPool<byte>.Shared.Rent(byteCount) : null;
            try
            {
                Span<byte> utf8 = rented is null ? stackalloc byte[byteCount] : rented.AsSpan(0, byteCount);
                reader.BaseStream.ReadExactly(utf8);
                int charCount = System.Text.Encoding.UTF8.GetCharCount(utf8);
                char[]? rentedChars = charCount > 256 ? System.Buffers.ArrayPool<char>.Shared.Rent(charCount) : null;
                try
                {
                    Span<char> text = rentedChars is null ? stackalloc char[charCount] : rentedChars.AsSpan(0, charCount);
                    System.Text.Encoding.UTF8.GetChars(utf8, text);
                    var lookup = names.GetAlternateLookup<ReadOnlySpan<char>>();
                    if (lookup.TryGetValue(text, out string? known))
                        return known;
                    string name = new(text);
                    names.Add(name, name);
                    return name;
                }
                finally
                {
                    if (rentedChars is not null) System.Buffers.ArrayPool<char>.Shared.Return(rentedChars);
                }
            }
            finally
            {
                if (rented is not null) System.Buffers.ArrayPool<byte>.Shared.Return(rented);
            }
        }

        /// <summary>
        /// The hash-keyed collections the restore made empty, each with its saved entries, filled in the order their
        /// entries finished reading once the whole state is read (#1681).
        /// </summary>
        private readonly List<(HashKeyedCollection.Mutable Kind, object Collection, object?[] Entries)> unfilled = [];

        /// <summary>The objects whose fields are still being read, which a collection built at once cannot hash yet.</summary>
        private readonly HashSet<object> restoring = new(ReferenceEqualityComparer.Instance);

        /// <summary>
        /// Decodes a whole state, then fills its hash-keyed collections from their saved entries, every key's fields restored
        /// by then, so each key lands where this process hashes it (#1681).
        /// </summary>
        public object? ReadState()
        {
            object? root = Read();
            foreach ((HashKeyedCollection.Mutable kind, object collection, object?[] entries) in unfilled)
                kind.Fill(collection, entries);
            unfilled.Clear();
            return root;
        }

        /// <summary>Decodes one graph value, resolving references and rejecting unknown payload kinds.</summary>
        public object? Read()
        {
            ObjectMarker marker = (ObjectMarker)reader.ReadByte();
            if (marker == ObjectMarker.Null)
                return null;
            if (marker == ObjectMarker.Reference)
            {
                int id = reader.ReadInt32();
                return references.TryGetValue(id, out object? existing)
                    ? existing
                    : throw new InvalidDataException(
                        $"Debugger state references object {id} before its definition.");
            }
            if (marker == ObjectMarker.Bound)
            {
                int boundId = reader.ReadInt32();
                string path = reader.ReadString();
                return Register(boundId, (bound ?? throw new InvalidDataException(
                    "Debugger state names a bound object, but no bound content was supplied to restore it.")).Resolve(path));
            }
            if (marker == ObjectMarker.Definition)
            {
                int definitionId = reader.ReadInt32();
                return Register(definitionId, DefinitionObjects.Resolve(reader.ReadString()));
            }
            if (marker != ObjectMarker.New)
                throw new InvalidDataException($"Unknown debugger-state object marker {(byte)marker}.");

            int referenceId = reader.ReadInt32();
            Type type = ResolveName(ReadName());
            PayloadKind kind = (PayloadKind)reader.ReadByte();
            return kind switch
            {
                PayloadKind.Primitive => ReadPrimitive(type),
                PayloadKind.String => Register(referenceId, reader.ReadString()),
                PayloadKind.PrimitiveArray => ReadPrimitiveArray(type, referenceId),
                PayloadKind.Array => ReadArray(type, referenceId),
                PayloadKind.Delegate => ReadDelegate(type, referenceId),
                PayloadKind.Fields => ReadFields(type, referenceId),
                PayloadKind.InlineArray => ReadInlineArray(type),
                PayloadKind.EnumName => ReadEnumName(type),
                PayloadKind.HashEntries => ReadHashEntries(type, referenceId),
                PayloadKind.PackedArray => ReadPackedArray(type, referenceId),
                _ => throw new InvalidDataException(
                    $"Unknown debugger-state payload kind {(byte)kind} for {type.FullName}."),
            };
        }

        /// <summary>Decodes an enum stored by member name and validates it in the current context.</summary>
        private object ReadEnumName(Type type)
        {
            if (!type.IsEnum)
                throw new InvalidDataException($"Debugger state stores an enum name for non-enum {type.FullName}.");
            return DebuggerEnumValues.DecodeName(type, reader.ReadString(), structElementDepth > 0);
        }

        /// <summary>
        /// Resolves a serialized type name this restore read through <see cref="ReadName"/>, whose single instance of each
        /// name keys the restore's own resolutions, so a repeated name is not hashed again (#1532).
        /// </summary>
        private Type ResolveName(string name)
        {
            if (resolvedNames.TryGetValue(name, out Type? known))
                return known;
            Type type = ResolveAllowedType(name);
            resolvedNames.Add(name, type);
            return type;
        }

        /// <summary>Each name instance <see cref="ReadName"/> returned and resolved, with its type.</summary>
        private readonly Dictionary<string, Type> resolvedNames = new(ReferenceEqualityComparer.Instance);

        /// <summary>Decodes a fixed scalar and validates enum values in the current context.</summary>
        private object ReadPrimitive(Type serializedType)
        {
            object value = DebuggerPrimitives.Read(reader, serializedType.IsEnum ? Enum.GetUnderlyingType(serializedType) : serializedType);
            return serializedType.IsEnum ? DebuggerEnumValues.Decode(serializedType, value, structElementDepth > 0) : value;
        }

        /// <summary>Fills raw array storage directly after validating its serialized byte count.</summary>
        private Array ReadPrimitiveArray(Type type, int referenceId)
        {
            Type elementType = type.GetElementType()
                ?? throw new InvalidDataException($"Serialized array {type.FullName} has no element type.");
            Array array = CreateArray(elementType);
            Register(referenceId, array);
            if (!RawArrayElements.IsRaw(elementType))
                throw new InvalidDataException($"Serialized raw array {type.FullName} has a non-raw element type.");
            int byteLength = ReadNonnegativeLength("primitive-array byte");
            Span<byte> bytes = RawArrayElements.Bytes(array);
            if (byteLength != bytes.Length)
            {
                throw new InvalidDataException(
                    $"Primitive array {type.FullName} declares {byteLength} bytes, " +
                    $"expected {bytes.Length}.");
            }
            // Fill the array's own memory directly; no intermediate copy of large arrays such as installed images.
            while (!bytes.IsEmpty)
            {
                int read = reader.Read(bytes);
                if (read == 0)
                    throw new EndOfStreamException("Debugger state ended inside a primitive array.");
                bytes = bytes[read..];
            }
            return array;
        }

        /// <summary>
        /// Restores an array of values with no identity from its packed run (#1696): checks the run's element schema against
        /// this build's layout, reads its enums' member tables, then fills each element from the run's values, which must
        /// be read exactly. Enum members inside struct elements may be zero storage slots, as for framed struct elements.
        /// </summary>
        private Array ReadPackedArray(Type type, int referenceId)
        {
            Type elementType = type.GetElementType()
                ?? throw new InvalidDataException($"Serialized array {type.FullName} has no element type.");
            Array array = CreateArray(elementType);
            Register(referenceId, array);
            PackedElementLayout packed = PackedElementLayout.For(elementType) ?? throw new InvalidDataException(
                $"{UnsupportedVersion}: the state packs the elements of {type.FullName}; this build writes each of them framed.");
            packed.CheckSchema(new PackedElementLayout.SchemaSource(reader, ReadName, ResolveName));
            object[][] tables = packed.ReadTables(reader, zeroStorageSlot: !elementType.IsEnum || structElementDepth > 0);
            int byteCount = ReadNonnegativeLength("packed element byte");
            byte[] bytes = reader.ReadBytes(byteCount);
            if (bytes.Length != byteCount)
                throw new EndOfStreamException($"Debugger state ended inside the packed elements of {type.FullName}.");
            using var values = new BinaryReader(new MemoryStream(bytes, writable: false), System.Text.Encoding.UTF8);
            if (IsVector(array))
                for (int index = 0; index < array.Length; index++) array.SetValue(packed.ReadElement(values, tables), index);
            else
                foreach (int[] indices in EnumerateArrayIndices(array))
                    array.SetValue(packed.ReadElement(values, tables), indices);
            if (values.BaseStream.Position != byteCount)
            {
                throw new InvalidDataException(
                    $"The packed elements of the saved {type.FullName} hold {byteCount} bytes; its {array.Length} elements read {values.BaseStream.Position}.");
            }
            return array;
        }

        /// <summary>Registers a shaped array before recursively reading elements, including struct storage.</summary>
        private Array ReadArray(Type type, int referenceId)
        {
            Type elementType = type.GetElementType()
                ?? throw new InvalidDataException($"Serialized array {type.FullName} has no element type.");
            Array array = CreateArray(elementType);
            Register(referenceId, array);
            // Struct elements of collection storage (List<T>._items past Count) are zero-filled
            // slots, not domain values; their zero enum fields are storage, not undefined members.
            bool structElements = elementType.IsValueType && !elementType.IsPrimitive && !elementType.IsEnum;
            if (structElements) structElementDepth++;
            if (IsVector(array))
                for (int index = 0; index < array.Length; index++) array.SetValue(Read(), index);
            else
                foreach (int[] indices in EnumerateArrayIndices(array))
                    array.SetValue(Read(), indices);
            if (structElements) structElementDepth--;
            return array;
        }

        /// <summary>Recreates rank, lengths, and lower bounds while rejecting invalid dimensions.</summary>
        private Array CreateArray(Type elementType)
        {
            int rank = ReadNonnegativeLength("array rank");
            if (rank is < 1 or > 32)
                throw new InvalidDataException($"Debugger state has invalid array rank {rank}.");
            var lengths = new int[rank];
            var lowerBounds = new int[rank];
            for (int dimension = 0; dimension < rank; dimension++)
            {
                lengths[dimension] = ReadNonnegativeLength("array dimension");
                lowerBounds[dimension] = reader.ReadInt32();
            }
            return Array.CreateInstance(elementType, lengths, lowerBounds);
        }

        /// <summary>Rebuilds the invocation list from allowed method identities and restored targets.</summary>
        private object ReadDelegate(Type type, int referenceId)
        {
            int count = ReadNonnegativeLength("delegate invocation");
            if (count == 0)
                throw new InvalidDataException("A serialized delegate has an empty invocation list.");
            Delegate? combined = null;
            for (int index = 0; index < count; index++)
            {
                Type declaringType = ResolveName(ReadName());
                MethodInfo method = DebuggerDelegateIdentity.Read(reader, declaringType, ResolveAllowedType);
                object? target = Read();
                Delegate call = method.CreateDelegate(type, target);
                combined = combined is null ? call : Delegate.Combine(combined, call);
            }
            return Register(referenceId, combined ??
                throw new InvalidDataException($"{UnsupportedVersion}: a saved delegate has no invocation entries."));
        }

        /// <summary>
        /// Rebuilds a hash-keyed collection from its saved comparers and entries (#1681): a collection that can change is
        /// made empty and registered at once, so objects inside it can hold it, and filled once the whole state is read; one
        /// that cannot is built as soon as its entries are read, from keys that must be whole by then.
        /// </summary>
        private object ReadHashEntries(Type type, int referenceId)
        {
            if (HashKeyedCollection.For(type) is not { } kind || kind is not (HashKeyedCollection.Mutable or HashKeyedCollection.Immutable))
            {
                throw new InvalidDataException(
                    $"{UnsupportedVersion}: the state holds {type.FullName} as hash entries; this build does not rebuild it from them.");
            }
            var comparers = new object?[kind.ComparerCount];
            for (int index = 0; index < comparers.Length; index++)
                comparers[index] = Read();
            int count = ReadNonnegativeLength("hash entry");
            var entries = new object?[checked(count * kind.EntryWidth)];
            if (kind is HashKeyedCollection.Mutable mutable)
            {
                object collection = Register(referenceId, mutable.Empty(comparers));
                for (int index = 0; index < entries.Length; index++)
                    entries[index] = Read();
                unfilled.Add((mutable, collection, entries));
                return collection;
            }
            for (int index = 0; index < entries.Length; index++)
            {
                entries[index] = Read();
                if (index % kind.EntryWidth == 0 && entries[index] is { } key && restoring.Contains(key))
                {
                    throw new InvalidDataException(
                        $"Debugger state cannot rebuild the saved {type.FullName}: its key {key.GetType().FullName} encloses it, " +
                        "so the key's fields are still being read when the collection must hash it.");
                }
            }
            return Register(referenceId, ((HashKeyedCollection.Immutable)kind).Build(comparers, entries));
        }

        /// <summary>Restores each field only after matching its declaring type and name to this build.</summary>
        [ReflectionAudit("debugger state restore: an object is rebuilt from its serialized fields, not by a constructor")]
        private object ReadFields(Type type, int referenceId)
        {
            // A hash-keyed collection is never restored by its layout, whose buckets are the capturing process's (#1681).
            if (HashKeyedCollection.For(type) is not null)
            {
                throw new InvalidDataException(
                    $"{UnsupportedVersion}: the state holds the hash-keyed {type.FullName} by its layout; this build rebuilds it from its entries.");
            }
            object instance = RuntimeHelpers.GetUninitializedObject(type);
            if (!type.IsValueType)
            {
                Register(referenceId, instance);
                restoring.Add(instance);
            }
            FieldInfo[] currentFields = GetSerializableFields(type);
            int count = ReadNonnegativeLength("field");
            if (count != currentFields.Length)
            {
                throw new InvalidDataException(
                    $"{UnsupportedVersion}: the saved {type.FullName} has {count} fields; this build has {currentFields.Length}.");
            }
            // Fields are written in a fixed order, so the saved layout matches field for field.
            foreach (FieldInfo field in currentFields)
            {
                string declaringName = ReadName();
                string fieldName = ReadName();
                if (ResolveName(declaringName) != field.DeclaringType || fieldName != field.Name)
                {
                    throw new InvalidDataException(
                        $"{UnsupportedVersion}: the saved {type.FullName} has field {declaringName}.{fieldName} " +
                        $"where this build has {ReflectionBoundary.DeclaringType(field).FullName}.{field.Name}.");
                }
                object? restoredValue;
                try
                {
                    restoredValue = Read();
                }
                catch (InvalidDataException exception)
                {
                    // Name the field path so an invalid nested value is diagnosable.
                    throw new InvalidDataException($"{declaringName}.{fieldName}: {exception.Message}", exception);
                }
                if (restoredValue is not null && !field.FieldType.IsInstanceOfType(restoredValue))
                {
                    throw new InvalidDataException(
                        $"{UnsupportedVersion}: the saved {type.FullName}.{field.Name} holds {restoredValue.GetType().FullName}; " +
                        $"this build stores {field.FieldType.FullName}.");
                }
                field.SetValue(instance, restoredValue);
            }
            if (!type.IsValueType)
                restoring.Remove(instance);
            if (instance is SuperMetroid.AssetExtraction.Installation.IRestoredSharedContent shared)
                shared.ReattachSharedContent();
            return instance;
        }

        /// <summary>Rebuilds an inline-array struct from its saved elements.</summary>
        private object ReadInlineArray(Type type)
        {
            var elements = new object?[ReadNonnegativeLength("inline-array element")];
            for (int index = 0; index < elements.Length; index++)
                elements[index] = Read();
            return InlineArrayElements.Create(type, elements);
        }

        /// <summary>Rejects negative serialized counts before allocating or iterating.</summary>
        private int ReadNonnegativeLength(string label)
        {
            int value = reader.ReadInt32();
            return value >= 0
                ? value
                : throw new InvalidDataException($"Debugger state has negative {label} length {value}.");
        }

        /// <summary>Publishes a unique positive ID so later references can resolve cycles.</summary>
        private object Register(int id, object value)
        {
            if (id <= 0 || !references.TryAdd(id, value))
                throw new InvalidDataException($"Debugger state has invalid or duplicate object ID {id}.");
            return value;
        }
    }

    /// <summary>Single-dimension, zero-based arrays index directly without an index tuple.</summary>
    private static bool IsVector(Array array) => array.Rank == 1 && array.GetLowerBound(0) == 0;

    /// <summary>
    /// Every index tuple in row-major order. The yielded buffer is reused: callers consume each
    /// tuple immediately (one element read or write) before advancing, and each array has its
    /// own enumerator, so nested arrays never share it.
    /// </summary>
    private static IEnumerable<int[]> EnumerateArrayIndices(Array array)
    {
        if (array.Length == 0)
            yield break;
        var indices = new int[array.Rank];
        for (int dimension = 0; dimension < array.Rank; dimension++)
            indices[dimension] = array.GetLowerBound(dimension);
        while (true)
        {
            yield return indices;
            int dimension = array.Rank - 1;
            while (dimension >= 0)
            {
                indices[dimension]++;
                if (indices[dimension] <= array.GetUpperBound(dimension))
                    break;
                indices[dimension] = array.GetLowerBound(dimension);
                dimension--;
            }
            if (dimension < 0)
                yield break;
        }
    }

    /// <summary>Cached reflection layouts reused across graph instances; callers treat arrays as read-only.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, FieldInfo[]> serializableFields = new();

    /// <summary>Returns the cached ordered instance fields for a concrete type.</summary>
    internal static FieldInfo[] GetSerializableFields(Type type) =>
        serializableFields.GetOrAdd(type, CollectSerializableFields);

    /// <summary>
    /// Each type's serialized name as <see cref="BinaryWriter.Write(string)"/> writes it, its UTF-8 length then its bytes,
    /// encoded once: a capture writes the name of every object it defines.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, byte[]> encodedTypeNames = new();

    /// <summary>
    /// Each type's serializable fields' headers, each field's declaring type name and then its name as
    /// <see cref="BinaryWriter.Write(string)"/> writes them, encoded once: a capture writes them before every field value.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, byte[][]> encodedFieldHeaders = new();

    /// <summary>The encoded serialized name of <paramref name="type"/>.</summary>
    private static byte[] EncodedTypeName(Type type) =>
        encodedTypeNames.GetOrAdd(type, static owner => Encoded(DebuggerStateTypeIdentity.GetSerializedName(owner)));

    /// <summary>The encoded headers of <paramref name="type"/>'s serializable fields, in their serialized order.</summary>
    private static byte[][] EncodedFieldHeaders(Type type) =>
        encodedFieldHeaders.GetOrAdd(type, static owner => [.. GetSerializableFields(owner).Select(field =>
            Encoded(DebuggerStateTypeIdentity.GetSerializedName(ReflectionBoundary.DeclaringType(field)), field.Name))]);

    /// <summary>The bytes a UTF-8 <see cref="BinaryWriter"/> writes for <paramref name="names"/>, one string after another.</summary>
    private static byte[] Encoded(params string[] names)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            foreach (string name in names)
                writer.Write(name);
        }
        return stream.ToArray();
    }

    /// <summary>Collects inherited instance fields in stable metadata order, excluding transient event state.</summary>
    [ReflectionAudit("debugger state capture serializes every instance field of the object graph by name")]
    private static FieldInfo[] CollectSerializableFields(Type type) =>
        EnumerateHierarchy(type)
            .SelectMany(level => level.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly))
            .Where(field => !field.IsStatic && !field.IsDefined(typeof(NonSerializedAttribute), false))
            .OrderBy(field => ReflectionBoundary.DeclaringType(field).FullName, StringComparer.Ordinal)
            .ThenBy(field => field.MetadataToken)
            .ToArray();

    /// <summary>Visits the concrete type followed by each base type for field discovery.</summary>
    private static IEnumerable<Type> EnumerateHierarchy(Type type)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
            yield return current;
    }

    /// <summary>Caches only successfully resolved allowed types; rejected names remain errors.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Type> allowedTypes = new(StringComparer.Ordinal);

    /// <summary>Resolves a repeated serialized type name from the allowed-type cache.</summary>
    private static Type ResolveAllowedType(string assemblyQualifiedName)
    {
        if (allowedTypes.TryGetValue(assemblyQualifiedName, out Type? cached)) return cached;
        Type allowed = ResolveAllowedTypeUncached(assemblyQualifiedName);
        allowedTypes.TryAdd(assemblyQualifiedName, allowed);
        return allowed;
    }

    /// <summary>Rejects missing types and assemblies outside the emulator and approved runtime set.</summary>
    private static Type ResolveAllowedTypeUncached(string assemblyQualifiedName)
    {
        Type type = DebuggerStateTypeIdentity.Resolve(assemblyQualifiedName)
            ?? throw new InvalidDataException(
                $"{UnsupportedVersion}: the state names type '{assemblyQualifiedName}', which this build does not have.");
        string assembly = type.Assembly.GetName().Name ?? string.Empty;
        if (assembly.StartsWith("SuperMetroid.", StringComparison.Ordinal) ||
            assembly is "System.Private.CoreLib" or "System.Collections" or
                "System.Collections.Concurrent" or "System.Collections.Immutable" or "System.Linq")
        {
            return type;
        }
        throw new InvalidDataException(
            $"Debugger state type {type.FullName} comes from disallowed assembly {assembly}.");
    }
}
