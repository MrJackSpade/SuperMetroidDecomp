using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks cartridge bytes assigned to the glass program's control fields.</summary>
    /// <param name="rom">Retail address space used to verify each independently owned byte.</param>
    private static void VerifyGlassControls(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 0);

    /// <summary>Checks the glass program's spritemap drawing operands against the cartridge.</summary>
    /// <param name="rom">Retail address space containing the native operands.</param>
    private static void VerifyGlassDraws(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 1);

    /// <summary>Checks the target-selection words used by the glass program.</summary>
    /// <param name="rom">Retail address space containing the native target words.</param>
    private static void VerifyGlassTargets(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 2);

    /// <summary>Checks the native callback operand in the glass instruction sequence.</summary>
    /// <param name="rom">Retail address space used to verify the callback bytes.</param>
    private static void VerifyGlassCallback(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 3);

    /// <summary>Checks the glass program's event-related bytes against their cartridge locations.</summary>
    /// <param name="rom">Retail address space containing the native event operands.</param>
    private static void VerifyGlassEvents(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 4);

    /// <summary>Checks the boss-mask value read by the glass PLM program.</summary>
    /// <param name="rom">Retail address space used to verify the native mask.</param>
    private static void VerifyGlassBossMask(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 5);

    /// <summary>Checks the timing threshold words that divide the glass program's stages.</summary>
    /// <param name="rom">Retail address space containing the native thresholds.</param>
    private static void VerifyGlassThresholds(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 6);

    /// <summary>Checks the eight-byte argument blocks passed when glass shards are spawned.</summary>
    /// <param name="rom">Retail address space containing the native shard arguments.</param>
    private static void VerifyGlassShardArguments(SuperMetroidAddressSpace rom) => VerifyGlassField(rom, 7);

    /// <summary>Verifies one classified glass-program field over its full owned address domain.</summary>
    /// <param name="rom">Retail address space providing the expected native bytes and words.</param>
    /// <param name="field">Field category selected by the glass-specific verifier methods.</param>
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
