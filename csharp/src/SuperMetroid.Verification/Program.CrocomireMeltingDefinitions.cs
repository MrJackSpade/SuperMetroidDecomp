using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCrocomireMeltingDefinitions(SuperMetroidAddressSpace rom)
    {
        const int columnTable = 0xa49697;
        const int maskTable = 0xa49bbd;
        for (int cursor = 0; cursor < CrocomireMeltingDefinitions.ColumnCount; cursor++)
        {
            AssertEqual(rom.ReadByte(columnTable + cursor),
                CrocomireMeltingDefinitions.SelectColumn(cursor),
                $"Crocomire melt column {cursor}");
            AssertEqual(rom.ReadByte(maskTable + (cursor & 7)),
                CrocomireMeltingDefinitions.SelectMask(cursor),
                $"Crocomire melt mask {cursor}");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => CrocomireMeltingDefinitions.SelectColumn(-1),
            "Crocomire negative melt cursor");
        AssertThrows<ArgumentOutOfRangeException>(
            () => CrocomireMeltingDefinitions.SelectColumn(49),
            "Crocomire melt cursor after authored table");

        VerifyCrocomireMeltingProductionSequence(rom);
        Console.WriteLine(
            "Crocomire melting definitions: all 49 column selectors, eight masks, " +
            "the complete production erase sequence, and the terminal cursor guard pass " +
            "with both source tables forbidden.");
    }

    private static void VerifyCrocomireMeltingProductionSequence(
        SuperMetroidAddressSpace rom)
    {
        const int meltingTable = 0xa49bc5;
        const ushort syntheticTableOffset = 0x0100;
        var memory = new TestAddressSpace();
        for (int record = 0; record < CrocomireMeltingDefinitions.ColumnCount; record++)
        {
            int address = meltingTable + syntheticTableOffset + record * 8;
            memory.WriteBytes(address,
                [0x02, 0x00, 0x00, 0x00, 0x7e, 0x00, 0x00, 0x40]);
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        var death = new CrocomireDeathState
        {
            MeltingTableOffset = syntheticTableOffset,
            PixelsToErasePerColumn = 48,
            TargetHeightOrSkeletonTileIndex = 48,
        };
        death.MutableMeltingGraphics.Fill(0xff);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new CrocomireMeltingDefinitionReadGuard(memory));
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, new SnesVram());
        typeof(RoomEnemySystem).GetField("_crocomireDeath", flags)!.SetValue(enemies, death);
        var erase = typeof(RoomEnemySystem).GetMethod(
            "EraseNextCrocomireMeltingColumn",
            flags)!.CreateDelegate<Func<bool>>(enemies);

        var expectedGraphics = Enumerable.Repeat((byte)0xff,
            CrocomireDeathState.MeltingGraphicsByteCount).ToArray();
        var expectedHeights = new byte[CrocomireDeathState.MeltingColumnCount];
        for (int cursor = 0; cursor < CrocomireMeltingDefinitions.ColumnCount; cursor++)
        {
            int xColumn = rom.ReadByte(0xa49697 + cursor);
            byte mask = rom.ReadByte(0xa49bbd + (cursor & 7));
            for (int remaining = 48; remaining != 0; remaining--)
            {
                int y = expectedHeights[xColumn];
                int byteIndex = 2 * (y & 7) + ((y & ~7) << 6) + 4 * (xColumn & ~7);
                expectedGraphics[byteIndex] &= mask;
                expectedGraphics[byteIndex + 1] &= mask;
                expectedGraphics[byteIndex + 16] &= mask;
                expectedGraphics[byteIndex + 17] &= mask;
                expectedHeights[xColumn]++;
            }

            AssertTrue(erase(), $"Crocomire melt production cursor {cursor} succeeds");
            AssertEqual((ushort)cursor, death.MeltingColumnCursor,
                $"Crocomire melt production cursor {cursor}");
            AssertEqual((byte)48, death.MeltingColumnHeights[xColumn],
                $"Crocomire melt production column {xColumn} height");
        }

        AssertTrue(death.MeltingGraphics.SequenceEqual(expectedGraphics),
            "Crocomire melt production bitplane silhouette");
        AssertTrue(death.MeltingColumnHeights.SequenceEqual(expectedHeights),
            "Crocomire melt production column heights");
        AssertTrue(erase(), "Crocomire melt post-table guard retains distortion phase");
        AssertEqual((ushort)CrocomireMeltingDefinitions.ColumnCount,
            death.MeltingColumnCursor,
            "Crocomire melt post-table cursor");
    }

    private sealed class CrocomireMeltingDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is
            >= 0xa49697 and < 0xa496c8 or
            >= 0xa49bbd and < 0xa49bc5
                ? throw new InvalidOperationException(
                    $"Crocomire melting attempted migrated definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
