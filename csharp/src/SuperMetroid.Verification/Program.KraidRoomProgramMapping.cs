using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static class KraidRoomProgramMappingVerification
{
    internal static void VerifyKraidRoomProgramMapping(SuperMetroidAddressSpace rom)
    {
        // Independent native lists. The byte timer shifts the loop onto even addresses;
        // the seven-byte machine-code callback is not part of any instruction list.
        (ushort Start, int Words)[] lists =
        [
            (0xab6d, 6), (0xab79, 3), (0xab7f, 6), (0xab8b, 3),
            (0xab91, 6), (0xab9d, 3), (0xaba3, 3), (0xaba9, 1),
            (0xabac, 21), (0xabdd, 3),
        ];
        var expected = new SortedDictionary<ushort, ushort>();
        foreach (var list in lists)
            for (int word = 0; word < list.Words; word++)
            {
                ushort address = (ushort)(list.Start + word * 2);
                expected.Add(address, (ushort)(SnesLittleEndian.Word(KraidRoomProgramMappingRomByte(rom, 0x840000 | address), KraidRoomProgramMappingRomByte(rom, 0x840000 | (address + 1)))));
            }
        ushort[] enumerated = KraidRoomPlmProgramDefinitions.NativeWordAddresses().ToArray();
        AssertEqual(55, enumerated.Length, "Kraid room program word count");
        AssertTrue(expected.Keys.SequenceEqual(enumerated), "Kraid room exact native word enumeration");
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool exists = expected.TryGetValue((ushort)address, out ushort expectedWord);
            AssertEqual(exists, KraidRoomPlmProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort actual),
                "Kraid room complete word address domain");
            AssertEqual(expectedWord, actual, "Kraid room native word or zero on rejected address");
            bool isTimer = address == 0xabab;
            AssertEqual(isTimer, KraidRoomPlmProgramDefinitions.TryReadMechanicsByte((ushort)address, out byte actualByte),
                "Kraid room byte timer domain");
            AssertEqual(isTimer ? KraidRoomProgramMappingRomByte(rom, 0x84abab) : (byte)0, actualByte,
                "Kraid room original timer byte or zero on rejection");
        }
        Console.WriteLine("Kraid room programs: 55 native words, byte timer, exact enumeration and complete address domains pass.");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid room program mapping")]
    private static byte KraidRoomProgramMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
