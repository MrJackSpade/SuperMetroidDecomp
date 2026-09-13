using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyRoomCallbackStateIdentity()
    {
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type closureType = DebuggerStateTypeIdentity.Resolve(DebuggerStateTypeIdentity.RoomLoadCallbacksIdentity)!;
        object target = RuntimeHelpers.GetUninitializedObject(closureType);
        var runtime = (SuperMetroidRuntime)RuntimeHelpers.GetUninitializedObject(typeof(SuperMetroidRuntime));
        var samus = new SamusState { XPosition = 137, Health = 83 };
        typeof(SuperMetroidRuntime).GetField("<Samus>k__BackingField", fields)!.SetValue(runtime, samus);
        var room = new CartridgeRoomHeader(0, 0, AreaId.Maridia, 0, 0, 1, 1, 0, 0, 0, 0, null!);
        closureType.GetField("<>4__this", fields)!.SetValue(target, runtime);
        closureType.GetField("room", fields)!.SetValue(target, room);
        var callback = closureType.GetMethod("<LoadCartridgeRoom>b__0", fields)!
            .CreateDelegate<Func<SamusState>>(target);
        using var encoded = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(encoded, callback);
        byte[] current = encoded.ToArray();
        AssertTrue(!Encoding.UTF8.GetString(current).Contains(closureType.FullName!),
            "room callback graph does not persist compiler ordinal in type, field, or delegate identities");
        foreach (string identity in new[] {
            DebuggerStateTypeIdentity.RoomLoadCallbacksIdentity,
            "SuperMetroid.Core.Runtime.SuperMetroidRuntime+<>c__DisplayClass443_0, SuperMetroid.Core, Version=0.2.1.0, Culture=neutral, PublicKeyToken=null",
            "SuperMetroid.Core.Runtime.SuperMetroidRuntime+<>c__DisplayClass461_0, SuperMetroid.Core, Version=0.2.1.0, Culture=neutral, PublicKeyToken=null",
            "SuperMetroid.Core.Runtime.SuperMetroidRuntime+<>c__DisplayClass463_0, SuperMetroid.Core, Version=0.3.1.0, Culture=neutral, PublicKeyToken=null" })
        {
            byte[] graph = ReplaceEncodedIdentity(current, DebuggerStateTypeIdentity.RoomLoadCallbacksIdentity, identity);
            using var source = new MemoryStream(graph);
            var restored = DebuggerObjectGraphSerializer.Deserialize<Func<SamusState>>(source);
            AssertEqual((ushort)137, restored().XPosition, "restored room callback invokes captured runtime");
            AssertEqual((ushort)83, restored().Health, "restored room callback retains captured actor state");
            AssertEqual(AreaId.Maridia, ((CartridgeRoomHeader)closureType.GetField("room", fields)!
                .GetValue(restored.Target)!).AreaIndex, "restored callback retains nondefault room capture");
        }
        AssertTrue(DebuggerStateTypeIdentity.Resolve(
            "SuperMetroid.Core.Runtime.SuperMetroidRuntime+<>c__DisplayClass999999_0, SuperMetroid.Core") is null,
            "unknown legacy compiler closures are not guessed");
    }

    private static byte[] ReplaceEncodedIdentity(byte[] graph, string from, string to)
    {
        static byte[] Encode(string value)
        {
            using var bytes = new MemoryStream();
            using (var writer = new BinaryWriter(bytes, Encoding.UTF8, leaveOpen: true)) writer.Write(value);
            return bytes.ToArray();
        }
        byte[] needle = Encode(from), replacement = Encode(to);
        using var result = new MemoryStream();
        for (int cursor = 0; cursor < graph.Length;)
        {
            if (graph.AsSpan(cursor).StartsWith(needle))
            {
                result.Write(replacement);
                cursor += needle.Length;
            }
            else result.WriteByte(graph[cursor++]);
        }
        return result.ToArray();
    }
}
