using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using SuperMetroid.Desktop;

/// <summary>
/// #628 replaced separately stored X/Y components with spatial values. A debugger state captured
/// before that change names the component fields; restoration must compose the identical value.
/// </summary>
internal static partial class Program
{
    private const BindingFlags SpatialMergeFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static void VerifyDebuggerSpatialFieldMerges()
    {
        Assembly core = typeof(SuperMetroid.Core.Spatial.Point32<>).Assembly;
        foreach (DebuggerSpatialFieldMerge merge in DebuggerSpatialFieldMerges.Merges)
        {
            Type type = core.GetType(merge.TypeName, throwOnError: true)!;
            FieldInfo field = SpatialMergeField(type, merge.Field);
            foreach (string legacy in merge.LegacyFields)
                if (TrySpatialMergeField(type, legacy) is not null)
                    throw new InvalidOperationException($"{merge.TypeName}.{legacy} still exists beside its merged {merge.Field}.");

            // Signed and high components prove exact word reinterpretation, not only small values.
            object[] components = merge.LegacyType.Select((componentType, index) => SpatialMergeSample(componentType, index)).ToArray();
            object expected = merge.Compose(components);
            if (expected.GetType() != field.FieldType)
                throw new InvalidOperationException($"{merge.TypeName}.{merge.Field} composes {expected.GetType()}, not {field.FieldType}.");

            object instance = RuntimeHelpers.GetUninitializedObject(type);
            // Restoration validates closed domains (#627); give enum fields a defined member.
            for (Type? owner = type; owner is not null; owner = owner.BaseType)
                foreach (FieldInfo member in owner.GetFields(SpatialMergeFields | BindingFlags.DeclaredOnly))
                    if (member.FieldType.IsEnum && !Enum.IsDefined(member.FieldType, member.GetValue(instance)!))
                        member.SetValue(instance, Enum.GetValues(member.FieldType).GetValue(0));
            field.SetValue(instance, expected);
            byte[] current = SpatialMergeSerialize(instance);
            byte[] legacyLayout = SpatialMergeLegacyLayout(current, field, expected, merge, components);
            object restored = SpatialMergeDeserialize(legacyLayout, type);
            object? actual = field.GetValue(restored);
            if (!Equals(actual, expected))
                throw new InvalidOperationException($"{merge.TypeName}.{merge.Field} restored {actual}, expected {expected}.");
            if (!SpatialMergeSerialize(restored).AsSpan().SequenceEqual(current))
                throw new InvalidOperationException($"{merge.TypeName} legacy restore changed another field.");
        }
        Console.WriteLine($"Debugger spatial merges: {DebuggerSpatialFieldMerges.Merges.Length} legacy component layouts restore their exact spatial values.");
    }

    private static object SpatialMergeSample(Type componentType, int index) => componentType switch
    {
        _ when componentType == typeof(byte) => (object)(byte)(index == 0 ? 0xfe : 0x12),
        _ when componentType == typeof(ushort) => (object)(ushort)(index == 0 ? 0xffe0 : 0x8123),
        _ when componentType == typeof(short) => (object)(short)(index == 0 ? -46 : 0x0123),
        _ when componentType == typeof(int) => (object)(index == 0 ? 0x1234 : 0x56),
        _ => throw new InvalidOperationException($"No sample for legacy component type {componentType}."),
    };

    /// <summary>
    /// Rewrites a serialized root: its single current field record becomes the legacy component
    /// records. Every value type serializes the same as a root or a nested field.
    /// </summary>
    private static byte[] SpatialMergeLegacyLayout(byte[] serializedRoot, FieldInfo field, object value,
        DebuggerSpatialFieldMerge merge, object[] components)
    {
        string declaring = DebuggerStateTypeIdentity.GetSerializedName(field.DeclaringType!);
        byte[] currentRecord = [.. SpatialMergeNames(declaring, field.Name), .. SpatialMergeSerialize(value)];
        int at = SpatialMergeIndexOf(serializedRoot, currentRecord);
        if (at < 0 || SpatialMergeIndexOf(serializedRoot.AsSpan(at + 1).ToArray(), currentRecord) >= 0)
            throw new InvalidOperationException($"{merge.TypeName}.{merge.Field} record is not uniquely locatable.");
        var legacyRecords = new List<byte>();
        for (int index = 0; index < components.Length; index++)
        {
            legacyRecords.AddRange(SpatialMergeNames(declaring, merge.LegacyFields[index]));
            legacyRecords.AddRange(SpatialMergeSerialize(components[index]));
        }

        using var input = new BinaryReader(new MemoryStream(serializedRoot));
        input.ReadByte(); // New-object marker.
        input.ReadInt32(); // Reference id.
        input.ReadString(); // Root type identity.
        input.ReadByte(); // Field-payload kind.
        int countOffset = (int)input.BaseStream.Position;
        int count = input.ReadInt32();

        byte[] result = [.. serializedRoot[..at], .. legacyRecords, .. serializedRoot[(at + currentRecord.Length)..]];
        BitConverter.GetBytes(count + components.Length - 1).CopyTo(result, countOffset);
        return result;
    }

    private static byte[] SpatialMergeNames(string declaring, string field)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(declaring);
            writer.Write(field);
        }
        return stream.ToArray();
    }

    private static int SpatialMergeIndexOf(byte[] haystack, byte[] needle) =>
        haystack.AsSpan().IndexOf(needle);

    private static FieldInfo SpatialMergeField(Type type, string name) =>
        TrySpatialMergeField(type, name) ?? throw new InvalidOperationException($"{type.FullName}.{name} does not exist.");

    private static FieldInfo? TrySpatialMergeField(Type type, string name)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
            if (current.GetField(name, SpatialMergeFields | BindingFlags.DeclaredOnly) is { } found)
                return found;
        return null;
    }

    private static byte[] SpatialMergeSerialize(object value)
    {
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, value);
        return stream.ToArray();
    }

    private static object SpatialMergeDeserialize(byte[] bytes, Type type)
    {
        using var stream = new MemoryStream(bytes);
        MethodInfo deserialize = typeof(DebuggerObjectGraphSerializer).GetMethod(nameof(DebuggerObjectGraphSerializer.Deserialize))!
            .MakeGenericMethod(type.IsValueType ? typeof(object) : type);
        return deserialize.Invoke(null, [stream, false])!;
    }
}
