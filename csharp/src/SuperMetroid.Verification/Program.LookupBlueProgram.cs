using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBlueProgramControls(SuperMetroidAddressSpace rom) => VerifyBlueProgramField(rom, 0);
    private static void VerifyBlueProgramDraws(SuperMetroidAddressSpace rom) => VerifyBlueProgramField(rom, 1);
    private static void VerifyBlueProgramSounds(SuperMetroidAddressSpace rom) => VerifyBlueProgramField(rom, 2);
    private static void VerifyBlueProgramBts(SuperMetroidAddressSpace rom) => VerifyBlueProgramField(rom, 3);

    private static void VerifyBlueProgramField(SuperMetroidAddressSpace rom, int field)
    {
        // Independent native operand locations, not outputs of the proposed decoder.
        int[] origins = [0xc489,0xc4ba,0xc4eb,0xc51c];
        int[] drawOffsets = [5,9,13,17,23,27,34,38,45];
        var drawBytes = origins.SelectMany(first => drawOffsets.SelectMany(offset => new[] {first+offset,first+offset+1})).ToHashSet();
        var soundBytes = origins.SelectMany(first => new[] {first+2,first+31}).ToHashSet();
        var btsBytes = origins.Select(first => first+42).ToHashSet();
        int FieldAt(int a) => drawBytes.Contains(a) ? 1 : soundBytes.Contains(a) ? 2 : btsBytes.Contains(a) ? 3 : 0;
        int checkedBytes = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool ownsByte = raw is >= 0xc489 and <= 0xc54c;
            bool ownsWord = raw is >= 0xc489 and < 0xc54c;
            AssertEqual(ownsByte, BlueDoorPlmProgramDefinitions.TryReadMechanicsByte(address, out byte b), "Blue program full byte domain");
            AssertEqual(ownsWord, BlueDoorPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort w), "Blue program full word domain");
            if (!ownsByte) AssertEqual((byte)0, b, "Blue program rejected byte output");
            else if (FieldAt(raw) == field)
            {
                checkedBytes++;
                byte expected = rom.ReadByte(0x840000 | raw);
                AssertEqual(expected, b, "Blue program original field byte");
                AssertTrue(RoomPlmProgramDefinitions.TryReadByte(address, out byte shared), "Blue program shared byte reader");
                AssertEqual(expected, shared, "Blue program composed original field");
            }
            if (!ownsWord) AssertEqual((ushort)0, w, "Blue program rejected word output");
            else
            {
                int mask = (FieldAt(raw) == field ? 0xff : 0) | (FieldAt(raw + 1) == field ? 0xff00 : 0);
                if (mask == 0) continue;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | raw) & mask;
                AssertEqual(expected, w & mask, "Blue program original overlapping field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Blue program shared word reader");
                AssertEqual(expected, shared & mask, "Blue program composed overlapping field");
            }
        }
        AssertEqual(field switch { 0 => 112, 1 => 72, 2 => 8, _ => 4 }, checkedBytes,
            "Blue program independent field byte count");
    }
}
