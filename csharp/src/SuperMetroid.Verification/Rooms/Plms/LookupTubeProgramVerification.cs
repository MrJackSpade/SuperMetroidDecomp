using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>Checks Noob Tube PLM program fields and full lookup domains against bank-$84 data.</summary>
internal static class LookupTubeProgramVerification
{
    /// <summary>Checks tube control words outside separately classified operands.</summary>
    internal static void VerifyTubeControls(SuperMetroidAddressSpace rom) => VerifyTubeField(rom, 0);
    /// <summary>Checks tube drawing operands against native instruction words.</summary>
    internal static void VerifyTubeDraws(SuperMetroidAddressSpace rom) => VerifyTubeField(rom, 1);
    /// <summary>Checks tube branch targets against native instruction words.</summary>
    internal static void VerifyTubeTargets(SuperMetroidAddressSpace rom) => VerifyTubeField(rom, 2);
    /// <summary>Checks the two tube callback operands against native words.</summary>
    internal static void VerifyTubeCallbacks(SuperMetroidAddressSpace rom) => VerifyTubeField(rom, 3);
    /// <summary>Checks the two tube event operands against native words.</summary>
    internal static void VerifyTubeEvents(SuperMetroidAddressSpace rom) => VerifyTubeField(rom, 4);
    /// <summary>Checks the tube's one-byte sound operand against bank $84.</summary>
    internal static void VerifyTubeSound(SuperMetroidAddressSpace rom) => VerifyTubeField(rom, 5);

    /// <summary>Compares one tube operand family over the complete sixteen-bit lookup domain.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Tube field")]
    private static void VerifyTubeField(SuperMetroidAddressSpace rom, int field)
    {
        // Native instruction boundaries, independently transcribed from bank_84.asm.
        int[] words = Enumerable.Range(0, 25).Select(i => 0xd4d4 + 2 * i)
            .Concat(Enumerable.Range(0, 9).Select(i => 0xd507 + 2 * i))
            .Concat(new[] { 0xd521, 0xd523 }).ToArray();
        int FieldAt(int address) => address switch
        {
            0xd4e4 or 0xd4fa or 0xd4fe or 0xd502 or 0xd50d => 1,
            0xd4d8 or 0xd4dc or 0xd4ea => 2,
            0xd4e0 or 0xd4ee => 3,
            0xd4d6 or 0xd511 => 4,
            _ => 0,
        };
        AssertTrue(words.SequenceEqual(NoobTubePlmProgramDefinitions.MechanicsWordAddresses().Select(a => (int)a)),
            "Tube ordered word inventory");
        AssertTrue(NoobTubePlmProgramDefinitions.MechanicsByteAddresses().SequenceEqual(new ushort[] { 0xd506 }),
            "Tube ordered byte inventory");
        int count = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool ownsWord = words.Contains(raw);
            AssertEqual(ownsWord, NoobTubePlmProgramDefinitions.TryReadMechanicsWord(address, out ushort word), "Tube full word domain");
            AssertEqual(raw == 0xd506, NoobTubePlmProgramDefinitions.TryReadMechanicsByte(address, out byte b), "Tube full byte domain");
            if (!ownsWord) AssertEqual((ushort)0, word, "Tube rejected word output");
            else if (FieldAt(raw) == field)
            {
                count++;
                ushort expected = ReadSamusEaterPlmWord(rom, 0x840000 | raw);
                AssertEqual(expected, word, "Tube original field word");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Tube composed word ownership");
                AssertEqual(expected, shared, "Tube composed original field word");
            }
            if (raw != 0xd506) AssertEqual((byte)0, b, "Tube rejected byte output");
            else if (field == 5)
            {
                count++;
                byte expected = rom.ReadCartridgeByte(0x84d506);
                AssertEqual(expected, b, "Tube original sound byte");
                AssertTrue(RoomPlmProgramDefinitions.TryReadByte(address, out byte shared), "Tube composed sound ownership");
                AssertEqual(expected, shared, "Tube composed original sound byte");
            }
        }
        AssertEqual(field switch { 0 => 24, 1 => 5, 2 => 3, 3 => 2, 4 => 2, _ => 1 }, count,
            "Tube independent field count");
    }
}
