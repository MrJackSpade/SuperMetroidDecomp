using SuperMetroid.Core;
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
using SuperMetroid.Core.Rooms.RoomObjects;
namespace SuperMetroid.Verification.Runtime;
using SuperMetroid.Verification.Harness;

/// <summary>Checks debugger graph restoration rejects stale field and type layouts.</summary>
internal static partial class DebuggerStateLayoutVerification
{
    /// <summary>
    /// #1270: debugger states restore only this build's exact layout. A stale field, a type this build
    /// lacks, or a retyped value is rejected as an unsupported state version instead of being converted. The grapple state
    /// holds Samus's installed artwork (#2049), so it is captured and restored bound to the installed content, as a host's
    /// store binds it, and names that content by path instead of writing all of it (#2609).
    /// </summary>
    internal static void VerifyDebuggerStateRejectsStaleLayouts()
    {
        var state = new SamusGrappleState(global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { PoseChangeAutoFireTimer = UpdateCountdown.Lasting(7) };
        byte[] current = GraphDigestVerification.SerializeGraph(state);
        AssertEqual(7, GraphDigestVerification.RestoreGraph<SamusGrappleState>(current).PoseChangeAutoFireTimer.UpdatesLeft,
            "a current-layout graph round-trips");

        // Same-length replacements keep every length prefix valid, so only the identity changes.
        const string timerField = "PoseChangeAutoFireTimer";
        RequireUnsupported(Replace(current, timerField, "PoseChangeAutoFireTamer"),
            "a renamed field", RestoreBound<SamusGrappleState>);
        RequireUnsupported(Replace(current, "SuperMetroid.Core.Samus.Grapple.SamusGrappleState", "SuperMetroid.Core.Samus.Grapple.SamusGrappleStatf"),
            "a type this build does not have", RestoreBound<SamusGrappleState>);
        // A state stores each name's text once and names it by number after (#2602), so retyping the stored name retypes
        // every value of the type, and the first field read holding one is refused.
        RequireUnsupported(Replace(current, "System.Int32", "System.Int64"),
            "a retyped field value", RestoreBound<SamusGrappleState>);
        Console.WriteLine("Debugger state layout: current graphs round-trip; renamed fields, missing types and retyped values are rejected.");

    }

    /// <summary>Restores <paramref name="graph"/> bound to the installed content, as a host's store does.</summary>
    private static T RestoreBound<T>(byte[] graph) where T : class => GraphDigestVerification.RestoreGraph<T>(graph);

    /// <summary>Requires restoring <paramref name="stale"/> as a <typeparamref name="T"/> to be rejected as an unsupported version.</summary>
    private static void RequireUnsupported<T>(byte[] stale, string label) where T : class =>
        RequireUnsupported(stale, label, DeserializeDebuggerGraph<T>);

    /// <summary>Requires restoring <paramref name="stale"/> through <paramref name="restore"/> to be rejected as an unsupported version.</summary>
    private static void RequireUnsupported<T>(byte[] stale, string label, Func<byte[], T> restore) where T : class
    {
        try
        {
            _ = restore(stale);
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

    /// <summary>
    /// A copy of <paramref name="graph"/> with the first occurrence of <paramref name="from"/> replaced by the same-length
    /// <paramref name="to"/>: the name's stored text, which every later use of the name refers to.
    /// </summary>
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
    /// #2060: a state holding the room-object visuals captures and restores them. Their store holds every family's pictures
    /// under the draws that show them (#2571), so the restored store finds each draw's picture under this build's draw
    /// again, and the bound setup-block pictures with it (#2290).
    /// </summary>
    internal static void VerifyDebuggerStateRestoresRoomObjectVisuals()
    {
        RoomObjectSetupBlock block = Enum.GetValues<RoomObjectSetupBlock>()[0];
        var shown = RoomBlockAppearance.Of(5, BlockMirroring.Horizontal);
        var visuals = new SuperMetroid.Core.World.RoomObjectVisuals(RoomObjectFixtures.Installed with
        {
            RoomObjectSetupBlocks = new InstalledRoomObjectSetupBlocks(new Dictionary<RoomObjectSetupBlock, RoomBlockAppearance> { [block] = shown }),
        });
        RoomObjectDraw grapple = RoomObjectFixtures.Installed.RoomObjectGrappleBlockVisuals.Images.All.Keys.First();

        var restored = DeserializeDebuggerGraph<SuperMetroid.Core.World.RoomObjectVisuals>(SerializeDebuggerGraph(visuals));
        AssertTrue(!ReferenceEquals(visuals, restored), "the restore builds its own room-object visuals");
        AssertTrue(restored.SetupBlock(block) == shown,
            "the restored visuals show the bound setup-block pictures, showing what they showed");
        AssertTrue(restored.Image(grapple).Cells.OrderBy(cell => (cell.Key.Y, cell.Key.X))
                .SequenceEqual(visuals.Image(grapple).Cells.OrderBy(cell => (cell.Key.Y, cell.Key.X))),
            "the restored visuals find every other family's pictures under their draws too");
        Console.WriteLine("Debugger state room-object visuals: the picture store restores each draw's picture.");
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
        runtime.HudSetup.InitializeHud(SuperMetroid.Core.Hud.HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
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
        RequireUnsupported<RoomTerrainBlock[]>(Replace(packed, "Collision:", "Collisiom:"),
            "a packed run with a renamed field");
        RequireUnsupported<RoomTerrainBlock[]>(Replace(packed, "LevelData.RoomCollisionType", "LevelData.RoomCollisionTypf"),
            "a packed run naming a type this build does not have");
        Console.WriteLine("Debugger state packing: packed runs with renamed fields or missing types are rejected.");

        // #2609: struct elements holding references, as a room's block behaviours hold their kept behaviour, are packed too:
        // their field is named once in the run's schema, and each object they hold is written once and restored shared.
        var shared = new HashedOwner("shared");
        HoldingElement[] holding = [new(shared, 1), new(shared, 2), new(null, 3), new(new HashedOwner("own"), 4)];
        byte[] held = SerializeDebuggerGraph(new HoldingRun(holding));
        AssertEqual(1, Occurrences(held, "Owner:"), "a run of struct elements holding references names their field once");
        HoldingElement[] restoredHolding = DeserializeDebuggerGraph<HoldingRun>(held).Elements;
        AssertTrue(restoredHolding.Select(element => element.Count).SequenceEqual(holding.Select(element => element.Count)) &&
            restoredHolding[0].Owner is { Name: "shared" } && ReferenceEquals(restoredHolding[0].Owner, restoredHolding[1].Owner) &&
            restoredHolding[2].Owner is null && restoredHolding[3].Owner is { Name: "own" } && !ReferenceEquals(restoredHolding[3].Owner, restoredHolding[0].Owner),
            "a packed run restores the objects its elements held, a shared one shared and none as none");
        Console.WriteLine("Debugger state packing: struct elements holding references pack, their objects written once each.");
    }

    /// <summary>
    /// #2554: a restore never repeats an enemy's setup. Botwoon's setup spawns his body as the population load places him;
    /// a runtime standing in his room captures and restores, and the restored room holds exactly the body segments the
    /// captured one held, none spawned again, with the captured runtime's graph digest: the restore built every enemy
    /// through the constructor that only assigns what the state holds, and no species' setup ran on the restored objects.
    /// The room's species creations and part offers are named methods and binding objects, so the state holds them too.
    /// </summary>
    internal static void VerifyDebuggerStateRestoreRunsNoSetup()
    {
        SuperMetroidRuntime runtime = RetailRuntimeFixtureVerification.CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(SuperMetroid.Core.Hud.HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(SuperMetroid.Core.Rooms.RoomDefinitions.Botwoon);
        object[] content = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent;
        int captured = BotwoonBodySegments(runtime);
        AssertTrue(captured > 0, "Botwoon's setup spawned his body as his room's population load placed him");

        var restored = GraphDigestVerification.RestoreGraph<SuperMetroidRuntime>(GraphDigestVerification.SerializeGraph(runtime, content), content);
        AssertEqual(captured, BotwoonBodySegments(restored), "the restored room holds the body Botwoon's setup spawned, none spawned again");
        AssertTrue(GraphDigestVerification.GraphDigest(restored, content).SequenceEqual(GraphDigestVerification.GraphDigest(runtime, content)),
            "the restored Botwoon room runtime has the captured runtime's graph digest");
        Console.WriteLine($"Debugger state restore: Botwoon's room restores with his {captured} body segments, his setup not run again.");
    }

    /// <summary>
    /// #2554: a restore rebuilds the Dachora room's speed-boost echoes, which join their body's trail through the creation
    /// the body offered (<see cref="RoundTripJoinedParts"/>).
    /// </summary>
    internal static void VerifyDebuggerStateRestoresDachoraEchoes() =>
        RoundTripJoinedParts(JoinedPartsRuntime(), SuperMetroid.Core.Rooms.RoomDefinitions.Dachora,
            ("Dachora echoes", place => place.Entity is SuperMetroid.Core.Enemies.Animals.DachoraEchoEntity));

    /// <summary>
    /// #2554: a restore rebuilds Rising Tide's Squeept followers, which follow their parents, and its Dragon wings, which take
    /// their bodies' wing states, each through the creation its partner offered (<see cref="RoundTripJoinedParts"/>).
    /// </summary>
    internal static void VerifyDebuggerStateRestoresSqueeptAndDragonParts() =>
        RoundTripJoinedParts(JoinedPartsRuntime(), SuperMetroid.Core.Rooms.RoomDefinitions.RisingTide,
            ("Squeept followers", place => place.Entity is SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumperFollowerEntity),
            ("Dragon wings", place => place.Entity is SuperMetroid.Core.Enemies.Dragon.DragonWingEntity));

    /// <summary>
    /// #2554: a restore rebuilds the Cathedral's Geruta followers, which follow their parents through the creation each
    /// parent offered (<see cref="RoundTripJoinedParts"/>).
    /// </summary>
    internal static void VerifyDebuggerStateRestoresGerutaFollowers() =>
        RoundTripJoinedParts(JoinedPartsRuntime(), SuperMetroid.Core.Rooms.RoomDefinitions.Cathedral,
            ("Geruta followers", place => place.Entity is SuperMetroid.Core.Enemies.Rios.GerutaFollowerEntity));

    /// <summary>
    /// #2554: a restore rebuilds the Acid Statue room's Holtz followers, which follow their parents through the creation each
    /// parent offered, with the room's Chozo statue and its hand (<see cref="RoundTripJoinedParts"/>).
    /// </summary>
    internal static void VerifyDebuggerStateRestoresHoltzFollowers() =>
        RoundTripJoinedParts(JoinedPartsRuntime(), SuperMetroid.Core.Rooms.RoomDefinitions.AcidStatue,
            ("Holtz followers", place => place.Entity is SuperMetroid.Core.Enemies.Rios.HoltzFollowerEntity));

    /// <summary>
    /// #2554: a restore rebuilds the Attic's Ki-Hunter wings once Phantoon is dead, which follow their bodies through the
    /// creation each body offered (<see cref="RoundTripJoinedParts"/>).
    /// </summary>
    internal static void VerifyDebuggerStateRestoresKiHunterWings()
    {
        SuperMetroidRuntime runtime = JoinedPartsRuntime();
        runtime.System.RecordBossesDefeated(SuperMetroid.Core.Progression.AreaId.WreckedShip, SuperMetroid.Core.Progression.DefeatedBosses.AreaBoss);
        RoundTripJoinedParts(runtime, SuperMetroid.Core.Rooms.RoomDefinitions.Attic,
            ("Ki-Hunter wings", place => place.Entity is SuperMetroid.Core.Enemies.KiHunters.KiHunterWingsEntity));
    }

    /// <summary>
    /// #2554: a debugger state leaves out a get-only lazy cache (<c>X => field ??= …</c>), whose value its object's other
    /// state rebuilds on first use. A sealed primary-constructor type derived from a non-sealed primary-constructor base,
    /// each with a lazy reaction built before the capture, round-trips through the generated members: the base's restore
    /// does not compare its cache with one it just built, and each restored reaction is built again from the restored
    /// object, reading what the state restored and what changes after it, while the captured object's reactions still read
    /// the captured object.
    /// </summary>
    internal static void VerifyDebuggerStateRebuildsLazyCaches()
    {
        var shutter = new LazyShutter(3, 5) { Hits = 2 };
        AssertEqual(5, shutter.Reaction(), "the captured base's reaction reads its strength and hits");
        AssertEqual(7, shutter.Rise(), "the captured derived type's reaction reads its height and hits");

        LazyShutter restored = DeserializeDebuggerGraph<LazyShutter>(SerializeDebuggerGraph(shutter));
        AssertTrue(!ReferenceEquals(shutter, restored), "the restore builds its own object");
        AssertEqual(2, restored.Hits, "the restored object holds the captured hits");
        restored.Hits = 4;
        AssertEqual(7, restored.Reaction(), "the restored base's reaction is built again from the restored object");
        AssertEqual(9, restored.Rise(), "the restored derived type's reaction is built again from the restored object");
        AssertEqual(5, shutter.Reaction(), "the captured object's reaction still reads the captured object");
        Console.WriteLine("Debugger state lazy caches: a derived primary-constructor object restores and rebuilds its base's and its own caches.");
    }

    /// <summary>The updates the Kraid eye door room runs before its capture, past each eye door part's first update.</summary>
    private const int EyeDoorUpdatesBeforeCapture = 8;

    /// <summary>The updates the captured and the restored Kraid eye door rooms each run after the restore.</summary>
    private const int EyeDoorUpdatesAfterRestore = 120;

    /// <summary>
    /// #2554: a restore rebuilds the eye door parts' own dispatch of what each part does as its image ends, a lazy cache of
    /// arms reading the part itself (<see cref="SuperMetroid.Core.Rooms.Doors.EyeDoorPartObject"/>), which the state leaves
    /// out. The Kraid eye door room runs past its parts' first updates, so the captured parts have built theirs; the restored
    /// room holds parts in the same phases with the captured runtime's graph digest, and the captured and restored rooms
    /// then run the same updates to the same parts' phases and the same digest.
    /// </summary>
    internal static void VerifyDebuggerStateRestoresEyeDoorParts()
    {
        object[] content = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent;
        SuperMetroidRuntime runtime = JoinedPartsRuntime();
        runtime.LoadCartridgeRoomForDebug(SuperMetroid.Core.Rooms.RoomDefinitions.KraidEyeDoor);
        runtime.Player.Samus.Control.LockOutPlayer();
        for (int update = 0; update < EyeDoorUpdatesBeforeCapture; update++)
            runtime.StepFrame(0);
        SuperMetroid.Core.Rooms.Doors.EyeDoorPartPhase[] captured = EyeDoorPartPhases(runtime);
        AssertTrue(captured.Length > 0, "the Kraid eye door room placed its eye door's parts");

        var restored = GraphDigestVerification.RestoreGraph<SuperMetroidRuntime>(GraphDigestVerification.SerializeGraph(runtime, content), content);
        RetailRuntimeFixtureVerification.BindRetailRuntimeFixture(restored);
        AssertTrue(EyeDoorPartPhases(restored).SequenceEqual(captured), "the restored room holds the eye door parts in their captured phases");
        AssertTrue(GraphDigestVerification.GraphDigest(restored, content).SequenceEqual(GraphDigestVerification.GraphDigest(runtime, content)),
            "the restored Kraid eye door runtime has the captured runtime's graph digest");
        for (int update = 0; update < EyeDoorUpdatesAfterRestore; update++)
        {
            runtime.StepFrame(0);
            restored.StepFrame(0);
            AssertTrue(EyeDoorPartPhases(restored).SequenceEqual(EyeDoorPartPhases(runtime)),
                $"update {update + 1} after the restore leaves the restored eye door parts in the captured parts' phases");
        }
        AssertTrue(GraphDigestVerification.GraphDigest(restored, content).SequenceEqual(GraphDigestVerification.GraphDigest(runtime, content)),
            "the captured and restored Kraid eye door runtimes run to the same graph digest");
        Console.WriteLine($"Debugger state restore: the Kraid eye door's {captured.Length} parts restore and run {EyeDoorUpdatesAfterRestore} updates as captured.");
    }

    /// <summary>The phases of the eye door parts in <paramref name="runtime"/>'s room, in update order.</summary>
    private static SuperMetroid.Core.Rooms.Doors.EyeDoorPartPhase[] EyeDoorPartPhases(SuperMetroidRuntime runtime) =>
        [.. runtime.Room.LoadedScope.RoomObjects.Places.InUpdateOrder
            .Select(place => place.Behaviour)
            .OfType<SuperMetroid.Core.Rooms.Doors.EyeDoorPartObject>()
            .Select(part => part.Phase)];

    /// <summary>The most death updates that reach the suit's explosion: its preflash and its flashing (#2387).</summary>
    private const int DeathUpdatesToExplosion = 16 + 60;

    /// <summary>The death updates the captured and the restored runtimes each run after the restore, within the explosion.</summary>
    private const int DeathUpdatesAfterRestore = 40;

    /// <summary>
    /// #2554: a restore rebuilds Samus's death sequence's dispatch of what each death update draws, a lazy cache of arms
    /// reading the sequence itself (<see cref="SuperMetroid.Core.Samus.Death.SamusDeathSequenceState"/>), which the state
    /// leaves out. Samus dies in Alpha Power Bomb's room and her death runs into the suit's explosion, drawing each update,
    /// so the captured sequence has built its dispatch; the restored runtime has the captured graph digest and stage, and
    /// the captured and restored deaths then run the same updates to the same draws, explosion pictures and digest.
    /// </summary>
    internal static void VerifyDebuggerStateRestoresSamusDeathSequence()
    {
        object[] content = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent;
        SuperMetroidRuntime runtime = JoinedPartsRuntime();
        runtime.LoadCartridgeRoomForDebug(SuperMetroid.Core.Rooms.RoomDefinitions.AlphaPowerBomb);
        SuperMetroid.Core.Samus.SamusState samus = runtime.Player.Samus;
        samus.PoseId = SuperMetroid.Core.Samus.Poses.SamusPoseId.FacingRightNormalPose;
        SuperMetroid.Core.Samus.Poses.SamusPoseMetadata.RefreshCollisionRadii(samus);
        SuperMetroid.Core.Samus.Drawing.SamusPresentation.InitializeAnimation(samus);
        // The death begins from gameplay, where Samus's grounded movement runs.
        runtime.Player.GroundedSamusMovementEnabled = true;
        runtime.DeathSequence.BeginDeathSequenceAfterMusicWait(runtime.Room.LoadedScope, samus);
        for (int update = 0; update < DeathUpdatesToExplosion && samus.DeathSequence.Phase is not SuperMetroid.Core.Samus.Death.SamusDeathSequencePhase.Begun.SuitExplosion; update++)
            _ = StepDeath(runtime);
        AssertTrue(samus.DeathSequence.Phase is SuperMetroid.Core.Samus.Death.SamusDeathSequencePhase.Begun.SuitExplosion,
            "Samus's death reaches the suit's explosion after its preflash and flashing");
        AssertEqual(SuperMetroid.Core.Samus.Death.SamusDeathDraw.Explosion, StepDeath(runtime).Draws, "the exploding suit draws its explosion");

        var restored = GraphDigestVerification.RestoreGraph<SuperMetroidRuntime>(GraphDigestVerification.SerializeGraph(runtime, content), content);
        RetailRuntimeFixtureVerification.BindRetailRuntimeFixture(restored);
        AssertEqual(samus.DeathSequence.Phase, restored.Player.Samus.DeathSequence.Phase, "the restored death is in the captured stage");
        AssertTrue(GraphDigestVerification.GraphDigest(restored, content).SequenceEqual(GraphDigestVerification.GraphDigest(runtime, content)),
            "the restored dying runtime has the captured runtime's graph digest");
        for (int update = 0; update < DeathUpdatesAfterRestore; update++)
        {
            SuperMetroid.Core.Samus.Death.SamusDeathSequenceStepResult expected = StepDeath(runtime);
            AssertEqual(expected, StepDeath(restored), $"death update {update + 1} after the restore draws as the captured death draws");
            AssertEqual(samus.DeathSequence.ExplosionFrame, restored.Player.Samus.DeathSequence.ExplosionFrame,
                $"death update {update + 1} after the restore shows the captured explosion picture");
        }
        AssertTrue(GraphDigestVerification.GraphDigest(restored, content).SequenceEqual(GraphDigestVerification.GraphDigest(runtime, content)),
            "the captured and restored deaths run to the same graph digest");
        Console.WriteLine($"Debugger state restore: Samus's death restores in the suit's explosion and runs {DeathUpdatesAfterRestore} updates as captured.");
    }

    /// <summary>Runs one update of the death of <paramref name="runtime"/>'s Samus, as the game's death state runs it.</summary>
    private static SuperMetroid.Core.Samus.Death.SamusDeathSequenceStepResult StepDeath(SuperMetroidRuntime runtime) =>
        runtime.DeathSequence.StepDeathSequenceFrame(0, runtime.Room.LoadedScope, runtime.Player.Samus,
            (SuperMetroid.Core.Samus.Death.SamusDeathSequencePhase.Begun)runtime.Player.Samus.DeathSequence.Phase);

    /// <summary>A retail runtime with its HUD and Ceres start set up, from which a room loads directly.</summary>
    private static SuperMetroidRuntime JoinedPartsRuntime()
    {
        SuperMetroidRuntime runtime = RetailRuntimeFixtureVerification.CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(SuperMetroid.Core.Hud.HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        return runtime;
    }

    /// <summary>
    /// Loads <paramref name="room"/> into <paramref name="runtime"/>, captures and restores it, and checks that the restored
    /// room holds as many of each kind of offered part as the captured one, which holds some, and that the restored runtime
    /// has the captured runtime's graph digest.
    /// </summary>
    private static void RoundTripJoinedParts(SuperMetroidRuntime runtime, SuperMetroid.Core.Rooms.RoomDefinition room,
        params (string Parts, Func<SuperMetroid.Core.Enemies.RoomEnemyPlace, bool> IsPart)[] kinds)
    {
        object[] content = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent;
        runtime.LoadCartridgeRoomForDebug(room);
        int[] captured = [.. kinds.Select(kind => runtime.Room.LoadedScope.Enemies.Places.InUpdateOrder.Count(kind.IsPart))];
        for (int index = 0; index < kinds.Length; index++)
            AssertTrue(captured[index] > 0, $"the room's population load built {kinds[index].Parts} through their partners' offers");
        var restored = GraphDigestVerification.RestoreGraph<SuperMetroidRuntime>(GraphDigestVerification.SerializeGraph(runtime, content), content);
        for (int index = 0; index < kinds.Length; index++)
        {
            AssertEqual(captured[index], restored.Room.LoadedScope.Enemies.Places.InUpdateOrder.Count(kinds[index].IsPart),
                $"the restored room holds its {kinds[index].Parts}");
        }
        AssertTrue(GraphDigestVerification.GraphDigest(restored, content).SequenceEqual(GraphDigestVerification.GraphDigest(runtime, content)),
            $"the restored {room.Name} runtime has the captured runtime's graph digest");
        Console.WriteLine($"Debugger state restore: {room.Name} restores with its {string.Join(" and ", kinds.Select((kind, index) => $"{captured[index]} {kind.Parts}"))}.");
    }

    /// <summary>The live Botwoon body segments among <paramref name="runtime"/>'s room's enemy projectiles.</summary>
    private static int BotwoonBodySegments(SuperMetroidRuntime runtime) =>
        runtime.Room.LoadedScope.Enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.Kind == SuperMetroid.Core.Enemies.RoomEnemyProjectileKind.BotwoonBody);

    /// <summary>How many times <paramref name="text"/>'s UTF-8 bytes occur in <paramref name="graph"/>.</summary>
    private static int Occurrences(byte[] graph, string text)
    {
        byte[] sought = Encoding.UTF8.GetBytes(text);
        int count = 0;
        for (int start = graph.AsSpan().IndexOf(sought); start >= 0;)
        {
            count++;
            int next = graph.AsSpan(start + sought.Length).IndexOf(sought);
            start = next < 0 ? -1 : start + sought.Length + next;
        }
        return count;
    }

    /// <summary>A struct element holding a reference and a count, as a room's block behaviour holds its kept behaviour.</summary>
    /// <param name="Owner">The object it holds, or none.</param>
    /// <param name="Count">Its count.</param>
    private readonly partial record struct HoldingElement(HashedOwner? Owner, int Count);

    /// <summary>A run of <see cref="HoldingElement"/>s, held by a field declared with their array type, as a game object holds one.</summary>
    /// <param name="elements">The run.</param>
    private sealed partial class HoldingRun(HoldingElement[] elements)
    {
        /// <summary>The run.</summary>
        internal HoldingElement[] Elements { get; } = elements;
    }

    /// <summary>
    /// A non-sealed base that caches its own reaction in a lazy property built from its primary constructor's parameter, as
    /// an enemy part's base caches its own shot reaction.
    /// </summary>
    /// <param name="strength">What its reaction adds to its hits.</param>
    private partial class LazyReacting(int strength)
    {
        /// <summary>The hits it has taken.</summary>
        internal int Hits { get; set; }

        /// <summary>Its strength and hits, built once.</summary>
        internal Func<int> Reaction => field ??= () => strength + Hits;
    }

    /// <summary>A sealed primary-constructor type derived from <see cref="LazyReacting"/>, with a lazy reaction of its own.</summary>
    /// <param name="strength">What its base's reaction adds to its hits.</param>
    /// <param name="height">What its own reaction adds to its hits.</param>
    private sealed partial class LazyShutter(int strength, int height) : LazyReacting(strength)
    {
        /// <summary>Its height and hits, built once.</summary>
        internal Func<int> Rise => field ??= () => height + Hits;
    }

    /// <summary>A key compared by reference identity, whose hash is its object's.</summary>
    /// <param name="name">The name the fixture gives it.</param>
    private sealed partial class HashedOwner(string name)
    {
        /// <summary>The name the fixture gives it.</summary>
        internal string Name { get; } = name;
    }

    /// <summary>A record key, whose hash combines its name's and its owner's identity hash.</summary>
    /// <param name="Name">The owner's name.</param>
    /// <param name="Owner">The owner it keys.</param>
    private sealed partial record HashedKey(string Name, HashedOwner Owner);

    /// <summary>A dictionary subclass, which the rebuild of its base cannot rebuild.</summary>
    private sealed partial class NamedCounts : Dictionary<string, int>;

    /// <summary>A state holding every hash-keyed collection kind the debugger state rebuilds.</summary>
    private sealed partial class HashKeyedState
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
        GraphDigestVerification.DeclareDefinitionIndex();
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
        Suite(nameof(VerifyDebuggerStateRestoresRoomObjectVisuals), () => VerifyDebuggerStateRestoresRoomObjectVisuals());
        Suite(nameof(VerifyDebuggerStateRestoreRunsNoSetup), () => VerifyDebuggerStateRestoreRunsNoSetup());
        Suite(nameof(VerifyDebuggerStateRestoresDachoraEchoes), () => VerifyDebuggerStateRestoresDachoraEchoes());
        Suite(nameof(VerifyDebuggerStateRestoresSqueeptAndDragonParts), () => VerifyDebuggerStateRestoresSqueeptAndDragonParts());
        Suite(nameof(VerifyDebuggerStateRestoresGerutaFollowers), () => VerifyDebuggerStateRestoresGerutaFollowers());
        Suite(nameof(VerifyDebuggerStateRestoresHoltzFollowers), () => VerifyDebuggerStateRestoresHoltzFollowers());
        Suite(nameof(VerifyDebuggerStateRestoresKiHunterWings), () => VerifyDebuggerStateRestoresKiHunterWings());
        Suite(nameof(VerifyDebuggerStateRebuildsLazyCaches), () => VerifyDebuggerStateRebuildsLazyCaches());
        Suite(nameof(VerifyDebuggerStateRestoresEyeDoorParts), () => VerifyDebuggerStateRestoresEyeDoorParts());
        Suite(nameof(VerifyDebuggerStateRestoresSamusDeathSequence), () => VerifyDebuggerStateRestoresSamusDeathSequence());
        return 0;
    }
}
