using SuperMetroid.Core;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Shared layout-checked binary serializer for the managed emulator graph used by debugger states.
/// It preserves private fields, reference identity, cycles, arrays, and internal delegates;
/// this is intentionally not a general interchange format. Build changes are permitted,
/// but fields and delegate identities must still resolve against the current layout.
/// </summary>
internal static class DebuggerObjectGraphSerializer
{
    /// <summary>Writes a debugger snapshot while preserving cycles and private field layout.</summary>
    public static void Serialize(Stream destination, object root)
    {
        Ensure.NotNull(destination);
        Ensure.NotNull(root);
        using var writer = new BinaryWriter(destination, System.Text.Encoding.UTF8, leaveOpen: true);
        new GraphWriter(writer).Write(root);
    }

    /// <summary>The message prefix of every rejection of a state this build cannot restore exactly.</summary>
    internal const string UnsupportedVersion = "Unsupported debugger state version";

    /// <summary>Restores a snapshot only when its root and serialized field identities match this build.</summary>
    public static T Deserialize<T>(Stream source) where T : class
    {
        Ensure.NotNull(source);
        using var reader = new BinaryReader(source, System.Text.Encoding.UTF8, leaveOpen: true);
        object? root = new GraphReader(reader).Read();
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
    }

    /// <summary>Serializes one graph with reference IDs and strict type/member identities.</summary>
    private sealed class GraphWriter(BinaryWriter writer)
    {
        /// <summary>Preserves identity for shared objects and cycles.</summary>
        private readonly Dictionary<object, int> references =
            new(ReferenceEqualityComparer.Instance);
        /// <summary>Next positive ID reserved for a reference object.</summary>
        private int nextReferenceId = 1;

        /// <summary>Emits a marker, type identity, and payload for a new or previously seen value.</summary>
        public void Write(object? value)
        {
            if (value is null)
            {
                writer.Write((byte)ObjectMarker.Null);
                return;
            }

            Type type = value.GetType();
            bool tracksReference = !type.IsValueType;
            if (tracksReference && references.TryGetValue(value, out int existingId))
            {
                writer.Write((byte)ObjectMarker.Reference);
                writer.Write(existingId);
                return;
            }

            writer.Write((byte)ObjectMarker.New);
            int referenceId = tracksReference ? nextReferenceId++ : 0;
            writer.Write(referenceId);
            writer.Write(DebuggerStateTypeIdentity.GetSerializedName(type));
            if (tracksReference)
                references.Add(value, referenceId);

            if (TryWritePrimitive(type, value))
                return;
            if (type == typeof(string))
            {
                writer.Write((byte)PayloadKind.String);
                writer.Write((string)value);
                return;
            }
            if (type.IsArray)
            {
                WriteArray((Array)value, type);
                return;
            }
            if (value is Delegate callback)
            {
                WriteDelegate(callback);
                return;
            }

            if (InlineArrayElements.Length(type) is not null)
            {
                object?[] elements = InlineArrayElements.Read(value);
                writer.Write((byte)PayloadKind.InlineArray);
                writer.Write(elements.Length);
                foreach (object? element in elements)
                    Write(element);
                return;
            }

            writer.Write((byte)PayloadKind.Fields);
            FieldInfo[] fields = GetSerializableFields(type);
            writer.Write(fields.Length);
            foreach (FieldInfo field in fields)
            {
                writer.Write(DebuggerStateTypeIdentity.GetSerializedName(ReflectionBoundary.DeclaringType(field)));
                writer.Write(field.Name);
                Write(field.GetValue(value));
            }
        }

        /// <summary>Encodes scalars and enums through their fixed primitive representation.</summary>
        private bool TryWritePrimitive(Type type, object value)
        {
            Type primitiveType = type.IsEnum ? Enum.GetUnderlyingType(type) : type;
            if (!primitiveType.IsPrimitive && primitiveType != typeof(decimal) &&
                primitiveType != typeof(DateTime) && primitiveType != typeof(TimeSpan) &&
                primitiveType != typeof(Guid))
            {
                return false;
            }

            writer.Write((byte)PayloadKind.Primitive);
            if (type.IsEnum)
                value = Convert.ChangeType(value, primitiveType, System.Globalization.CultureInfo.InvariantCulture);
            WritePrimitiveValue(primitiveType, value);
            return true;
        }

        /// <summary>Writes each supported primitive with a fixed binary width or representation.</summary>
        private void WritePrimitiveValue(Type type, object value)
        {
            if (type == typeof(bool)) writer.Write((bool)value);
            else if (type == typeof(byte)) writer.Write((byte)value);
            else if (type == typeof(sbyte)) writer.Write((sbyte)value);
            else if (type == typeof(short)) writer.Write((short)value);
            else if (type == typeof(ushort)) writer.Write((ushort)value);
            else if (type == typeof(int)) writer.Write((int)value);
            else if (type == typeof(uint)) writer.Write((uint)value);
            else if (type == typeof(long)) writer.Write((long)value);
            else if (type == typeof(ulong)) writer.Write((ulong)value);
            else if (type == typeof(char)) writer.Write((char)value);
            else if (type == typeof(float)) writer.Write((float)value);
            else if (type == typeof(double)) writer.Write((double)value);
            else if (type == typeof(decimal)) writer.Write((decimal)value);
            else if (type == typeof(DateTime)) writer.Write(((DateTime)value).ToBinary());
            else if (type == typeof(TimeSpan)) writer.Write(((TimeSpan)value).Ticks);
            else if (type == typeof(Guid)) writer.Write(((Guid)value).ToByteArray());
            else throw new NotSupportedException($"Debugger state primitive {type.FullName} is unsupported.");
        }

        /// <summary>Writes raw primitive storage or recursively encodes each array element.</summary>
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

            writer.Write((byte)PayloadKind.Array);
            WriteArrayShape(array);
            if (IsVector(array))
                for (int index = 0; index < array.Length; index++) Write(array.GetValue(index));
            else
                foreach (int[] indices in EnumerateArrayIndices(array))
                    Write(array.GetValue(indices));
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

    /// <summary>Restores objects against this build's allowed types and exact field layout.</summary>
    private sealed class GraphReader(BinaryReader reader)
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
            if (marker != ObjectMarker.New)
                throw new InvalidDataException($"Unknown debugger-state object marker {(byte)marker}.");

            int referenceId = reader.ReadInt32();
            Type type = ResolveAllowedType(ReadName());
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
                _ => throw new InvalidDataException(
                    $"Unknown debugger-state payload kind {(byte)kind} for {type.FullName}."),
            };
        }

        /// <summary>Decodes a fixed scalar and validates enum values in the current context.</summary>
        private object ReadPrimitive(Type serializedType)
        {
            Type type = serializedType.IsEnum ? Enum.GetUnderlyingType(serializedType) : serializedType;
            object value;
            if (type == typeof(bool)) value = reader.ReadBoolean();
            else if (type == typeof(byte)) value = reader.ReadByte();
            else if (type == typeof(sbyte)) value = reader.ReadSByte();
            else if (type == typeof(short)) value = reader.ReadInt16();
            else if (type == typeof(ushort)) value = reader.ReadUInt16();
            else if (type == typeof(int)) value = reader.ReadInt32();
            else if (type == typeof(uint)) value = reader.ReadUInt32();
            else if (type == typeof(long)) value = reader.ReadInt64();
            else if (type == typeof(ulong)) value = reader.ReadUInt64();
            else if (type == typeof(char)) value = reader.ReadChar();
            else if (type == typeof(float)) value = reader.ReadSingle();
            else if (type == typeof(double)) value = reader.ReadDouble();
            else if (type == typeof(decimal)) value = reader.ReadDecimal();
            else if (type == typeof(DateTime)) value = DateTime.FromBinary(reader.ReadInt64());
            else if (type == typeof(TimeSpan)) value = TimeSpan.FromTicks(reader.ReadInt64());
            else if (type == typeof(Guid)) value = new Guid(reader.ReadBytes(16));
            else throw new NotSupportedException($"Debugger state primitive {type.FullName} is unsupported.");
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
            // Fill the array's own memory directly; no intermediate copy of large VRAM/WRAM images.
            while (!bytes.IsEmpty)
            {
                int read = reader.Read(bytes);
                if (read == 0)
                    throw new EndOfStreamException("Debugger state ended inside a primitive array.");
                bytes = bytes[read..];
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
                Type declaringType = ResolveAllowedType(ReadName());
                MethodInfo method = DebuggerDelegateIdentity.Read(reader, declaringType, ResolveAllowedType);
                object? target = Read();
                Delegate call = method.CreateDelegate(type, target);
                combined = combined is null ? call : Delegate.Combine(combined, call);
            }
            return Register(referenceId, combined ??
                throw new InvalidDataException($"{UnsupportedVersion}: a saved delegate has no invocation entries."));
        }

        /// <summary>Restores each field only after matching its declaring type and name to this build.</summary>
        [ReflectionAudit("debugger state restore: an object is rebuilt from its serialized fields, not by a constructor")]
        private object ReadFields(Type type, int referenceId)
        {
            object instance = RuntimeHelpers.GetUninitializedObject(type);
            if (!type.IsValueType)
                Register(referenceId, instance);
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
                if (ResolveAllowedType(declaringName) != field.DeclaringType || fieldName != field.Name)
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

    /// <summary>Collects inherited instance fields in stable metadata order, excluding transient event state.</summary>
    [ReflectionAudit("debugger state capture serializes every instance field of the object graph by name")]
    private static FieldInfo[] CollectSerializableFields(Type type) =>
        EnumerateHierarchy(type)
            .SelectMany(level => level.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly))
            .Where(field => !field.IsStatic && !field.IsDefined(typeof(NonSerializedAttribute), false) &&
                !(field.DeclaringType?.FullName == "SuperMetroid.Core.Frontend.SuperMetroidGame" &&
                    field.Name == "SaveRamChanged"))
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
