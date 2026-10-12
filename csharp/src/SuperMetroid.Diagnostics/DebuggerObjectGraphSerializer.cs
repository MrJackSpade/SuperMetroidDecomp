using SuperMetroid.Core;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Desktop;

/// <summary>
/// Shared layout-checked binary serializer for the managed emulator graph used by debugger states.
/// It preserves private fields, reference identity, cycles, arrays, and internal delegates;
/// this is intentionally not a general interchange format. Build changes are permitted,
/// but fields and delegate identities must still resolve against the current layout.
/// </summary>
internal static class DebuggerObjectGraphSerializer
{
    public static void Serialize(Stream destination, object root)
    {
        Ensure.NotNull(destination);
        Ensure.NotNull(root);
        using var writer = new BinaryWriter(destination, System.Text.Encoding.UTF8, leaveOpen: true);
        new GraphWriter(writer).Write(root);
    }

    /// <summary>The message prefix of every rejection of a state this build cannot restore exactly.</summary>
    internal const string UnsupportedVersion = "Unsupported debugger state version";

    public static T Deserialize<T>(Stream source) where T : class
    {
        Ensure.NotNull(source);
        using var reader = new BinaryReader(source, System.Text.Encoding.UTF8, leaveOpen: true);
        object? root = new GraphReader(reader).Read();
        return root as T ?? throw new InvalidDataException(
            $"Debugger state root is {root?.GetType().FullName ?? "null"}, expected {typeof(T).FullName}.");
    }

    private enum ObjectMarker : byte { Null, Reference, New }
    private enum PayloadKind : byte { Primitive, String, PrimitiveArray, Array, Delegate, Fields }

    private sealed class GraphWriter(BinaryWriter writer)
    {
        private readonly Dictionary<object, int> references =
            new(ReferenceEqualityComparer.Instance);
        private int nextReferenceId = 1;

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

            writer.Write((byte)PayloadKind.Fields);
            FieldInfo[] fields = GetSerializableFields(type);
            writer.Write(fields.Length);
            foreach (FieldInfo field in fields)
            {
                writer.Write(DebuggerStateTypeIdentity.GetSerializedName(field.DeclaringType!));
                writer.Write(field.Name);
                Write(field.GetValue(value));
            }
        }

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

        private void WriteArrayShape(Array array)
        {
            writer.Write(array.Rank);
            for (int dimension = 0; dimension < array.Rank; dimension++)
            {
                writer.Write(array.GetLength(dimension));
                writer.Write(array.GetLowerBound(dimension));
            }
        }

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

    private sealed class GraphReader(BinaryReader reader)
    {
        private readonly Dictionary<int, object> references = [];
        // Nesting depth inside struct elements of array storage (see ReadArray).
        private int structElementDepth;
        // Every object repeats its type and field names; decode each distinct name once.
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
                _ => throw new InvalidDataException(
                    $"Unknown debugger-state payload kind {(byte)kind} for {type.FullName}."),
            };
        }

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
            return Register(referenceId, combined!);
        }

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
                        $"where this build has {field.DeclaringType!.FullName}.{field.Name}.");
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
            if (instance is SuperMetroid.AssetExtraction.IRestoredSharedContent shared)
                shared.ReattachSharedContent();
            return instance;
        }

        private int ReadNonnegativeLength(string label)
        {
            int value = reader.ReadInt32();
            return value >= 0
                ? value
                : throw new InvalidDataException($"Debugger state has negative {label} length {value}.");
        }

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

    // A type's field layout is fixed for the process; graphs hold many objects of each type.
    // Callers treat the returned array as read-only (migrations build filtered copies).
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, FieldInfo[]> serializableFields = new();

    private static FieldInfo[] GetSerializableFields(Type type) =>
        serializableFields.GetOrAdd(type, CollectSerializableFields);

    private static FieldInfo[] CollectSerializableFields(Type type) =>
        EnumerateHierarchy(type)
            .SelectMany(level => level.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly))
            .Where(field => !field.IsStatic && !field.IsDefined(typeof(NonSerializedAttribute), false) &&
                !(field.DeclaringType?.FullName == "SuperMetroid.Core.Frontend.SuperMetroidGame" &&
                    field.Name == "SaveRamChanged"))
            .OrderBy(field => field.DeclaringType!.FullName, StringComparer.Ordinal)
            .ThenBy(field => field.MetadataToken)
            .ToArray();

    private static IEnumerable<Type> EnumerateHierarchy(Type type)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
            yield return current;
    }

    // Resolution of a name is fixed for the process, and graphs repeat the same few type
    // names for every object; only allowed results are cached, so rejections still throw.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Type> allowedTypes = new(StringComparer.Ordinal);

    private static Type ResolveAllowedType(string assemblyQualifiedName)
    {
        if (allowedTypes.TryGetValue(assemblyQualifiedName, out Type? cached)) return cached;
        Type allowed = ResolveAllowedTypeUncached(assemblyQualifiedName);
        allowedTypes.TryAdd(assemblyQualifiedName, allowed);
        return allowed;
    }

    private static Type ResolveAllowedTypeUncached(string assemblyQualifiedName)
    {
        Type type = DebuggerStateTypeIdentity.Resolve(assemblyQualifiedName)
            ?? throw new InvalidDataException(
                $"{UnsupportedVersion}: the state names type '{assemblyQualifiedName}', which this build does not have.");
        string assembly = type.Assembly.GetName().Name ?? string.Empty;
        if (assembly.StartsWith("SuperMetroid.", StringComparison.Ordinal) ||
            assembly is "System.Private.CoreLib" or "System.Collections" or
                "System.Collections.Concurrent" or "System.Linq")
        {
            return type;
        }
        throw new InvalidDataException(
            $"Debugger state type {type.FullName} comes from disallowed assembly {assembly}.");
    }
}

/// <summary>
/// Array element types written as their raw little-endian bytes: every primitive, plus the
/// 4-byte <see cref="SuperMetroid.Core.Assets.Rgba32"/> pixel, whose per-element object
/// encoding made one retained 256x224 display cost tens of megabytes.
/// </summary>
internal static class RawArrayElements
{
    static RawArrayElements()
    {
        if (System.Runtime.CompilerServices.Unsafe.SizeOf<SuperMetroid.Core.Assets.Rgba32>() != 4)
            throw new InvalidOperationException("Rgba32 must remain four packed bytes to be serialized raw.");
    }

    internal static bool IsRaw(Type elementType) =>
        elementType.IsPrimitive || elementType == typeof(SuperMetroid.Core.Assets.Rgba32);

    /// <summary>The array's element storage viewed as bytes, in memory order across every dimension.</summary>
    internal static Span<byte> Bytes(Array array)
    {
        Type elementType = array.GetType().GetElementType()!;
        int byteLength = elementType == typeof(SuperMetroid.Core.Assets.Rgba32)
            ? checked(array.Length * 4)
            : Buffer.ByteLength(array);
        return System.Runtime.InteropServices.MemoryMarshal.CreateSpan(
            ref System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(array), byteLength);
    }
}
