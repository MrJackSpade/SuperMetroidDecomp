using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGlassControls(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 0);
    private static void VerifyGlassDraws(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 1);
    private static void VerifyGlassTargets(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 2);
    private static void VerifyGlassCallback(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 3);
    private static void VerifyGlassEvents(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 4);
    private static void VerifyGlassBossMask(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 5);
    private static void VerifyGlassThresholds(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 6);
    private static void VerifyGlassShardArguments(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 7);

    private static void VerifyGlassField(SuperMetroidAddressSpace rom, int field)
    {
        // Independent native operand boundaries. Values come only from original ROM.
        int[] draws = [0xd213,0xd21d,0xd227,0xd23b,0xd249,0xd253,0xd25d,0xd271,0xd27f,0xd293,0xd2a1,0xd2b5,0xd2c3,0xd2d7,0xd2e5,0xd2ef,0xd2f5];
        int[] targets = [0xd205,0xd20b,0xd219,0xd223,0xd22d,0xd24f,0xd259,0xd263,0xd285,0xd2a7,0xd2c9];
        int[] thresholds = [0xd217,0xd221,0xd22b,0xd24d,0xd257,0xd261,0xd283,0xd2a5,0xd2c7];
        int[] bursts = [0xd231,0xd23f,0xd267,0xd275,0xd289,0xd297,0xd2ab,0xd2b9,0xd2cd,0xd2db];
        bool InWord(int address, int[] starts) => starts.Any(start => address == start || address == start + 1);
        int FieldAt(int a) => InWord(a, draws) ? 1 : InWord(a, targets) ? 2 :
            a is 0xd20f or 0xd210 ? 3 : a is 0xd209 or 0xd20a or 0xd2e9 or 0xd2ea ? 4 :
            a == 0xd204 ? 5 : InWord(a, thresholds) ? 6 : bursts.Any(first => a >= first && a < first + 8) ? 7 : 0;
        int count = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool ownsByte = raw is >= 0xd202 and <= 0xd2f8;
            bool ownsWord = raw is >= 0xd202 and < 0xd2f8;
            AssertEqual(ownsByte, MotherBrainGlassPlmProgramDefinitions.TryReadMechanicsByte(address, out byte b), "Glass full byte domain");
            AssertEqual(ownsWord, MotherBrainGlassPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort w), "Glass full word domain");
            if (!ownsByte) AssertEqual((byte)0, b, "Glass rejected byte output");
            else if (FieldAt(raw) == field)
            {
                count++;
                byte expected = rom.ReadByte(0x840000 | raw);
                AssertEqual(expected, b, "Glass native field byte");
                AssertTrue(RoomPlmProgramDefinitions.TryReadByte(address, out byte shared), "Glass composed byte ownership");
                AssertEqual(expected, shared, "Glass composed native byte");
            }
            if (!ownsWord) AssertEqual((ushort)0, w, "Glass rejected word output");
            else
            {
                int mask = (FieldAt(raw) == field ? 0xff : 0) | (FieldAt(raw + 1) == field ? 0xff00 : 0);
                if (mask == 0) continue;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | raw) & mask;
                AssertEqual(expected, w & mask, "Glass original overlapping field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Glass composed word ownership");
                AssertEqual(expected, shared & mask, "Glass composed original overlapping field");
            }
        }
        AssertEqual(field switch { 0 => 86, 1 => 34, 2 => 22, 3 => 2, 4 => 4, 5 => 1, 6 => 18, _ => 80 }, count,
            "Glass independent field byte count");
    }
}
