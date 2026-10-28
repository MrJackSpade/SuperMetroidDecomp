using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks typed PLM population ownership and execution on a RAM-only bus.</summary>
internal static class PlmPopulationInputBoundaryVerification
{
    /// <summary>Native slots the typed population occupies after extension deletion frees the highest slot.</summary>
    private static readonly int[] ReusedPopulationSlots =
        [39, 38];

    /// <summary>Verifies the identified typed-input conversion, without importing a ROM or driving gameplay.</summary>
    internal static void VerifyPlmPopulationInputBoundary()
    {
        byte[] pairs = [1, (byte)RoomScrollState.Green, 0x80];
        var trigger = new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(PlmHeaderId.ScrollTrigger),
            new(3, 3), 0x9100, pairs);
        var extension = new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(PlmHeaderId.RightwardsScrollExtension),
            new(4, 3), 0x8000);
        var second = trigger with { Block = new(5, trigger.Block.Y), ScrollProgram = new byte[] { 0x80 } };
        RoomPlmPlacement[] records = [trigger, extension, second];
        var population = new RoomPlmPopulationDefinition($"$8F:{0xfffa:X4}", records);
        pairs[0] = 49;
        pairs[1] = 0;
        records[0] = second;
        AssertEqual((byte)1, population.Placements.Span[0].ScrollProgram.Span[0],
            "decoded population owns its scroll pairs independently of caller mutation");
        AssertEqual(3, population.Placements.Span[0].Block.X,
            "decoded population owns its record ordering independently of caller mutation");

        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var level = new RoomLevelData(new Extent16<RoomBlocks>(8, 8), new RoomLevelPlanes(new ushort[64], new byte[64], new ushort[64]));
        var plms = PlmFixtures.Unbound();
        AssertEqual(3, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), AreaId.Crateria), population, new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "typed population runs the real sequential allocator using a RAM-only bus");
        AssertTrue(plms.PopulationSlots.Select(slot => slot.NativeSlotIndex).SequenceEqual(ReusedPopulationSlots),
            "extension deletion permits immediate highest-slot reuse in authored order");
        AssertEqual(RoomBlockBehaviorValues.ScrollTrigger.Value, level.GetCollisionBlock(new(3, 3)).Bts.Value,
            "typed trigger setup publishes its native collision behavior");
        AssertEqual((int)RoomCollisionType.HorizontalExtension, (int)level.GetCollisionBlock(new(4, 3)).CollisionType,
            "typed extension setup publishes its native collision type");
        var scrolls = RoomScrollGrid.CreateImplicit(new RoomScrollStorage(), new Extent16<RoomScreens>(2, 2), RoomScrollState.RedBoundary);
        AssertTrue(plms.Scrolls.TryNotifyScrollTouch(level.GetBlockIndex(new(3, 3))), "typed trigger wakes at its placement");
        plms.Step(level, scrolls);
        AssertEqual((byte)RoomScrollState.Green, scrolls.ReadStorage(1), "decoded pairs mutate the intended live scroll cell");
        AssertTrue(!plms.ScrollPlmTriggered(plms.Scrolls.Snapshots[0].BlockIndex), "decoded trigger returns to native sleep after its terminator");

        AssertThrows<InvalidDataException>(() => new RoomPlmPopulationDefinition($"$8F:{0x9000:X4}",
            [trigger with { ScrollProgram = new byte[] { 0, 2 } }]), "unterminated decoded pairs are rejected before allocation");
        AssertThrows<InvalidDataException>(() => new RoomPlmPopulationDefinition($"$8F:{0x9000:X4}",
            [trigger with { ScrollProgram = new byte[] { 0, 3, 0x80 } }]), "invalid decoded scroll states are rejected");
        AssertThrows<InvalidDataException>(() => new RoomPlmPopulationDefinition($"$8F:{0x9000:X4}",
            Enumerable.Repeat(second, RoomPlmPopulationFormat.MaximumParserIterations)),
            "typed records cannot evade the native bounded terminator limit");
        AssertThrows<InvalidDataException>(() => new RoomPlmPopulationDefinition($"$8F:{0x9000:X4}",
            [extension with { ScrollProgram = new byte[] { 0x80 } }]), "an extension cannot own a resident scroll program");

        int count = 0;
        foreach (ushort pointer in RoomPlmPopulationDefinitions.Pointers)
            count += RoomPlmPopulationDefinitions.Get(pointer).Placements.Length;
        AssertEqual(RoomPlmPopulationDefinitions.RetailRecordCount, count,
            "every compiled population converts to the typed contract before gameplay");
        Console.WriteLine("PLM input boundary: 284 compiled populations/941 records, immutable pairs, native slot reuse, RAM-only scroll execution, and bounded validation pass.");
    }

    /// <summary>Runs the focused compiled-population conversion and scroll execution check.</summary>
    [VerificationCommand("--plm-population-input-boundary")]
    internal static int PlmPopulationInputBoundaryCommand()
    {
        Suite(nameof(VerifyPlmPopulationInputBoundary), () => VerifyPlmPopulationInputBoundary());
        return 0;
    }
}
