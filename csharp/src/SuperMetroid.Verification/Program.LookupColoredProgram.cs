using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyColoredProgramControls(SuperMetroidAddressSpace rom) => VerifyColoredProgramField(rom, 0);
    private static void VerifyColoredProgramDraws(SuperMetroidAddressSpace rom) => VerifyColoredProgramField(rom, 1);
    private static void VerifyColoredProgramTargets(SuperMetroidAddressSpace rom) => VerifyColoredProgramField(rom, 2);
    private static void VerifyColoredProgramSounds(SuperMetroidAddressSpace rom) => VerifyColoredProgramField(rom, 3);
    private static void VerifyColoredProgramHitCount(SuperMetroidAddressSpace rom) => VerifyColoredProgramField(rom, 4);

    private static void VerifyColoredProgramCallback(SuperMetroidAddressSpace rom) => VerifyColoredProgramField(rom, 5);

    private static void VerifyColoredProgramField(SuperMetroidAddressSpace rom, int field)
    {
        // Independent native operand locations, not outputs of the proposed decoder.
        int[] origins = [0xbffd,0xc060,0xc0c3,0xc122,0xc185,0xc1e4,0xc243,0xc2a2,0xc301,0xc363,0xc3c5,0xc427];
        int[] hits = [0xc02a,0xc08d,0xc0ec,0xc14f,0xc1ae,0xc20d,0xc26c,0xc2cb,0xc32a,0xc38c,0xc3ee,0xc450];
        int[] flashes = [0xc02f,0xc092,0xc0f1,0xc154,0xc1b3,0xc212,0xc271,0xc2d0,0xc332,0xc394,0xc3f6,0xc458];
        int[] openings = [0xc04b,0xc0ae,0xc10d,0xc170,0xc1cf,0xc22e,0xc28d,0xc2ec,0xc34e,0xc3b0,0xc412,0xc474];
        var drawWords = origins.SelectMany(first => new[] {2,6,13,17,21,37}.Select(offset => first+offset))
            .Concat(flashes.SelectMany(first => new[] {2,6,10,14,18,22}.Select(offset => first+offset)))
            .Concat(openings.SelectMany(first => new[] {5,9,13,17}.Select(offset => first+offset))).Append(0xc14b);
        var targets = origins.SelectMany(first => new[] {first+25,first+29})
            .Concat(hits.Select(first => first+3)).Concat(flashes.Select(first => first+26)).Concat(new[] {0xc028,0xc08b});
        var drawBytes = drawWords.SelectMany(a => new[] {a,a+1}).ToHashSet();
        var targetBytes = targets.SelectMany(a => new[] {a,a+1}).ToHashSet();
        var soundBytes = origins.Select(first => first+10).Concat(openings.Select(first => first+2))
            .Concat(new[] {0xc331,0xc393,0xc3f5,0xc457}).ToHashSet();
        var hitBytes = hits.Select(first => first+2).ToHashSet();
        var callbackBytes = origins.SelectMany(first => new[] {first+33,first+34}).ToHashSet();
        int FieldAt(int a) => drawBytes.Contains(a) ? 1 : targetBytes.Contains(a) ? 2 :
            soundBytes.Contains(a) ? 3 : hitBytes.Contains(a) ? 4 : callbackBytes.Contains(a) ? 5 : 0;
        int checkedBytes = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool ownsByte = raw is >= 0xbffd and <= 0xc488;
            bool ownsWord = raw is >= 0xbffd and <= 0xc488 and not (0xc184 or 0xc300 or 0xc488);
            AssertEqual(ownsByte, ColoredDoorPlmProgramDefinitions.TryReadMechanicsByte(address, out byte b), "Colored program full byte domain");
            AssertEqual(ownsWord, ColoredDoorPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort w), "Colored program full word domain");
            if (!ownsByte) AssertEqual((byte)0, b, "Colored program rejected byte output");
            else if (FieldAt(raw) == field)
            {
                checkedBytes++;
                byte expected = rom.ReadByte(0x840000 | raw);
                AssertEqual(expected, b, "Colored program original field byte");
                AssertTrue(RoomPlmProgramDefinitions.TryReadByte(address, out byte shared), "Colored program shared byte reader");
                AssertEqual(expected, shared, "Colored program composed original field");
            }
            if (!ownsWord) AssertEqual((ushort)0, w, "Colored program rejected word output");
            else
            {
                int mask = (FieldAt(raw) == field ? 0xff : 0) | (FieldAt(raw + 1) == field ? 0xff00 : 0);
                if (mask == 0) continue;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | raw) & mask;
                AssertEqual(expected, w & mask, "Colored program original overlapping field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Colored program shared word reader");
                AssertEqual(expected, shared & mask, "Colored program composed overlapping field");
            }
        }
        AssertEqual(field switch { 0 => 614, 1 => 386, 2 => 100, 3 => 28, 4 => 12, _ => 24 }, checkedBytes,
            "Colored program independent field byte count");
    }
}
