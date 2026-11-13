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
        RoomScrollWrite[] writes = [new(1, RoomScrollState.Green)];
        var trigger = new RoomObjectPlacement(RoomObjectHeaders.Get(RoomObjectHeaderId.ScrollTrigger),
            new(3, 3), null, writes);
        var extension = new RoomObjectPlacement(RoomObjectHeaders.Get(RoomObjectHeaderId.RightwardsScrollExtension),
            new(4, 3), null);
        var second = trigger with { Block = new(5, trigger.Block.Y), ScrollProgram = Array.Empty<RoomScrollWrite>() };
        RoomObjectPlacement[] records = [trigger, extension, second];
        var population = new RoomObjectPopulation($"$8F:{0xfffa:X4}", records);
        writes[0] = new(49, RoomScrollState.RedBoundary);
        records[0] = second;
        AssertEqual(new RoomScrollWrite(1, RoomScrollState.Green), population.Placements.Span[0].ScrollProgram![0],
            "decoded population owns its scroll writes independently of caller mutation");
        AssertEqual(3, population.Placements.Span[0].Block.X,
            "decoded population owns its record ordering independently of caller mutation");

        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var level = new RoomLevelData(new Extent16<RoomBlocks>(8, 8), new RoomLevelPlanes(new RoomTerrainBlock[64], new RoomBlockBehavior[64], new RoomBlockAppearance[64]), RoomLevelData.NothingBeyondGrid());
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertEqual(3, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(new SharedRandom()), AreaId.Crateria), population, new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "typed population runs the real sequential allocator using a RAM-only bus");
        AssertTrue(roomObjects.PopulationSlots.Select(slot => slot.ProcessingPosition).SequenceEqual(ReusedPopulationSlots),
            "extension deletion lets the next record reuse the first slot in processing order, in authored order");
        AssertEqual(RoomBlockBehaviorCodec.Encode(RoomBlockBehavior.Reacting(BlockReactionDefinitions.ScrollTrigger)), RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(3, 3)).Behavior),
            "typed trigger setup publishes its native collision behavior");
        AssertEqual((int)RoomCollisionType.HorizontalExtension, (int)level.GetCollisionBlock(new(4, 3)).CollisionType,
            "typed extension setup publishes its native collision type");
        var scrolls = RoomScrollGrid.CreateImplicit(RoomScrollGrid.PowerOnCells(), new Extent16<RoomScreens>(2, 2), RoomScrollState.RedBoundary);
        AssertTrue(roomObjects.Scrolls.TryNotifyScrollTouch(new(3, 3)), "typed trigger wakes at its placement");
        roomObjects.Step(level, scrolls);
        AssertEqual(RoomScrollState.Green, scrolls.ReadState(1), "decoded pairs mutate the intended live scroll cell");
        AssertTrue(!roomObjects.ScrollObjectTriggered(roomObjects.Scrolls.Snapshots[0].Block), "decoded trigger returns to sleep after its last write");

        AssertThrows<InvalidDataException>(() => new RoomObjectPopulation($"$8F:{0x9000:X4}",
            [trigger with { ScrollProgram = [new(RoomScrollGrid.KeptCellCount, RoomScrollState.Blue)] }]),
            "a decoded write past the scroll grid is rejected before allocation");
        AssertThrows<InvalidDataException>(() => new RoomObjectPopulation($"$8F:{0x9000:X4}",
            [trigger with { ScrollProgram = [new(0, (RoomScrollState)3)] }]), "invalid decoded scroll states are rejected");
        AssertThrows<InvalidDataException>(() => new RoomObjectPopulation($"$8F:{0x9000:X4}",
            Enumerable.Repeat(second, RoomObjectPopulation.MaximumPlacementCount + 1)),
            "a population cannot hold more placements than a room's population holds");
        AssertThrows<InvalidDataException>(() => new RoomObjectPopulation($"$8F:{0x9000:X4}",
            [extension with { ScrollProgram = Array.Empty<RoomScrollWrite>() }]), "an extension cannot own a resident scroll program");

        int count = 0;
        foreach (ushort pointer in RoomObjectPopulationDefinitions.Pointers)
            count += RoomObjectPopulationDefinitions.Get(pointer).Placements.Length;
        AssertEqual(RoomObjectPopulationDefinitions.RetailRecordCount, count,
            "every compiled population converts to the typed contract before gameplay");
        Console.WriteLine("PLM input boundary: 284 compiled populations/941 records, immutable writes, native slot reuse, RAM-only scroll execution, and bounded validation pass.");
    }

    /// <summary>Runs the focused compiled-population conversion and scroll execution check.</summary>
    [VerificationCommand("--plm-population-input-boundary")]
    internal static int RoomObjectPopulationInputBoundaryCommand()
    {
        Suite(nameof(VerifyObjectPopulationInputBoundary), () => VerifyObjectPopulationInputBoundary());
        return 0;
    }
}
