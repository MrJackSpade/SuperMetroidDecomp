using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGreyProgramControls(SuperMetroidAddressSpace rom) => VerifyGreyProgramField(rom, 0);
    private static void VerifyGreyProgramDraws(SuperMetroidAddressSpace rom) => VerifyGreyProgramField(rom, 1);
    private static void VerifyGreyProgramTargets(SuperMetroidAddressSpace rom) => VerifyGreyProgramField(rom, 2);
    private static void VerifyGreyProgramSounds(SuperMetroidAddressSpace rom) => VerifyGreyProgramField(rom, 3);
    private static void VerifyGreyProgramHitCount(SuperMetroidAddressSpace rom) => VerifyGreyProgramField(rom, 4);

    private static void VerifyGreyProgramCallback(SuperMetroidAddressSpace rom) => VerifyGreyProgramField(rom, 5);

    private static void VerifyGreyProgramField(SuperMetroidAddressSpace rom, int field)
    {
        // Independent native operand locations, not outputs of the proposed decoder.
        int[] origins = [0xbe59,0xbec2,0xbf2b,0xbf94];
        int[] drawOffsets = [2,6,13,17,21,35,53,57,61,65,69,73,89,93,97,101];
        int[] targetOffsets = [25,29,41,45,77,82];
        var drawBytes = origins.SelectMany(first => drawOffsets.SelectMany(offset => new[] {first+offset,first+offset+1})).ToHashSet();
        var targetBytes = origins.SelectMany(first => targetOffsets.SelectMany(offset => new[] {first+offset,first+offset+1})).ToHashSet();
        var soundBytes = origins.SelectMany(first => new[] {first+10,first+86}).ToHashSet();
        var hitBytes = origins.Select(first => first+81).ToHashSet();
        var callbackBytes = origins.SelectMany(first => new[] {first+49,first+50}).ToHashSet();
        int FieldAt(int a) => drawBytes.Contains(a) ? 1 : targetBytes.Contains(a) ? 2 :
            soundBytes.Contains(a) ? 3 : hitBytes.Contains(a) ? 4 : callbackBytes.Contains(a) ? 5 : 0;
        int checkedBytes = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool ownsByte = raw is >= 0xbe59 and <= 0xbffc;
            bool ownsWord = raw is >= 0xbe59 and < 0xbffc;
            AssertEqual(ownsByte, GreyDoorPlmProgramDefinitions.TryReadMechanicsByte(address, out byte b), "Grey program full byte domain");
            AssertEqual(ownsWord, GreyDoorPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort w), "Grey program full word domain");
            if (!ownsByte) AssertEqual((byte)0, b, "Grey program rejected byte output");
            else if (FieldAt(raw) == field)
            {
                checkedBytes++;
                byte expected = rom.ReadByte(0x840000 | raw);
                AssertEqual(expected, b, "Grey program original field byte");
                AssertTrue(RoomPlmProgramDefinitions.TryReadByte(address, out byte shared), "Grey program shared byte reader");
                AssertEqual(expected, shared, "Grey program composed original field");
            }
            if (!ownsWord) AssertEqual((ushort)0, w, "Grey program rejected word output");
            else
            {
                int mask = (FieldAt(raw) == field ? 0xff : 0) | (FieldAt(raw + 1) == field ? 0xff00 : 0);
                if (mask == 0) continue;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | raw) & mask;
                AssertEqual(expected, w & mask, "Grey program original overlapping field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Grey program shared word reader");
                AssertEqual(expected, shared & mask, "Grey program composed overlapping field");
            }
        }
        AssertEqual(field switch { 0 => 224, 1 => 128, 2 => 48, 3 => 8, 4 => 4, _ => 8 }, checkedBytes,
            "Grey program independent field byte count");
    }
}
