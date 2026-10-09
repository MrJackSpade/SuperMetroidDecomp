using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks every decoded Torizo-door control byte and overlapping word against the cartridge and shared PLM reader.</summary>
    private static void VerifyTorizoDoorControls(SuperMetroidAddressSpace rom) => VerifyTorizoDoorField(rom, 0);
    /// <summary>Checks every decoded Torizo-door draw pointer against its independently identified cartridge operand.</summary>
    private static void VerifyTorizoDoorDraws(SuperMetroidAddressSpace rom) => VerifyTorizoDoorField(rom, 1);
    /// <summary>Checks every decoded Torizo-door target pointer against its independently identified cartridge operand.</summary>
    private static void VerifyTorizoDoorTargets(SuperMetroidAddressSpace rom) => VerifyTorizoDoorField(rom, 2);
    /// <summary>Checks the native Torizo-door sound operand against its cartridge byte and shared PLM reader.</summary>
    private static void VerifyTorizoDoorSounds(SuperMetroidAddressSpace rom) => VerifyTorizoDoorField(rom, 3);
    /// <summary>Checks the Torizo-door hit-count operand against its cartridge byte and shared PLM reader.</summary>
    private static void VerifyTorizoDoorHitCount(SuperMetroidAddressSpace rom) => VerifyTorizoDoorField(rom, 4);

    /// <summary>Checks the Torizo-door callback operands against their cartridge bytes and shared PLM reader.</summary>
    private static void VerifyTorizoDoorCallback(SuperMetroidAddressSpace rom) => VerifyTorizoDoorField(rom, 5);

    private static void VerifyTorizoDoorField(SuperMetroidAddressSpace rom, int field)
    {
        // Independent native operand locations, not outputs of the proposed decoder.
        int[] draws = [0xba4e,0xba56,0xba5d,0xba61,0xba65,0xba69,
            0xba8b,0xba9d,0xbaa1,0xbaa5,0xbaa9,0xbaad,0xbab1,0xbac1,0xbac5,0xbac9,0xbacd];
        int[] targets = [0xba52,0xba6d,0xba81,0xba85,0xba91,0xba95,0xbab5,0xbaba];
        var drawBytes = draws.SelectMany(a => new[] {a, a + 1}).ToHashSet();
        var targetBytes = targets.SelectMany(a => new[] {a, a + 1}).ToHashSet();
        int FieldAt(int a) => drawBytes.Contains(a) ? 1 : targetBytes.Contains(a) ? 2 :
            a is 0xba5a or 0xbabe ? 3 : a == 0xbab9 ? 4 : a is 0xba99 or 0xba9a ? 5 : 0;
        int checkedBytes = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool ownsByte = raw is >= 0xba4c and <= 0xba6e or >= 0xba7f and <= 0xbad0;
            bool ownsWord = raw is >= 0xba4c and < 0xba6e or >= 0xba7f and < 0xbad0;
            AssertEqual(ownsByte, BombTorizoGreyDoorPlmProgramDefinitions.TryReadMechanicsByte(address, out byte b), "Torizo door full byte domain");
            AssertEqual(ownsWord, BombTorizoGreyDoorPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort w), "Torizo door full word domain");
            if (!ownsByte) AssertEqual((byte)0, b, "Torizo door rejected byte output");
            else if (FieldAt(raw) == field)
            {
                checkedBytes++;
                byte expected = rom.ReadByte(0x840000 | raw);
                AssertEqual(expected, b, "Torizo door original field byte");
                AssertTrue(RoomPlmProgramDefinitions.TryReadByte(address, out byte shared), "Torizo door shared byte reader");
                AssertEqual(expected, shared, "Torizo door composed original field");
            }
            if (!ownsWord) AssertEqual((ushort)0, w, "Torizo door rejected word output");
            else
            {
                int mask = (FieldAt(raw) == field ? 0xff : 0) | (FieldAt(raw + 1) == field ? 0xff00 : 0);
                if (mask == 0) continue;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | raw) & mask;
                AssertEqual(expected, w & mask, "Torizo door original overlapping field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Torizo door shared word reader");
                AssertEqual(expected, shared & mask, "Torizo door composed overlapping field");
            }
        }
        AssertEqual(field switch { 0 => 62, 1 => 34, 2 => 16, 3 => 2, 4 => 1, _ => 2 }, checkedBytes,
            "Torizo door independent field byte count");
    }
}
