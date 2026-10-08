using System.Runtime.CompilerServices;
using SuperMetroid.Core.Game;
using SuperMetroid.Desktop;

/// <summary>
/// An older debugger state may still carry a field a later build removed because nothing read it.
/// Restoration drains exactly the registered retired fields and still rejects any other unknown field.
/// </summary>
internal static class DebuggerRetiredFieldTests
{
    internal static void Run()
    {
        var state = (RidleyEnemyState)RuntimeHelpers.GetUninitializedObject(typeof(RidleyEnemyState));
        byte[] current = Serialize(state);

        byte[] legacy = WithAppendedField(current, typeof(RidleyEnemyState), "<FireballCooldown>k__BackingField", (ushort)7);
        RidleyEnemyState restored = Deserialize<RidleyEnemyState>(legacy);
        if (!Serialize(restored).AsSpan().SequenceEqual(current))
            throw new InvalidOperationException("A retired debugger field must be drained without changing current fields.");

        byte[] unknown = WithAppendedField(current, typeof(RidleyEnemyState), "<NeverDeclared>k__BackingField", (ushort)7);
        bool rejected = false;
        try { Deserialize<RidleyEnemyState>(unknown); }
        catch (InvalidDataException) { rejected = true; }
        if (!rejected)
            throw new InvalidOperationException("An unregistered unknown debugger field must still be rejected.");
        Console.WriteLine("Debugger retired fields: registered legacy fields drain; other unknown fields are rejected.");
    }

    private static byte[] Serialize(object value)
    {
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, value);
        return stream.ToArray();
    }

    private static T Deserialize<T>(byte[] bytes) where T : class
    {
        using var stream = new MemoryStream(bytes);
        return DebuggerObjectGraphSerializer.Deserialize<T>(stream);
    }

    /// <summary>
    /// Appends one field record to a serialized root object. The root's field records end the
    /// stream, so the new record follows them and only the field count changes.
    /// </summary>
    private static byte[] WithAppendedField(byte[] serializedRoot, Type declaringType, string fieldName, object value)
    {
        using var input = new BinaryReader(new MemoryStream(serializedRoot));
        input.ReadByte(); // New-object marker.
        input.ReadInt32(); // Reference id.
        input.ReadString(); // Root type identity.
        input.ReadByte(); // Field-payload kind.
        long countOffset = input.BaseStream.Position;
        int count = input.ReadInt32();

        using var output = new MemoryStream();
        output.Write(serializedRoot);
        output.Position = countOffset;
        using (var writer = new BinaryWriter(output, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(count + 1);
            output.Position = output.Length;
            writer.Write(DebuggerStateTypeIdentity.GetSerializedName(declaringType));
            writer.Write(fieldName);
        }
        output.Write(Serialize(value));
        return output.ToArray();
    }
}
