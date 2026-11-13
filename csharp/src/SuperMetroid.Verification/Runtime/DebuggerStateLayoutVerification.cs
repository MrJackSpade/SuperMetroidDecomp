using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Runtime;
using SuperMetroid.Verification.Harness;

/// <summary>Checks debugger graph restoration rejects stale field and type layouts.</summary>
internal static class DebuggerStateLayoutVerification
{
    /// <summary>
    /// #1270: debugger states restore only this build's exact layout. A stale field, a type this build
    /// lacks, or a retyped value is rejected as an unsupported state version instead of being converted.
    /// </summary>
    internal static void VerifyDebuggerStateRejectsStaleLayouts()
    {
        var state = new SamusGrappleState { PoseChangeAutoFireTimer = 7 };
        byte[] current = SerializeDebuggerGraph(state);
        AssertEqual(7, DeserializeDebuggerGraph<SamusGrappleState>(current).PoseChangeAutoFireTimer,
            "a current-layout graph round-trips");

        // Same-length replacements keep every length prefix valid, so only the identity changes.
        RequireUnsupported(Replace(current, "<PoseChangeAutoFireTimer>k__BackingField", "<PoseChangeAutoFireTamer>k__BackingField"),
            "a renamed field");
        RequireUnsupported(Replace(current, "SuperMetroid.Core.Samus.Grapple.SamusGrappleState,", "SuperMetroid.Core.Samus.Grapple.SamusGrappleStatf,"),
            "a type this build does not have");
        RequireUnsupported(Replace(current, "System.Int32,", "System.Int64,"), "a retyped field value");
        Console.WriteLine("Debugger state layout: current graphs round-trip; renamed fields, missing types and retyped values are rejected.");

        static void RequireUnsupported(byte[] stale, string label)
        {
            try
            {
                _ = DeserializeDebuggerGraph<SamusGrappleState>(stale);
            }
            catch (InvalidDataException exception) when (
                exception.Message.Contains(DebuggerObjectGraphSerializer.UnsupportedVersion, StringComparison.Ordinal))
            {
                return;
            }
            catch (EndOfStreamException)
            {
                throw new InvalidOperationException($"Verification failed: {label} was misread instead of rejected.");
            }
            throw new InvalidOperationException($"Verification failed: {label} was restored instead of rejected.");
        }

        static byte[] Replace(byte[] graph, string from, string to)
        {
            byte[] source = Encoding.UTF8.GetBytes(from), target = Encoding.UTF8.GetBytes(to);
            if (source.Length != target.Length)
                throw new InvalidOperationException("A layout substitution must keep the encoded length.");
            int index = graph.AsSpan().IndexOf(source);
            if (index < 0)
                throw new InvalidOperationException($"The serialized graph does not contain '{from}'.");
            byte[] copy = [.. graph];
            target.CopyTo(copy, index);
            return copy;
        }
    }

    /// <summary>
    /// #1623: a restored state holds the catalogs' own members, not copies, so it compares equal to them: a progress record
    /// holding the doors the tester's encounter reset clears, an item location and a load station comes back with each
    /// set's members being the catalog's objects and finding them, and a room's X-ray overlay comes back as the catalog's.
    /// </summary>
    internal static void VerifyDebuggerStateRestoresCatalogMembers()
    {
        IReadOnlyList<LockedDoor> eyeDoors = TesterAssistDefinitions.EncounterReset.ClearedDoors;
        ItemLocation item = ItemLocationDefinitions.All[^1];
        var progress = new PersistentProgress();
        foreach (LockedDoor door in eyeDoors)
            progress.OpenedDoors.Add(door);
        progress.CollectedItems.Add(item);
        progress.UsedStations.Add(LoadStationDefinitions.CrocomireSave);

        PersistentProgress restored = DeserializeDebuggerGraph<PersistentProgress>(SerializeDebuggerGraph(progress));
        AssertTrue(!ReferenceEquals(progress, restored) && !ReferenceEquals(progress.OpenedDoors, restored.OpenedDoors),
            "the restore builds its own progress record");
        AssertTrue(restored.OpenedDoors.Members.SequenceEqual(LockedDoorDefinitions.All.Where(eyeDoors.Contains)),
            "the restored opened doors are the catalog's eye doors the tester's encounter reset clears");
        AssertTrue(eyeDoors.All(restored.OpenedDoors.Contains), "the restored opened doors find each catalog eye door");
        AssertTrue(restored.CollectedItems.Members.SequenceEqual([item]) && restored.CollectedItems.Contains(item),
            "the restored collected items are the catalog's item location");
        AssertTrue(ReferenceEquals(LoadStationDefinitions.CrocomireSave, restored.UsedStations.Members.Single()) &&
            restored.UsedStations.Contains(LoadStationDefinitions.CrocomireSave), "the restored used station is the catalog's");

        object[] overlay = [SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinitions.BombTorizoAnimalEscapeBlocks];
        AssertTrue(ReferenceEquals(overlay[0], DeserializeDebuggerGraph<object[]>(SerializeDebuggerGraph(overlay))[0]),
            "a restored X-ray overlay is the catalog's");
        Console.WriteLine("Debugger state identities: restored progress sets and overlays hold the catalogs' own members.");
    }

    /// <summary>Serializes a fixture object through the production debugger graph format.</summary>
    private static byte[] SerializeDebuggerGraph(object root)
    {
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, root);
        return stream.ToArray();
    }

    /// <summary>Restores a graph through the production serializer to check layout admission.</summary>
    private static T DeserializeDebuggerGraph<T>(byte[] graph) where T : class
    {
        using var stream = new MemoryStream(graph, writable: false);
        return DebuggerObjectGraphSerializer.Deserialize<T>(stream);
    }

    /// <summary>Runs current and stale debugger layout checks.</summary>
    [VerificationCommand("--debugger-state-layout")]
    internal static int DebuggerStateLayoutCommand()
    {
        Suite(nameof(VerifyDebuggerStateRejectsStaleLayouts), () => VerifyDebuggerStateRejectsStaleLayouts());
        Suite(nameof(VerifyDebuggerStateRestoresCatalogMembers), () => VerifyDebuggerStateRestoresCatalogMembers());
        return 0;
    }
}
