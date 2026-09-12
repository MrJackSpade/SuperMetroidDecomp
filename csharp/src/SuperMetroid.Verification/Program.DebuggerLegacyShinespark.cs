using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyLegacyShinesparkGraph()
    {
        foreach (var phase in new[] { ShinesparkPhase.Inactive, ShinesparkPhase.Crash, ShinesparkPhase.CrashEchoCircle, ShinesparkPhase.CrashFinish })
        foreach (sbyte delta in new sbyte[] { -4, 4 })
        {
            using var legacy = BuildLegacyShinesparkGraph(phase, delta, invalidField: false);
            var restored = DebuggerObjectGraphSerializer.Deserialize<SamusShinesparkState>(legacy);
            bool crash = phase != ShinesparkPhase.Inactive;
            AssertEqual(phase, restored.Phase, "legacy spark phase survives graph restoration");
            AssertEqual(crash ? (ushort)92 : (ushort)333, restored.FirstReleasedCrashEcho.YPosition,
                "active crash travel or inactive echo Y owns the restored alias");
            AssertEqual(crash ? unchecked((ushort)(short)delta) : (ushort)222, restored.FirstReleasedCrashEcho.XPosition,
                "legacy signed crash delta is sign-extended without overwriting an inactive echo");
            using var roundTrip = new MemoryStream();
            DebuggerObjectGraphSerializer.Serialize(roundTrip, restored);
            roundTrip.Position = 0;
            var current = DebuggerObjectGraphSerializer.Deserialize<SamusShinesparkState>(roundTrip);
            AssertEqual(restored.FirstReleasedCrashEcho, current.FirstReleasedCrashEcho,
                "migrated aliases survive the current seventeen-field format");
        }
        using var malformed = BuildLegacyShinesparkGraph(ShinesparkPhase.Inactive, 4, invalidField: true);
        bool rejected = false;
        try { DebuggerObjectGraphSerializer.Deserialize<SamusShinesparkState>(malformed); }
        catch (InvalidDataException) { rejected = true; }
        AssertTrue(rejected, "legacy spark adapter still rejects an unknown field identity");
    }

    /// <summary>Constructs the exact nineteen-field layout without a ROM or player state.</summary>
    private static MemoryStream BuildLegacyShinesparkGraph(ShinesparkPhase phase, sbyte delta, bool invalidField)
    {
        var state = new SamusShinesparkState();
        typeof(SamusShinesparkState).GetProperty(nameof(SamusShinesparkState.Phase))!.SetValue(state, phase);
        object echo = typeof(SamusShinesparkState).GetField("_firstReleasedCrashEcho", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(state)!;
        echo.GetType().GetProperty("XPosition")!.SetValue(echo, (ushort)222);
        echo.GetType().GetProperty("YPosition")!.SetValue(echo, (ushort)333);
        using var original = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(original, state);
        byte[] bytes = original.ToArray();
        original.Position = 0;
        using var reader = new BinaryReader(original);
        reader.ReadByte(); reader.ReadInt32(); reader.ReadString(); reader.ReadByte();
        int countOffset = checked((int)original.Position);
        AssertEqual(17, reader.ReadInt32(), "current spark fixture field count");

        using var identity = new MemoryStream();
        using (var writer = new BinaryWriter(identity, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(typeof(SamusShinesparkState).AssemblyQualifiedName!);
            writer.Write("<LastReleasedCrashEchoClear>k__BackingField");
        }
        int insertion = bytes.AsSpan().IndexOf(identity.ToArray());
        AssertTrue(insertion > countOffset, "legacy travel insertion follows crash angle fields");
        var result = new MemoryStream();
        using (var writer = new BinaryWriter(result, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(bytes, 0, insertion);
            WriteRemovedField(writer, invalidField ? "unknown-travel" : "<CrashAngularTravel>k__BackingField", (ushort)92);
            writer.Write(bytes, insertion, bytes.Length - insertion);
            WriteRemovedField(writer, "_crashAngularDelta", delta);
            result.Position = countOffset;
            writer.Write(19);
        }
        result.Position = 0;
        return result;

        static void WriteRemovedField(BinaryWriter writer, string name, object value)
        {
            writer.Write(typeof(SamusShinesparkState).AssemblyQualifiedName!);
            writer.Write(name);
            // Serialize the primitive through the production codec: value types
            // use reference ID zero, so the surrounding graph's IDs stay intact.
            using var primitive = new MemoryStream();
            DebuggerObjectGraphSerializer.Serialize(primitive, value);
            writer.Write(primitive.ToArray());
        }
    }
}
