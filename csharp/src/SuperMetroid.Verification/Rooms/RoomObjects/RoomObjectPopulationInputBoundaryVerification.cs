using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks typed PLM population ownership and execution on a RAM-only bus.</summary>
internal static class RoomObjectPopulationInputBoundaryVerification
{
    /// <summary>
    /// Processing positions the typed population occupies after extension deletion frees the first slot in processing
    /// order.
    /// </summary>
    private static readonly int[] ReusedPopulationSlots =
        [0, 1];

    /// <summary>Verifies the identified typed-input conversion, without importing a ROM or driving gameplay.</summary>
    internal static void VerifyObjectPopulationInputBoundary()
    {
        ScreenScrollChange[] writes = [new(new(1, 0), RoomScrollState.Open)];
        var trigger = new RoomObjectPlacement(RoomObjectKinds.RequirePlaced(RoomObjectKind.ScrollTrigger),
            new(3, 3), null, writes);
        var extension = new RoomObjectPlacement(RoomObjectKinds.RequirePlaced(RoomObjectKind.RightwardsScrollExtension),
            new(4, 3), null);
        var second = trigger with { Block = new(5, trigger.Block.Y), ScrollChange = Array.Empty<ScreenScrollChange>() };
        RoomObjectPlacement[] records = [trigger, extension, second];
        var population = new RoomObjectPopulation($"$8F:{0xfffa:X4}", records);
        writes[0] = new(new(0, 49), RoomScrollState.Closed);
        records[0] = second;
        AssertEqual(new ScreenScrollChange(new(1, 0), RoomScrollState.Open), population.Placements.Span[0].ScrollChange![0],
            "decoded population owns its scroll writes independently of caller mutation");
        AssertEqual(3, population.Placements.Span[0].Block.X,
            "decoded population owns its record ordering independently of caller mutation");

        var level = new RoomLevelData(new Extent16<RoomBlocks>(8, 8), new RoomLevelPlanes(new RoomTerrainBlock[64], new RoomBlockBehavior[64], new RoomBlockAppearance[64]), RoomLevelData.NothingBeyondGrid());
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertEqual(3, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Crateria), population, new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "typed population runs the real sequential allocator using a RAM-only bus");
        AssertTrue(roomObjects.PopulationSlots.Select(slot => slot.ProcessingPosition).SequenceEqual(ReusedPopulationSlots),
            "extension deletion lets the next record reuse the first slot in processing order, in authored order");
        AssertEqual(RoomBlockBehaviorCodec.Encode(RoomBlockBehavior.Reacting(SpecialAirReaction.ScrollTrigger)), RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(3, 3)).Behavior),
            "typed trigger setup publishes its native collision behavior");
        AssertEqual((int)RoomCollisionType.HorizontalExtension, (int)level.GetCollisionBlock(new(4, 3)).CollisionType,
            "typed extension setup publishes its native collision type");
        var scrolls = RoomScrollGrid.CreateImplicit(RoomScrollGrid.PowerOnCells(), new Extent16<RoomScreens>(2, 2), RoomScrollState.Closed);
        AssertTrue(roomObjects.Scrolls.TryNotifyScrollTouch(new(3, 3)), "typed trigger wakes at its placement");
        roomObjects.Step(level, scrolls);
        AssertEqual(RoomScrollState.Open, scrolls.ReadState(1), "decoded pairs mutate the intended live scroll cell");
        AssertTrue(!roomObjects.ScrollObjectTriggered(roomObjects.Scrolls.Snapshots[0].Block), "decoded trigger returns to sleep after its last write");

        AssertThrows<InvalidDataException>(() => new RoomObjectPopulation($"$8F:{0x9000:X4}",
            [trigger with { ScrollChange = [new(new(0, RoomScrollGrid.KeptCellCount), RoomScrollState.OpenAlignedToTop)] }]),
            "a decoded write to a screen no room width brings within the kept cells is rejected before allocation");
        AssertThrows<InvalidDataException>(() => new RoomObjectPopulation($"$8F:{0x9000:X4}",
            [trigger with { ScrollChange = [new(new(4, 10), RoomScrollState.Open)] }]),
            "a decoded write past the kept cells at every room width past its column is rejected before allocation");
        AssertEqual(2, new RoomObjectPopulation($"$8F:{0x9000:X4}", [trigger with { ScrollChange = [new(new(4, 9), RoomScrollState.Open), new(new(0, 49), RoomScrollState.Open)] }]).Placements.Span[0].ScrollChange!.Count,
            "writes to screens the last kept cell holds at the narrowest room width past their column are kept");
        AssertThrows<InvalidDataException>(() => new RoomObjectPopulation($"$8F:{0x9000:X4}",
            [trigger with { ScrollChange = [new(new(0, 0), (RoomScrollState)3)] }]), "invalid decoded scroll states are rejected");
        // A population holds any number of placements, in order (#1627); the room's places bound how many objects it holds.
        RoomObjectPlacement[] many = [.. Enumerable.Range(0, 300).Select(index => second with { Block = new(checked((ushort)(index % 8)), checked((ushort)(index / 8))) })];
        var large = new RoomObjectPopulation($"$8F:{0x9000:X4}", many);
        AssertEqual(300, large.Placements.Length, "a population keeps more than 255 placements");
        AssertTrue(large.Placements.ToArray().Select(placement => placement.Block).SequenceEqual(many.Select(placement => placement.Block)),
            "a large population keeps its placements in authored order");
        AssertThrows<InvalidDataException>(() => new RoomObjectPopulation($"$8F:{0x9000:X4}",
            [extension with { ScrollChange = Array.Empty<ScreenScrollChange>() }]), "an extension cannot own a resident scroll program");

        int count = 0;
        foreach (ushort pointer in RoomObjectPopulationDefinitions.Pointers)
            count += RoomObjectPopulationDefinitions.Get(pointer).Placements.Length;
        AssertEqual(RoomObjectPopulationDefinitions.RetailRecordCount, count,
            "every compiled population converts to the typed contract before gameplay");
        Console.WriteLine("PLM input boundary: 283 compiled populations/941 records, immutable writes, native slot reuse, RAM-only scroll execution, and bounded validation pass.");
    }

    /// <summary>Runs the focused compiled-population conversion and scroll execution check.</summary>
    [VerificationCommand("--plm-population-input-boundary")]
    internal static int RoomObjectPopulationInputBoundaryCommand()
    {
        Suite(nameof(VerifyObjectPopulationInputBoundary), () => VerifyObjectPopulationInputBoundary());
        return 0;
    }
}
