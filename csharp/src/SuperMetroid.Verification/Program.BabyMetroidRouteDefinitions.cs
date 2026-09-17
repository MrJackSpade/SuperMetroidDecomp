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
        var rom = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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

    private static ushort ReadRomWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

    private sealed class BabyRouteReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= BabyMetroidRouteDefinitions.SourceAddress and
                < BabyMetroidRouteDefinitions.SourceAddress +
                    BabyMetroidRouteDefinitions.SourceByteLength)
            {
                throw new InvalidOperationException(
                    $"Baby route still reads compiled ROM byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
