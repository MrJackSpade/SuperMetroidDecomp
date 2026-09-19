using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyExploredMapPackingDefinitions()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort Word(int address) => unchecked((ushort)(
            retail.ReadByte(address) | retail.ReadByte(address + 1) << 8));

        int exportedByteCount = 0;
        for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
        {
            ExploredMapPackingDefinition definition =
                ExploredMapPackingDefinitions.Area(area);
            ReadOnlySpan<byte> indexes = definition.AreaByteIndexes.Span;
            AssertEqual(retail.ReadByte(
                    ExploredMapPackingDefinitions.NativeByteCountTable + area),
                unchecked((byte)indexes.Length),
                $"packed map area {area} byte count");
            AssertEqual(Word(
                    ExploredMapPackingDefinitions.NativeDestinationOffsetTable + area * 2),
                definition.DestinationOffset,
                $"packed map area {area} SRAM offset");
            AssertEqual(Word(
                    ExploredMapPackingDefinitions.NativeSourcePointerTable + area * 2),
                definition.NativeSourcePointer,
                $"packed map area {area} source pointer");
            for (int index = 0; index < indexes.Length; index++)
            {
                AssertEqual(retail.ReadByte(0x810000 |
                        unchecked((ushort)(definition.NativeSourcePointer + index))),
                    indexes[index],
                    $"packed map area {area} byte index {index}");
            }
            exportedByteCount += indexes.Length;
        }
        AssertEqual(327, exportedByteCount, "packed map exported byte count");
        AssertThrows<ArgumentOutOfRangeException>(
            () => ExploredMapPackingDefinitions.Area(SaveRamLayout.PackedMapAreaCount),
            "packed map excludes native Ceres list");

        var guard = new ExploredMapPackingReadGuard(retail);
        var saveRam = new SuperMetroidSaveRam(guard);
        var explored = new byte[
            Bank80SystemState.ExploredMapAreaCount *
            Bank80SystemState.ExploredMapBytesPerArea];
        for (int index = 0; index < explored.Length; index++)
            explored[index] = unchecked((byte)(index * 37 + 11));
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot { ExploredMapBytes = explored });
        SuperMetroidSaveSlot restored = saveRam.ReadSlot(0) ??
            throw new InvalidOperationException("Compiled explored-map save failed checksums.");

        var exported = new bool[explored.Length];
        for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
        {
            foreach (byte areaByteIndex in
                ExploredMapPackingDefinitions.Area(area).AreaByteIndexes.Span)
            {
                exported[area * Bank80SystemState.ExploredMapBytesPerArea +
                    areaByteIndex] = true;
            }
        }
        for (int index = 0; index < explored.Length; index++)
        {
            AssertEqual(exported[index] ? explored[index] : (byte)0,
                restored.ExploredMapBytes[index],
                $"packed map production round trip byte {index}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "save/load avoids compiled explored-map codec tables");

        Console.WriteLine(
            "  Explored-map packing: six area records and all 327 exported byte indexes match the cartridge; production save/load round-trips with the native codec tables forbidden.");
    }

    private sealed class ExploredMapPackingReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (IsForbidden(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Save-map codec reread compiled byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static bool IsForbidden(int address)
        {
            if (address is >= 0x818131 and < 0x818146 or
                >= 0x8182d6 and < 0x8182e4)
                return true;

            for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
            {
                ExploredMapPackingDefinition definition =
                    ExploredMapPackingDefinitions.Area(area);
                int start = 0x810000 | definition.NativeSourcePointer;
                if (address >= start &&
                    address < start + definition.AreaByteIndexes.Length)
                    return true;
            }
            return false;
        }
    }
}
