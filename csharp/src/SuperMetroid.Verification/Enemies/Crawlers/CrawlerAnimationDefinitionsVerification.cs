using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>Checks crawler orientation tables and their live initialization and surface-switch selection.</summary>
internal static class CrawlerAnimationDefinitionsVerification
{
    /// <summary>Compares crawler orientation tables with cartridge words and production initial/surface instruction selection.</summary>
    internal static void VerifyCrawlerAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        (CrawlerAnimationFamily Family, int Address, ushort? Offset)[] initialTables =
        [
            (CrawlerAnimationFamily.Shared, 0xa3e2cc, null),
            (CrawlerAnimationFamily.Viola, 0xa3b667, 6),
            (CrawlerAnimationFamily.Sciser, 0xa396db, 8),
            (CrawlerAnimationFamily.Zero, 0xa3992b, 10),
            (CrawlerAnimationFamily.HZoomer, 0xa3e03b, null),
        ];
        CrawlerSurfaceOrientation[] orientations =
        [
            CrawlerSurfaceOrientation.UpsideRight,
            CrawlerSurfaceOrientation.UpsideLeft,
            CrawlerSurfaceOrientation.UpsideDown,
            CrawlerSurfaceOrientation.UpsideUp,
        ];

        foreach ((CrawlerAnimationFamily family, int address, _) in initialTables)
        for (int orientation = 0; orientation < 4; orientation++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + orientation * 2)),
                NativeEnemyProgramOracle.EntryOf(CrawlerNativeListAddresses.Entries, CrawlerAnimationDefinitions.InitialProgram(
                    family, orientations[orientation])),
                $"crawler initial selector {family}/{orientation}");
        }

        int[] surfaceTables = [0xa3e648, 0xa3e654, 0xa3e630, 0xa3e63c];
        for (ushort speciesOffset = 0; speciesOffset <= 10; speciesOffset += 2)
        for (int orientation = 0; orientation < 4; orientation++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((surfaceTables[orientation] + speciesOffset)),
                NativeEnemyProgramOracle.EntryOf(CrawlerNativeListAddresses.Entries, CrawlerAnimationDefinitions.SurfaceProgram(
                    speciesOffset, orientations[orientation])),
                $"crawler surface selector {speciesOffset}/{orientation}");
        }

        foreach ((CrawlerAnimationFamily family, _, ushort? speciesOffset) in initialTables)
        for (ushort orientation = 0; orientation < 4; orientation++)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(NewCrawlerAnimationReadGuard(rom));
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(family == CrawlerAnimationFamily.HZoomer ? EnemyDefinitionId.HZoomer : EnemyDefinitionId.Zoomer, slot.Definition);
            slot.CurrentInstruction = orientation;
            if (family == CrawlerAnimationFamily.Viola)
            {
                // InitAI_Viola ($A3:B678) selects with the property bits, not the parameter.
                slot.CurrentInstruction = (ushort)(orientation ^ 1);
                slot.Properties = orientation;
            }
            slot.Parameter1 = 0;
            if (family == CrawlerAnimationFamily.HZoomer)
            {
                new Action<RoomEnemySlot>(CrawlersBehavior.InitializeHZoomer)(slot);
            }
            else
            {
                new Action<RoomEnemySlot, CrawlerAnimationFamily, ushort?>(CrawlersBehavior.InitializeCrawler)(
                        slot, family, speciesOffset);
            }

            AssertEqual(AnimationProgramCursor<CrawlerProgram>.Start(CrawlerAnimationDefinitions.InitialProgram(
                    family, (CrawlerSurfaceOrientation)orientation)),
                (enemies.CrawlerStates[0] ?? throw new InvalidOperationException("The crawler has no state.")).Program,
                $"crawler production initial selector {family}/{orientation}");
        }

        Action<RoomEnemySlot, CrawlerEnemyState> setVertical = CrawlersBehavior.SetCrawlerVerticalSurfaceInstruction;
        Action<RoomEnemySlot, CrawlerEnemyState> setHorizontal = CrawlersBehavior.SetCrawlerHorizontalSurfaceInstruction;
        for (ushort speciesOffset = 0; speciesOffset <= 10; speciesOffset += 2)
        {
            var enemies = new RoomEnemySystem();
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Parameter2 = speciesOffset;
            var state = new CrawlerEnemyState(slot);

            state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(0xffff));
            setHorizontal(slot, state);
            AssertEqual(AnimationProgramCursor<CrawlerProgram>.Start(CrawlerAnimationDefinitions.SurfaceProgram(
                    speciesOffset, CrawlerSurfaceOrientation.UpsideRight)),
                state.Program, $"crawler production right surface {speciesOffset}");
            state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(1));
            setHorizontal(slot, state);
            AssertEqual(AnimationProgramCursor<CrawlerProgram>.Start(CrawlerAnimationDefinitions.SurfaceProgram(
                    speciesOffset, CrawlerSurfaceOrientation.UpsideLeft)),
                state.Program, $"crawler production left surface {speciesOffset}");
            state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(0xffff));
            setVertical(slot, state);
            AssertEqual(AnimationProgramCursor<CrawlerProgram>.Start(CrawlerAnimationDefinitions.SurfaceProgram(
                    speciesOffset, CrawlerSurfaceOrientation.UpsideDown)),
                state.Program, $"crawler production down surface {speciesOffset}");
            state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(1));
            setVertical(slot, state);
            AssertEqual(AnimationProgramCursor<CrawlerProgram>.Start(CrawlerAnimationDefinitions.SurfaceProgram(
                    speciesOffset, CrawlerSurfaceOrientation.UpsideUp)),
                state.Program, $"crawler production up surface {speciesOffset}");
        }

        AssertThrows<InvalidDataException>(
            () => CrawlerAnimationDefinitions.InitialProgram(
                (CrawlerAnimationFamily)5, CrawlerSurfaceOrientation.UpsideRight),
            "crawler initial family beyond authored tables");
        AssertThrows<InvalidDataException>(
            () => CrawlerAnimationDefinitions.SurfaceProgram(
                1, CrawlerSurfaceOrientation.UpsideRight),
            "crawler odd species byte offset");
        AssertThrows<InvalidDataException>(
            () => CrawlerAnimationDefinitions.SurfaceProgram(
                12, CrawlerSurfaceOrientation.UpsideRight),
            "crawler species byte offset beyond authored tables");
        Console.WriteLine(
            "Crawler animation definitions: forty-four native selectors and all family/orientation initializers plus 24 real surface handoffs pass with source tables forbidden.");
    }

    /// <summary>Forbids crawler animation-selector table reads while exercising production behavior.</summary>
    private static CartridgeReadGuard NewCrawlerAnimationReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa396db and < 0xa396e3 or
                >= 0xa3992b and < 0xa39933 or
                >= 0xa3b667 and < 0xa3b66f or
                >= 0xa3e03b and < 0xa3e043 or
                >= 0xa3e2cc and < 0xa3e2d4 or
                >= 0xa3e630 and < 0xa3e660,
                address => $"Crawler attempted migrated animation-selector read ${address:X6}.");
}
