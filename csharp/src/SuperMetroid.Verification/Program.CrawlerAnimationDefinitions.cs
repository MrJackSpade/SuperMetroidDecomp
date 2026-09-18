using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCrawlerAnimationDefinitions(SuperMetroidAddressSpace rom)
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
            AssertEqual(ReadCrawlerAnimationWord(rom, address + orientation * 2),
                CrawlerAnimationDefinitions.InitialInstruction(
                    family, orientations[orientation]),
                $"crawler initial selector {family}/{orientation}");
        }

        int[] surfaceTables = [0xa3e648, 0xa3e654, 0xa3e630, 0xa3e63c];
        for (ushort speciesOffset = 0; speciesOffset <= 10; speciesOffset += 2)
        for (int orientation = 0; orientation < 4; orientation++)
        {
            AssertEqual(ReadCrawlerAnimationWord(
                    rom, surfaceTables[orientation] + speciesOffset),
                CrawlerAnimationDefinitions.SurfaceInstruction(
                    speciesOffset, orientations[orientation]),
                $"crawler surface selector {speciesOffset}/{orientation}");
        }

        var initializeCrawlerMethod = typeof(RoomEnemySystem).GetMethod(
            "InitializeCrawler", flags)!;
        var initializeHZoomerMethod = typeof(RoomEnemySystem).GetMethod(
            "InitializeHZoomer", flags)!;
        foreach ((CrawlerAnimationFamily family, _, ushort? speciesOffset) in initialTables)
        for (ushort orientation = 0; orientation < 4; orientation++)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies, new CrawlerAnimationReadGuard(rom));
            RoomEnemySlot slot = enemies.Slots[0];
            slot.CurrentInstruction = orientation;
            slot.Parameter1 = 0;
            if (family == CrawlerAnimationFamily.HZoomer)
            {
                initializeHZoomerMethod.CreateDelegate<Action<RoomEnemySlot>>(enemies)(slot);
            }
            else
            {
                initializeCrawlerMethod
                    .CreateDelegate<Action<RoomEnemySlot, CrawlerAnimationFamily, ushort?>>(enemies)(
                        slot, family, speciesOffset);
            }

            AssertEqual(CrawlerAnimationDefinitions.InitialInstruction(
                    family, (CrawlerSurfaceOrientation)orientation),
                slot.CurrentInstruction,
                $"crawler production initial selector {family}/{orientation}");
        }

        var setVertical = typeof(RoomEnemySystem).GetMethod(
                "SetCrawlerVerticalSurfaceInstruction", flags)!
            .CreateDelegate<Action<RoomEnemySlot, CrawlerEnemyState>>();
        var setHorizontal = typeof(RoomEnemySystem).GetMethod(
                "SetCrawlerHorizontalSurfaceInstruction", flags)!
            .CreateDelegate<Action<RoomEnemySlot, CrawlerEnemyState>>();
        for (ushort speciesOffset = 0; speciesOffset <= 10; speciesOffset += 2)
        {
            var enemies = new RoomEnemySystem();
            RoomEnemySlot slot = enemies.Slots[0];
            slot.Parameter2 = speciesOffset;
            var state = new CrawlerEnemyState(slot);

            state.XVelocity = 0xffff;
            setHorizontal(slot, state);
            AssertEqual(CrawlerAnimationDefinitions.SurfaceInstruction(
                    speciesOffset, CrawlerSurfaceOrientation.UpsideRight),
                slot.CurrentInstruction, $"crawler production right surface {speciesOffset}");
            state.XVelocity = 1;
            setHorizontal(slot, state);
            AssertEqual(CrawlerAnimationDefinitions.SurfaceInstruction(
                    speciesOffset, CrawlerSurfaceOrientation.UpsideLeft),
                slot.CurrentInstruction, $"crawler production left surface {speciesOffset}");
            state.YVelocity = 0xffff;
            setVertical(slot, state);
            AssertEqual(CrawlerAnimationDefinitions.SurfaceInstruction(
                    speciesOffset, CrawlerSurfaceOrientation.UpsideDown),
                slot.CurrentInstruction, $"crawler production down surface {speciesOffset}");
            state.YVelocity = 1;
            setVertical(slot, state);
            AssertEqual(CrawlerAnimationDefinitions.SurfaceInstruction(
                    speciesOffset, CrawlerSurfaceOrientation.UpsideUp),
                slot.CurrentInstruction, $"crawler production up surface {speciesOffset}");
        }

        AssertThrows<InvalidDataException>(
            () => CrawlerAnimationDefinitions.InitialInstruction(
                (CrawlerAnimationFamily)5, CrawlerSurfaceOrientation.UpsideRight),
            "crawler initial family beyond authored tables");
        AssertThrows<InvalidDataException>(
            () => CrawlerAnimationDefinitions.SurfaceInstruction(
                1, CrawlerSurfaceOrientation.UpsideRight),
            "crawler odd species byte offset");
        AssertThrows<InvalidDataException>(
            () => CrawlerAnimationDefinitions.SurfaceInstruction(
                12, CrawlerSurfaceOrientation.UpsideRight),
            "crawler species byte offset beyond authored tables");
        Console.WriteLine(
            "Crawler animation definitions: forty-four native selectors and all family/orientation initializers plus 24 real surface handoffs pass with source tables forbidden.");
    }

    private static ushort ReadCrawlerAnimationWord(
        SuperMetroidAddressSpace source,
        int address) =>
        (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

    private sealed class CrawlerAnimationReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa396db and < 0xa396e3 or
                >= 0xa3992b and < 0xa39933 or
                >= 0xa3b667 and < 0xa3b66f or
                >= 0xa3e03b and < 0xa3e043 or
                >= 0xa3e2cc and < 0xa3e2d4 or
                >= 0xa3e630 and < 0xa3e660
                ? throw new InvalidOperationException(
                    $"Crawler attempted migrated animation-selector read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
