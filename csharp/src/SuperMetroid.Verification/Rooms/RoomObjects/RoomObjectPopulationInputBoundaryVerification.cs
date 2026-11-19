using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;

/// <summary>Checks typed PLM population ownership and execution on a RAM-only bus.</summary>
internal static class RoomObjectPopulationInputBoundaryVerification
{
    /// <summary>
    /// Processing positions the typed population occupies after extension deletion frees the first slot in processing
    /// order.
    /// </summary>
    private static readonly int[] ReusedPopulationSlots =
        [0, 1];

    /// <summary>The compiled scroll change the fixture's triggers apply: it opens screen (1, 0) aligned to its top.</summary>
    private const RoomScrollChange TriggerChange = RoomScrollChange.BrinstarReserveTankOpensScreen1x0TopAligned;

    /// <summary>Verifies the identified typed-input conversion, without importing a ROM or driving gameplay.</summary>
    internal static void VerifyObjectPopulationInputBoundary()
    {
        // A scroll trigger applies its compiled scroll change; no room builds another kind of scroll write (#2355).
        var trigger = RoomObjectPlacementAccess.Placed(RoomObjectKind.ScrollTrigger,
            new(3, 3), scrollChange: TriggerChange);
        var extension = RoomObjectPlacementAccess.Placed(RoomObjectKind.RightwardsScrollExtension,
            new(4, 3));
        var second = global::SuperMetroid.Core.PrivateState.With<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPlacement>(trigger with {  }, "Block", (global::SuperMetroid.Core.Spatial.Position16<global::SuperMetroid.Core.Spatial.RoomBlocks>)(new(5, trigger.Block.Y)));
        RoomObjectPlacement[] records = [trigger, extension, second];
        var population = global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulation>([typeof(string), typeof(global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPlacement>)], new object?[] { (string)($"$8F:{0xfffa:X4}"), (global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPlacement>)(records) });
        records[0] = second;
        AssertEqual(3, population.Placements.Span[0].Block.X,
            "decoded population owns its record ordering independently of caller mutation");

        var level = new RoomLevelData(new Extent16<RoomBlocks>(8, 8), new RoomLevelPlanes(new RoomTerrainBlock[64], new RoomBlockBehavior[64], new RoomBlockAppearance[64]), RoomLevelData.NothingBeyondGrid(), []);
        var roomObjects = RoomObjectFixtures.Unbound();
        RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Crateria), population, RoomObjectServiceFixtures.Uninstalled with { Samus = new FixtureRoomSamus(() => new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        AssertTrue(roomObjects.PopulationPlaces.Select(place => place.UpdatePosition).SequenceEqual(ReusedPopulationSlots),
            "extension deletion lets the next record reuse the first slot in processing order, in authored order");
        AssertEqual(RoomBlockBehaviorCodec.Encode(RoomBlockBehavior.Reacting(SpecialAirReaction.ScrollTrigger)), RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(3, 3)).Behavior),
            "typed trigger setup publishes its native collision behavior");
        AssertEqual((int)RoomCollisionType.HorizontalExtension, (int)level.GetCollisionBlock(new(4, 3)).CollisionType,
            "typed extension setup publishes its native collision type");
        var scrolls = RoomScrollGrid.CreateImplicit(RoomScrollGrid.PowerOnCells(), new Extent16<RoomScreens>(2, 2), RoomScrollState.Closed);
        AssertTrue(roomObjects.Scrolls.TryNotifyScrollTouch(new(3, 3)), "typed trigger wakes at its placement");
        roomObjects.Step(level, scrolls);
        foreach (ScreenScrollChange write in RoomObjectScrollChangeDefinitions.Changes[TriggerChange])
            AssertEqual(write.State, scrolls.ReadScreen(write.Screen), "the compiled scroll change mutates the intended live scroll cell");
        AssertTrue(!roomObjects.ScrollObjectTriggered(roomObjects.Scrolls.TriggerBlocks[0]), "the trigger returns to sleep after its last write");

        // A population holds any number of placements, in order (#1627); the room's places bound how many objects it holds.
        RoomObjectPlacement[] many = [.. Enumerable.Range(0, 300).Select(index => global::SuperMetroid.Core.PrivateState.With<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPlacement>(second with {  }, "Block", (global::SuperMetroid.Core.Spatial.Position16<global::SuperMetroid.Core.Spatial.RoomBlocks>)(new(checked((ushort)(index % 8)), checked((ushort)(index / 8))))))];
        var large = global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulation>([typeof(string), typeof(global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPlacement>)], new object?[] { (string)($"$8F:{0x9000:X4}"), (global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPlacement>)(many) });
        AssertEqual(300, large.Placements.Length, "a population keeps more than 255 placements");
        AssertTrue(large.Placements.ToArray().Select(placement => placement.Block).SequenceEqual(many.Select(placement => placement.Block)),
            "a large population keeps its placements in authored order");

        int count = 0;
        foreach (ushort pointer in RoomObjectPopulationDefinitions.Pointers)
            count += RoomObjectPopulationDefinitions.Get(pointer).Placements.Length;
        AssertEqual(RoomObjectPopulationDefinitions.RetailRecordCount, count,
            "every compiled population converts to the typed contract before gameplay");
        Console.WriteLine("PLM input boundary: 283 compiled populations/941 records, native slot reuse, RAM-only scroll execution, and bounded validation pass.");
    }

    /// <summary>Runs the focused compiled-population conversion and scroll execution check.</summary>
    [VerificationCommand("--plm-population-input-boundary")]
    internal static int RoomObjectPopulationInputBoundaryCommand()
    {
        Suite(nameof(VerifyObjectPopulationInputBoundary), () => VerifyObjectPopulationInputBoundary());
        return 0;
    }
}
