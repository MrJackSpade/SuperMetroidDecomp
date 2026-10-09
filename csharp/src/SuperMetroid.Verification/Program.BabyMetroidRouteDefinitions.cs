using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Compares every compiled Baby Metroid route word with the pinned cartridge. The
    /// integrated cutscene fixture separately runs the complete route through a bus that
    /// rejects reads of this source range.
    /// </summary>
    private static void VerifyBabyMetroidRouteDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0; index < BabyMetroidRouteDefinitions.RecordCount; index++)
        {
            ushort pointer = unchecked((ushort)(
                BabyMetroidRouteDefinitions.FirstRecordPointer +
                index * BabyMetroidRouteDefinitions.RecordStride));
            int address = 0xa90000 | pointer;
            BabyMetroidRouteRecord record = BabyMetroidRouteDefinitions.GetRecord(pointer);

            AssertEqual(ReadRomWord(rom, address), record.TargetX,
                $"Baby route {index} target X");
            AssertEqual(ReadRomWord(rom, address + 2), record.TargetY,
                $"Baby route {index} target Y");
            AssertEqual(ReadRomWord(rom, address + 4), record.AccelerationDivisorIndex,
                $"Baby route {index} acceleration-divisor index");
            AssertEqual(ReadRomWord(rom, address + 6), record.MovementFunction,
                $"Baby route {index} movement callback");
            AssertEqual(ReadRomWord(rom, address + 8), record.FollowingWord,
                $"Baby route {index} overlapping following word");
        }

        AssertThrows<InvalidDataException>(
            () => BabyMetroidRouteDefinitions.GetRecord(0xca23),
            "Baby route rejects pointer before its definition range");
        AssertThrows<InvalidDataException>(
            () => BabyMetroidRouteDefinitions.GetRecord(0xca25),
            "Baby route rejects an unaligned definition pointer");
        AssertThrows<InvalidDataException>(
            () => BabyMetroidRouteDefinitions.GetRecord(0xca64),
            "Baby route rejects its terminal callback word as a record");
        AssertThrows<InvalidDataException>(
            () => BabyMetroidRouteDefinitions.GetWrongWayOffScreenXSpeed(0),
            "Baby route rejects an unknown movement callback");

        Console.WriteLine(
            "  Baby route definitions: 33 overlapping native words and callback identities agree.");
    }

    /// <summary>Reads a little-endian word from two consecutive cartridge-bus bytes.</summary>
    /// <param name="bus">Address space that supplies the word's bytes.</param>
    /// <param name="address">Address of the low byte; the high byte is read at the next address.</param>
    /// <returns>The combined 16-bit word.</returns>
    private static ushort ReadRomWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

    /// <summary>Wraps an address space to fail if execution reads the cartridge bytes used to compile Baby Metroid routes.</summary>
    /// <param name="source">Underlying address space for reads outside the compiled route source range and for all writes.</param>
    private sealed class BabyRouteReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-import reads through the guarded address-space read path.</summary>
        /// <param name="address">Address requested by the cartridge importer.</param>
        /// <returns>The byte supplied by the wrapped address space when the read is outside the compiled route source range.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the compiled route source range and forwards all other reads.</summary>
        /// <param name="address">Address of the byte requested from the wrapped address space.</param>
        /// <returns>The byte at an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The address falls within the cartridge range compiled into Baby Metroid route definitions.</exception>
        public byte ReadByte(int address)
        {
            if (address is >= BabyMetroidRouteDefinitionsConstants.SourceAddress and
                < BabyMetroidRouteDefinitionsConstants.SourceAddress +
                    BabyMetroidRouteDefinitionsConstants.SourceByteLength)
            {
                throw new InvalidOperationException(
                    $"Baby route still reads compiled ROM byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">Address to write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
