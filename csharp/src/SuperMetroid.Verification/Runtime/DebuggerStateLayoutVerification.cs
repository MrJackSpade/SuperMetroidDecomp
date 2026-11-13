using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms.LevelData;
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
        RequireUnsupported<SamusGrappleState>(Replace(current, "<PoseChangeAutoFireTimer>k__BackingField", "<PoseChangeAutoFireTamer>k__BackingField"),
            "a renamed field");
        RequireUnsupported<SamusGrappleState>(Replace(current, "SuperMetroid.Core.Samus.Grapple.SamusGrappleState,", "SuperMetroid.Core.Samus.Grapple.SamusGrappleStatf,"),
            "a type this build does not have");
        RequireUnsupported<SamusGrappleState>(Replace(current, "System.Int32,", "System.Int64,"), "a retyped field value");
        Console.WriteLine("Debugger state layout: current graphs round-trip; renamed fields, missing types and retyped values are rejected.");

    }

    /// <summary>Requires restoring <paramref name="stale"/> as a <typeparamref name="T"/> to be rejected as an unsupported version.</summary>
    private static void RequireUnsupported<T>(byte[] stale, string label) where T : class
    {
        try
        {
            _ = DeserializeDebuggerGraph<T>(stale);
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

    /// <summary>A copy of <paramref name="graph"/> with the first occurrence of <paramref name="from"/> replaced by the same-length <paramref name="to"/>.</summary>
    private static byte[] Replace(byte[] graph, string from, string to)
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

    /// <summary>
    /// #1681: a restored state finds every key of its hash-keyed collections. The fixture keys by records and by reference
    /// identity; the restored keys are new objects whose identity hashes differ from the captured ones, so a bucket layout
    /// carried over from the capture would miss them. Every rebuilt kind finds each key, equal records built after the
    /// restore among them, keeps its count, comparer and enumeration order; a hash-keyed kind with no rebuild, or a
    /// subclass of a rebuilt one, is refused at capture.
    /// </summary>
    internal static void VerifyDebuggerStateRebuildsHashKeyedCollections()
    {
        var state = new HashKeyedState();
        for (int index = 0; index < 48; index++)
        {
            var owner = new HashedOwner($"owner {index}");
            var key = new HashedKey(owner.Name, owner);
            state.Owners.Add(owner);
            state.ByRecord.Add(key, index);
            state.ByReference.Add(owner, owner.Name);
            state.ByIdentity.Add(owner, index);
            state.Members.Add(key);
            state.Concurrent.TryAdd(key, index);
        }
        // A removal leaves a free entry in the captured layout, which the rebuild does not carry.
        state.ByRecord.Remove(state.ByRecord.Keys.First());
        state.Immutable = state.ByRecord.ToImmutableDictionary();
        state.ImmutableMembers = [.. state.Owners];
        state.Frozen = state.ByRecord.ToFrozenDictionary();
        state.FrozenMembers = state.Owners.ToFrozenSet();

        HashKeyedState restored = DeserializeDebuggerGraph<HashKeyedState>(SerializeDebuggerGraph(state));
        AssertTrue(!ReferenceEquals(state.Owners[0], restored.Owners[0]) &&
            state.Owners.Zip(restored.Owners).Any(pair =>
                System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(pair.First) !=
                System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(pair.Second)),
            "the restored keys are new objects that hash differently from the captured ones");
        AssertTrue(state.ByRecord.Keys.Select(key => key.Name).SequenceEqual(restored.ByRecord.Keys.Select(key => key.Name)),
            "a rebuilt dictionary keeps its enumeration order");
        AssertTrue(restored.ByIdentity.Comparer is ReferenceEqualityComparer, "a rebuilt dictionary keeps its own comparer");
        AssertEqual(state.ByRecord.Count, restored.ByRecord.Count, "the record-keyed dictionary keeps its count");
        AssertEqual(state.Owners.Count, restored.ByReference.Count, "the reference-keyed dictionary keeps its count");
        AssertEqual(state.Owners.Count, restored.Members.Count, "the hash set keeps its count");
        for (int index = 0; index < restored.Owners.Count; index++)
        {
            HashedOwner owner = restored.Owners[index];
            var key = new HashedKey(owner.Name, owner);
            int expected = index;
            bool kept = state.ByRecord.ContainsKey(new HashedKey(state.Owners[index].Name, state.Owners[index]));
            AssertEqual(kept, restored.ByRecord.TryGetValue(key, out int byRecord) && byRecord == expected,
                $"the record-keyed dictionary finds {owner.Name}");
            AssertEqual(kept, restored.Immutable.TryGetValue(key, out int immutable) && immutable == expected,
                $"the immutable dictionary finds {owner.Name}");
            AssertEqual(kept, restored.Frozen.TryGetValue(key, out int frozen) && frozen == expected,
                $"the frozen dictionary finds {owner.Name}");
            AssertTrue(restored.ByReference.TryGetValue(owner, out string? name) && name == owner.Name,
                $"the reference-keyed dictionary finds {owner.Name}");
            AssertTrue(restored.ByIdentity.TryGetValue(owner, out int byIdentity) && byIdentity == expected,
                $"the identity-compared dictionary finds {owner.Name}");
            AssertTrue(restored.Members.Contains(key), $"the hash set finds {owner.Name}");
            AssertTrue(restored.Concurrent.TryGetValue(key, out int concurrent) && concurrent == expected,
                $"the concurrent dictionary finds {owner.Name}");
            AssertTrue(restored.ImmutableMembers.Contains(owner), $"the immutable hash set finds {owner.Name}");
            AssertTrue(restored.FrozenMembers.Contains(owner), $"the frozen set finds {owner.Name}");
        }

        AssertThrows<NotSupportedException>(() => SerializeDebuggerGraph(new System.Collections.Hashtable { ["key"] = 1 }),
            "a hash-keyed collection with no rebuild is refused at capture");
        AssertThrows<NotSupportedException>(() => SerializeDebuggerGraph(new NamedCounts { ["key"] = 1 }),
            "a subclass of a rebuilt collection is refused at capture");
        Console.WriteLine("Debugger state hash-keyed collections: every rebuilt kind finds each restored key; unrebuilt kinds are refused.");
    }

    /// <summary>
    /// The most bytes a Landing Site runtime's debugger state may take (#1696). Its level data alone made the state 137 MB
    /// when every block was written as a framed object naming its type and each field; written as packed runs of the
    /// blocks' values, the whole state fits in a small fraction of that.
    /// </summary>
    private const int LandingSiteStateBound = 16 * 1024 * 1024;

    /// <summary>
    /// #1696: a debugger state writes a room's per-block arrays (terrain, behaviours, appearances and the terrain beyond
    /// the room) as packed runs of their values rather than a framed object per block. A runtime standing in Landing Site
    /// captures within <see cref="LandingSiteStateBound"/>, and its restore is the same graph: its digest equals the
    /// captured runtime's, and its level data equals the captured room's field by field.
    /// </summary>
    internal static void VerifyDebuggerStatePacksLevelBlocks()
    {
        SuperMetroidRuntime runtime = RetailRuntimeFixtureVerification.CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(SuperMetroid.Core.Hud.HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(SuperMetroid.Core.Rooms.RoomDefinitions.LandingSite);
        object[] content = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent;

        byte[] captured = GraphDigestVerification.SerializeGraph(runtime, content);
        Console.WriteLine($"Debugger state packing: a Landing Site runtime captures to {captured.Length} bytes (bound {LandingSiteStateBound}).");
        AssertTrue(captured.Length <= LandingSiteStateBound,
            $"a Landing Site runtime's state takes {captured.Length} bytes, over the {LandingSiteStateBound}-byte bound");

        var restored = GraphDigestVerification.RestoreGraph<SuperMetroidRuntime>(captured, content);
        AssertTrue(GraphDigestVerification.GraphDigest(restored, content).SequenceEqual(GraphDigestVerification.GraphDigest(runtime, content)),
            "the restored Landing Site runtime has the captured runtime's graph digest");
        AssertTrue(GraphDigestVerification.InstanceStateEquals(restored.Room.LoadedLevelData, runtime.Room.LoadedLevelData),
            "the restored Landing Site level data equals the captured room's, field by field");
        Console.WriteLine("Debugger state packing: the Landing Site runtime restores to an equal graph and equal level data.");

        // A packed run's schema is checked against this build field by field before a value is read, as a framed object is.
        RoomTerrainBlock[] blocks =
        [
            RoomTerrainBlock.Of(5, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.SolidBlock),
            RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Air),
        ];
        byte[] packed = SerializeDebuggerGraph(blocks);
        AssertTrue(DeserializeDebuggerGraph<RoomTerrainBlock[]>(packed).SequenceEqual(blocks), "a packed block run round-trips");
        RequireUnsupported<RoomTerrainBlock[]>(Replace(packed, "<Collision>k__BackingField", "<Collisiom>k__BackingField"),
            "a packed run with a renamed field");
        RequireUnsupported<RoomTerrainBlock[]>(Replace(packed, "LevelData.RoomCollisionType,", "LevelData.RoomCollisionTypf,"),
            "a packed run naming a type this build does not have");
        Console.WriteLine("Debugger state packing: packed runs with renamed fields or missing types are rejected.");
    }

    /// <summary>A key compared by reference identity, whose hash is its object's.</summary>
    /// <param name="name">The name the fixture gives it.</param>
    private sealed class HashedOwner(string name)
    {
        /// <summary>The name the fixture gives it.</summary>
        internal string Name { get; } = name;
    }

    /// <summary>A record key, whose hash combines its name's and its owner's identity hash.</summary>
    /// <param name="Name">The owner's name.</param>
    /// <param name="Owner">The owner it keys.</param>
    private sealed record HashedKey(string Name, HashedOwner Owner);

    /// <summary>A dictionary subclass, which the rebuild of its base cannot rebuild.</summary>
    private sealed class NamedCounts : Dictionary<string, int>;

    /// <summary>A state holding every hash-keyed collection kind the debugger state rebuilds.</summary>
    private sealed class HashKeyedState
    {
        /// <summary>Every owner, in creation order.</summary>
        internal List<HashedOwner> Owners { get; } = [];

        /// <summary>Record-keyed.</summary>
        internal Dictionary<HashedKey, int> ByRecord { get; } = [];

        /// <summary>Reference-keyed by the default comparer.</summary>
        internal Dictionary<HashedOwner, string> ByReference { get; } = [];

        /// <summary>Reference-keyed by an explicit comparer.</summary>
        internal Dictionary<HashedOwner, int> ByIdentity { get; } = new(ReferenceEqualityComparer.Instance);

        /// <summary>A record set.</summary>
        internal HashSet<HashedKey> Members { get; } = [];

        /// <summary>A concurrent record-keyed dictionary.</summary>
        internal System.Collections.Concurrent.ConcurrentDictionary<HashedKey, int> Concurrent { get; } = new();

        /// <summary>An immutable record-keyed dictionary.</summary>
        internal ImmutableDictionary<HashedKey, int> Immutable { get; set; } = ImmutableDictionary<HashedKey, int>.Empty;

        /// <summary>An immutable reference set.</summary>
        internal ImmutableHashSet<HashedOwner> ImmutableMembers { get; set; } = [];

        /// <summary>A frozen record-keyed dictionary.</summary>
        internal FrozenDictionary<HashedKey, int> Frozen { get; set; } = FrozenDictionary<HashedKey, int>.Empty;

        /// <summary>A frozen reference set.</summary>
        internal FrozenSet<HashedOwner> FrozenMembers { get; set; } = FrozenSet<HashedOwner>.Empty;
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
        Suite(nameof(VerifyDebuggerStateRebuildsHashKeyedCollections), () => VerifyDebuggerStateRebuildsHashKeyedCollections());
        Suite(nameof(VerifyDebuggerStatePacksLevelBlocks), () => VerifyDebuggerStatePacksLevelBlocks());
        return 0;
    }
}
