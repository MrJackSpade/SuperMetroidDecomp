using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks compiled control instructions against native Chozo statue program bytes.</summary>
    /// <param name="rom">Cartridge address space containing the original PLM program.</param>
    private static void VerifyChozoProgramControls(SuperMetroidAddressSpace rom) => VerifyChozoProgramField(rom, 0);
    /// <summary>Checks compiled spritemap operands against their native draw-pointer bytes.</summary>
    /// <param name="rom">Cartridge address space containing the original PLM program.</param>
    private static void VerifyChozoProgramDraws(SuperMetroidAddressSpace rom) => VerifyChozoProgramField(rom, 1);
    /// <summary>Checks compiled instruction branch targets against the native target words.</summary>
    /// <param name="rom">Cartridge address space containing the original PLM program.</param>
    private static void VerifyChozoProgramTargets(SuperMetroidAddressSpace rom) => VerifyChozoProgramField(rom, 2);
    /// <summary>Checks the compiled callback operand against the native callback word.</summary>
    /// <param name="rom">Cartridge address space containing the original PLM program.</param>
    private static void VerifyChozoProgramCallback(SuperMetroidAddressSpace rom) => VerifyChozoProgramField(rom, 3);
    /// <summary>Checks compiled event operands against the native event word.</summary>
    /// <param name="rom">Cartridge address space containing the original PLM program.</param>
    private static void VerifyChozoProgramEvent(SuperMetroidAddressSpace rom) => VerifyChozoProgramField(rom, 4);

    /// <summary>Checks byte and word ownership across the full address domain and compares a selected program field with ROM.</summary>
    /// <param name="rom">Cartridge address space used to read expected native program bytes.</param>
    /// <param name="field">Field selector: controls, draw operands, branch targets, callback, or event.</param>
    private static void VerifyChozoProgramField(SuperMetroidAddressSpace rom, int field)
    {
        int[] draws = [0xd0f8,0xd0fc,0xd100,0xd104,0xd151,0xd3d1,0xd3ee];
        int FieldAt(int a) => draws.Any(start => a == start || a == start + 1) ? 1 :
            a is 0xd143 or 0xd144 ? 2 : a is 0xd147 or 0xd148 ? 3 : a is 0xd141 or 0xd142 ? 4 : 0;
        int count = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool ownsByte = raw is >= 0xd0f6 and <= 0xd107 or >= 0xd13f and <= 0xd154 or >= 0xd3cf and <= 0xd3d6 or >= 0xd3ec and <= 0xd3f3;
            bool ownsWord = ownsByte && raw is not (0xd107 or 0xd154 or 0xd3d6 or 0xd3f3);
            AssertEqual(ownsByte, ChozoStatuePlmProgramDefinitions.TryReadMechanicsByte(address, out byte b), "ChozoProgram byte domain");
            AssertEqual(ownsWord, ChozoStatuePlmProgramDefinitions.TryReadMechanicsWord(address, out ushort w), "ChozoProgram word domain");
            if (!ownsByte) AssertEqual((byte)0, b, "ChozoProgram rejected byte output");
            else if (FieldAt(raw) == field)
            {
                count++;
                byte expected = rom.ReadByte(0x840000 | raw);
                AssertEqual(expected, b, "ChozoProgram native field byte");
                AssertTrue(RoomPlmProgramDefinitions.TryReadByte(address, out byte shared), "ChozoProgram composed byte ownership");
                AssertEqual(expected, shared, "ChozoProgram composed native byte");
            }
            if (!ownsWord) AssertEqual((ushort)0, w, "ChozoProgram rejected word output");
            else
            {
                int mask = (FieldAt(raw) == field ? 0xff : 0) | (FieldAt(raw + 1) == field ? 0xff00 : 0);
                if (mask == 0) continue;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | raw) & mask;
                AssertEqual(expected, w & mask, "ChozoProgram native overlapping field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "ChozoProgram composed word ownership");
                AssertEqual(expected, shared & mask, "ChozoProgram composed overlapping field");
            }
        }
        AssertEqual(field switch {0 => 36,1 => 14,2 => 2,3 => 2,_ => 2}, count, "ChozoProgram independent field count");
    }
}
