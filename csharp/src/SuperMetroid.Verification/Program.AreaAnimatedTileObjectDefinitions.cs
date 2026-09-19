using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>
    /// Compares all eight native list pointers and all 64 object selections to the pinned
    /// cartridge, then executes both production population owners while rejecting every
    /// read from those immutable bank-$83 sources.
    /// </summary>
    private static void VerifyAreaAnimatedTileObjectDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Area animated-tile definitions: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        for (int areaIndex = 0;
             areaIndex < AreaAnimatedTileObjectDefinitions.NativeAreaCount;
             areaIndex++)
        {
            ushort expectedList = AreaAnimatedTileObjectDefinitions.NativeListPointer(areaIndex);
            ushort actualList = RomDataReader.ReadWordFixedBank(
                rom,
                AreaAnimatedTileObjectDefinitions.NativeListPointerTable +
                    areaIndex * sizeof(ushort));
            AssertEqual(expectedList, actualList,
                $"area animated-tile list pointer {areaIndex}");

            for (int bit = 0;
                 bit < AreaAnimatedTileObjectDefinitions.ObjectsPerArea;
                 bit++)
            {
                ushort expectedObject =
                    AreaAnimatedTileObjectDefinitions.NativeObjectPointer(areaIndex, bit);
                ushort actualObject = RomDataReader.ReadWordFixedBank(
                    rom,
                    RoomFxRomData.Banks.RoomDefinitions |
                        unchecked((ushort)(actualList + bit * sizeof(ushort))));
                AssertEqual(expectedObject, actualObject,
                    $"area animated-tile object {areaIndex}/{bit}");
            }
        }

        VerifyCompiledAreaSelection(rom, AreaId.Maridia, 0x0c, expectedSand: 2,
            expectedTreadmills: 0);
        VerifyCompiledAreaSelection(rom, AreaId.WreckedShip, 0x0c, expectedSand: 0,
            expectedTreadmills: 2);
        AssertThrows<ArgumentOutOfRangeException>(
            () => AreaAnimatedTileObjectDefinitions.Read(AreaId.Crateria, 8),
            "animated-tile bit outside the native bitset fails loudly");

        Console.WriteLine(
            "  Area animated-tile definitions: 8 list pointers and 64 object " +
            "selectors are compiled; both production owners reject their ROM sources.");
    }

    private static void VerifyCompiledAreaSelection(
        ISnesAddressSpace rom,
        AreaId area,
        byte animatedTileBits,
        int expectedSand,
        int expectedTreadmills)
    {
        const ushort fxRecord = 0x9000;
        var fixture = new TestAddressSpace();
        fixture.WriteBytes(
            RoomFxRomData.Banks.RoomDefinitions | fxRecord,
            new byte[RoomFxRomData.Record.ByteCount]);
        fixture.WriteByte(
            RoomFxRomData.Banks.RoomDefinitions |
                unchecked((ushort)(fxRecord +
                    RoomFxRomData.Record.AnimatedTileBitsetOffset)),
            animatedTileBits);

        var guarded = new AreaAnimatedTileSelectionForbiddenBus(fixture, rom);
        var sand = new RoomSandAnimatedTilesState();
        var treadmills = new RoomTreadmillAnimatedTilesState();
        sand.LoadRoom(guarded, fxRecord, doorPointer: 0, area);
        treadmills.LoadRoom(guarded, fxRecord, doorPointer: 0, area);

        AssertEqual(expectedSand, sand.Count, $"{area} compiled sand selection");
        AssertEqual(expectedTreadmills, treadmills.Count,
            $"{area} compiled treadmill selection");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{area} selection performs no immutable table reads");
    }

    /// <summary>
    /// Serves the constructed FX record from a mutable fixture, delegates all other reads
    /// to the retail cartridge, and rejects exactly the compiled pointer/list bytes.
    /// </summary>
    private sealed class AreaAnimatedTileSelectionForbiddenBus(
        ISnesAddressSpace fixture,
        ISnesAddressSpace rom) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (IsCompiledSource(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled area animated-tile byte " +
                    $"{SnesAddress.FromBusAddress(address)}.");
            }

            SnesAddress source = SnesAddress.FromBusAddress(address);
            if (source.Bank == (byte)(RoomFxRomData.Banks.RoomDefinitions >> 16) &&
                source.Offset >= 0x9000 &&
                source.Offset < 0x9000 + RoomFxRomData.Record.ByteCount)
            {
                return fixture.ReadByte(address);
            }

            return rom.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => fixture.WriteByte(address, value);

        private static bool IsCompiledSource(int address)
        {
            if (address >= AreaAnimatedTileObjectDefinitions.NativeListPointerTable &&
                address < AreaAnimatedTileObjectDefinitions.NativeListPointerTable +
                    AreaAnimatedTileObjectDefinitions.NativeAreaCount * sizeof(ushort))
            {
                return true;
            }

            SnesAddress source = SnesAddress.FromBusAddress(address);
            if (source.Bank != (byte)(RoomFxRomData.Banks.RoomDefinitions >> 16))
                return false;

            for (int areaIndex = 0;
                 areaIndex < AreaAnimatedTileObjectDefinitions.NativeAreaCount;
                 areaIndex++)
            {
                ushort list = AreaAnimatedTileObjectDefinitions.NativeListPointer(areaIndex);
                if (source.Offset >= list &&
                    source.Offset < list +
                        AreaAnimatedTileObjectDefinitions.ObjectsPerArea * sizeof(ushort))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
