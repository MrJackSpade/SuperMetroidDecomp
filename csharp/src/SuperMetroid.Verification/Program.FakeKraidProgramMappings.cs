using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFakeKraidMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        // Independent native control-word addresses, including jump operands.
        ushort[] addresses =
        [
            0x99ac, 0x99ae, 0x99b2, 0x99b6, 0x99ba, 0x99be, 0x99c0, 0x99c2,
            0x99c4, 0x99c6, 0x99ca, 0x99cc, 0x99d0, 0x99d4, 0x99d8, 0x99da,
            0x99dc, 0x99e0, 0x99e2, 0x99e6, 0x99e8, 0x99ec, 0x99f0, 0x99f2,
            0x99fa, 0x99fc, 0x9a00, 0x9a04, 0x9a08, 0x9a0c, 0x9a0e, 0x9a10,
            0x9a12, 0x9a14, 0x9a18, 0x9a1a, 0x9a1e, 0x9a22, 0x9a26, 0x9a28,
            0x9a2a, 0x9a2e, 0x9a30, 0x9a34, 0x9a36, 0x9a3a, 0x9a3e, 0x9a40,
        ];
        AssertEqual(addresses.Length, FakeKraidInstructionProgramDefinitions.MechanicsWordCount,
            "Fake Kraid mechanics enumeration extent");
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = ReadFakeKraidInstructionWord(rom, 0xa60000 | address);
            var actual = FakeKraidInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(address, actual.Address, "Fake Kraid mechanics enumeration address");
            AssertEqual(expected, actual.Value, "Fake Kraid enumerated native control word");
            AssertEqual(expected, FakeKraidInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Fake Kraid direct native control word");
        }
        var words = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = words.Contains((ushort)(address & ~1));
            AssertEqual(expected,
                FakeKraidInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa60000 | address),
                "Fake Kraid complete bank byte ownership");
            AssertEqual(expected,
                FakeKraidInstructionProgramDefinitions.IsCompiledMechanicsByte(0x12a60000 | address),
                "Fake Kraid bank mask ignores bits above 24-bit address");
            AssertTrue(!FakeKraidInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa70000 | address),
                "Fake Kraid other bank rejects ownership");
        }
        for (int address = 0x99aa; address <= 0x9a48; address++)
        {
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(
                    () => FakeKraidInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                    "Fake Kraid rejects visual words, odd addresses and unused programs");
        }
        foreach (ushort address in new ushort[] { 0, ushort.MaxValue })
            AssertThrows<InvalidDataException>(
                () => FakeKraidInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Fake Kraid rejects distant mechanics addresses");
        foreach (int index in new[] { int.MinValue, -1, 48, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(
                () => FakeKraidInstructionProgramDefinitions.MechanicsWord(index),
                "Fake Kraid mechanics enumeration preserves bounds");
    }

    private static void VerifyFakeKraidPresentationAddresses()
    {
        ushort[] addresses =
        [
            0x99b0, 0x99b4, 0x99b8, 0x99bc, 0x99c8, 0x99ce, 0x99d2, 0x99d6,
            0x99de, 0x99e4, 0x99ea, 0x99ee, 0x99fe, 0x9a02, 0x9a06, 0x9a0a,
            0x9a16, 0x9a1c, 0x9a20, 0x9a24, 0x9a2c, 0x9a32, 0x9a38, 0x9a3c,
        ];
        AssertEqual(addresses.Length, FakeKraidInstructionProgramDefinitions.PresentationWordCount,
            "Fake Kraid presentation enumeration extent");
        for (int index = 0; index < addresses.Length; index++)
            AssertEqual(addresses[index], FakeKraidInstructionProgramDefinitions.PresentationWordAddress(index),
                "Fake Kraid native presentation operand position");
        foreach (int index in new[] { int.MinValue, -1, 24, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(
                () => FakeKraidInstructionProgramDefinitions.PresentationWordAddress(index),
                "Fake Kraid presentation enumeration preserves bounds");
    }
}
