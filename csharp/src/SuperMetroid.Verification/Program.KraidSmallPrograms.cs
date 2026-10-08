using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidLintMechanicsMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidSmallMechanics), () => VerifyKraidSmallMechanics(rom, [0x8afe, 0x8b02, 0x8b04, 0x8b08],
            KraidLintInstructionProgramDefinitions.MechanicsWordCount,
            index => { var word = KraidLintInstructionProgramDefinitions.MechanicsWord(index); return (word.Address, word.Value); },
            KraidLintInstructionProgramDefinitions.ReadMechanicsWord, KraidLintInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte));

    private static void VerifyKraidNailMechanicsMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidSmallMechanics), () => VerifyKraidSmallMechanics(rom, [0x8b0a, 0x8b0e, 0x8b12, 0x8b16, 0x8b1a, 0x8b1e, 0x8b22, 0x8b26, 0x8b2a, 0x8b2c],
            KraidNailInstructionProgramDefinitions.MechanicsWordCount,
            index => { var word = KraidNailInstructionProgramDefinitions.MechanicsWord(index); return (word.Address, word.Value); },
            KraidNailInstructionProgramDefinitions.ReadMechanicsWord, KraidNailInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte));

    private static void VerifyKraidLintPresentationMapping() =>
        Suite(nameof(VerifyKraidSmallPresentation), () => VerifyKraidSmallPresentation([0x8b00, 0x8b06], KraidLintInstructionProgramDefinitions.PresentationWordCount,
            KraidLintInstructionProgramDefinitions.PresentationWordAddress, KraidLintInstructionProgramDefinitions.ReadMechanicsWord));

    private static void VerifyKraidNailPresentationMapping() =>
        Suite(nameof(VerifyKraidSmallPresentation), () => VerifyKraidSmallPresentation([0x8b0c, 0x8b10, 0x8b14, 0x8b18, 0x8b1c, 0x8b20, 0x8b24, 0x8b28],
            KraidNailInstructionProgramDefinitions.PresentationWordCount,
            KraidNailInstructionProgramDefinitionsTooling.PresentationWordAddress, KraidNailInstructionProgramDefinitions.ReadMechanicsWord));

    // Independent original address lists from the pinned native programs;
    // all expected values come from the cartridge, not the replacement formulas.
    private static void VerifyKraidSmallMechanics(SuperMetroidAddressSpace rom, ushort[] addresses, int count,
        Func<int, (ushort Address, ushort Value)> indexed, Func<ushort, ushort> read, Func<int, bool> ownsByte, int bank = 0xa7)
    {
        AssertEqual(addresses.Length, count, "small Kraid mechanics count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            int full = bank << 16 | address;
            ushort expected = (ushort)(rom.ReadByte(full) | rom.ReadByte(full + 1) << 8);
            AssertEqual((address, expected), indexed(index), "native small Kraid mechanics address/value");
            AssertEqual(expected, read(address), "native small Kraid mechanics read");
            bytes.Add(address);
            bytes.Add(address + 1);
            AssertThrows<InvalidDataException>(() => read((ushort)(address + 1)), "odd mechanics address rejected");
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(bytes.Contains(address), ownsByte(bank << 16 | address), "small Kraid mechanics byte domain");
        AssertTrue(!ownsByte((bank ^ 1) << 16 | addresses[0]), "wrong mechanics bank rejected");
        AssertThrows<InvalidDataException>(() => read((ushort)(addresses[^1] + 2)), "adjacent program excluded");
        AssertThrows<IndexOutOfRangeException>(() => indexed(-1), "negative mechanics index");
        AssertThrows<IndexOutOfRangeException>(() => indexed(addresses.Length), "mechanics index past end");
    }

    private static void VerifyKraidSmallPresentation(ushort[] expected, int count,
        Func<int, ushort> actual, Func<ushort, ushort> readMechanics)
    {
        AssertEqual(expected.Length, count, "small Kraid presentation count");
        for (int index = 0; index < expected.Length; index++)
        {
            ushort address = expected[index];
            AssertEqual(address, actual(index), "native small Kraid presentation offset");
            AssertThrows<InvalidDataException>(() => readMechanics(address), "presentation is not mechanics");
        }
        AssertThrows<IndexOutOfRangeException>(() => actual(-1), "negative presentation index");
        AssertThrows<IndexOutOfRangeException>(() => actual(expected.Length), "presentation index past end");
    }
}
