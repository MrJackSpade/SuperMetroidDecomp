using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyHandProgramControls(SuperMetroidAddressSpace rom) => VerifyHandProgramField(rom, 0);
    private static void VerifyHandProgramDraws(SuperMetroidAddressSpace rom) => VerifyHandProgramField(rom, 1);
    private static void VerifyHandProgramCallback(SuperMetroidAddressSpace rom) => VerifyHandProgramField(rom, 2);
    private static void VerifyHandProgramDebrisArguments(SuperMetroidAddressSpace rom) => VerifyHandProgramField(rom, 3);
    private static void VerifyHandProgramTransferSize(SuperMetroidAddressSpace rom) => VerifyHandProgramField(rom, 4);
    private static void VerifyHandProgramTransferSource(SuperMetroidAddressSpace rom) => VerifyHandProgramField(rom, 5);
    private static void VerifyHandProgramTransferDestination(SuperMetroidAddressSpace rom) => VerifyHandProgramField(rom, 6);

    private static void VerifyHandProgramField(SuperMetroidAddressSpace rom, int field)
    {
        // Actual supported-ROM boundaries, including the alignment change after
        // the three-byte source pointer. Some disassembly timing annotations lag
        // these offsets; original bytes remain the independent expected values.
        int[][] words = [
            [0xd368,0xd36c,0xd370,0xd372,0xd376,0xd37f,0xd383,0xd387,0xd38b,
                0xd38f,0xd393,0xd397,0xd39b,0xd39f,0xd3a3,0xd3a7,0xd3ab,
                0xd3af,0xd3b3,0xd3b7,0xd3bb,0xd3bf,0xd3c3,0xd3c5],
            [0xd36a,0xd374,0xd381,0xd389,0xd391,0xd399,0xd3a1,0xd3a9,0xd3b1,0xd3b9,0xd3c1],
            [0xd36e],
            [0xd385,0xd38d,0xd395,0xd39d,0xd3a5,0xd3ad,0xd3b5,0xd3bd],
            [0xd378],
            [],
            [0xd37d],
        ];
        var bytes = words.Select(group => group.SelectMany(address => new[] {address,address+1}).ToHashSet()).ToArray();
        bytes[5].UnionWith(new[] {0xd37a,0xd37b,0xd37c});
        AssertEqual(95, bytes.Sum(group => group.Count), "Hand field partition byte count");
        AssertTrue(bytes.SelectMany(group => group).Order().SequenceEqual(Enumerable.Range(0xd368,95)),
            "Hand field partition covers original interval exactly once");
        var selected = bytes[field];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool byteOwned = raw is >= 0xd368 and <= 0xd3c6;
            bool wordOwned = raw is >= 0xd368 and < 0xd3c6;
            AssertEqual(byteOwned, BombTorizoHandPlmProgramDefinitions.TryReadMechanicsByte(address, out byte b), "Hand complete byte domain");
            AssertEqual(wordOwned, BombTorizoHandPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort w), "Hand complete overlapping word domain");
            if (!byteOwned) AssertEqual((byte)0, b, "Hand missing byte output");
            else if (selected.Contains(raw))
            {
                byte expected = rom.ReadByte(0x840000 | raw);
                AssertEqual(expected, b, "Hand original program field byte");
                AssertTrue(RoomPlmProgramDefinitions.TryReadByte(address, out byte shared), "Hand composed byte reader");
                AssertEqual(expected, shared, "Hand composed original byte");
            }
            if (!wordOwned) AssertEqual((ushort)0, w, "Hand missing word output");
            else
            {
                int mask = (selected.Contains(raw) ? 0xff : 0) | (selected.Contains(raw + 1) ? 0xff00 : 0);
                if (mask == 0) continue;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | raw) & mask;
                AssertEqual(expected, w & mask, "Hand original field in overlapping word");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Hand composed word reader");
                AssertEqual(expected, shared & mask, "Hand composed original overlapping field");
            }
        }
    }
}
