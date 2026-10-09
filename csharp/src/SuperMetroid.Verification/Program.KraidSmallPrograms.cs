using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks Kraid's lint mechanics words against cartridge data and validates their compiled byte range.</summary>
    /// <param name="rom">ROM address space supplying the independent expected instruction words.</param>
    private static void VerifyKraidLintMechanicsMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidSmallMechanics), () => VerifyKraidSmallMechanics(rom, [0x8afe, 0x8b02, 0x8b04, 0x8b08],
            KraidLintInstructionProgramDefinitions.MechanicsWordCount,
            index => { var word = KraidLintInstructionProgramDefinitions.MechanicsWord(index); return (word.Address, word.Value); },
            KraidLintInstructionProgramDefinitions.ReadMechanicsWord, KraidLintInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte));

    /// <summary>Checks Kraid's nail mechanics words against cartridge data and validates their compiled byte range.</summary>
    /// <param name="rom">ROM address space supplying the independent expected instruction words.</param>
    private static void VerifyKraidNailMechanicsMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidSmallMechanics), () => VerifyKraidSmallMechanics(rom, [0x8b0a, 0x8b0e, 0x8b12, 0x8b16, 0x8b1a, 0x8b1e, 0x8b22, 0x8b26, 0x8b2a, 0x8b2c],
            KraidNailInstructionProgramDefinitions.MechanicsWordCount,
            index => { var word = KraidNailInstructionProgramDefinitions.MechanicsWord(index); return (word.Address, word.Value); },
            KraidNailInstructionProgramDefinitions.ReadMechanicsWord, KraidNailInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte));

    /// <summary>Checks that lint presentation operands retain their native addresses and stay outside mechanics dispatch.</summary>
    private static void VerifyKraidLintPresentationMapping() =>
        Suite(nameof(VerifyKraidSmallPresentation), () => VerifyKraidSmallPresentation([0x8b00, 0x8b06], KraidLintInstructionProgramDefinitions.PresentationWordCount,
            KraidLintInstructionProgramDefinitions.PresentationWordAddress, KraidLintInstructionProgramDefinitions.ReadMechanicsWord));

    /// <summary>Checks that nail presentation operands retain their native addresses and stay outside mechanics dispatch.</summary>
    private static void VerifyKraidNailPresentationMapping() =>
        Suite(nameof(VerifyKraidSmallPresentation), () => VerifyKraidSmallPresentation([0x8b0c, 0x8b10, 0x8b14, 0x8b18, 0x8b1c, 0x8b20, 0x8b24, 0x8b28],
            KraidNailInstructionProgramDefinitions.PresentationWordCount,
            KraidNailInstructionProgramDefinitionsTooling.PresentationWordAddress, KraidNailInstructionProgramDefinitions.ReadMechanicsWord));

    // Independent original address lists from the pinned native programs;
    // all expected values come from the cartridge, not the replacement formulas.
    /// <summary>Compares an indexed Kraid mechanics program with ROM words and checks exact byte ownership and rejection cases.</summary>
    /// <param name="rom">ROM address space used to read the expected little-endian words.</param>
    /// <param name="addresses">Native bank-relative addresses of the program's mechanics words.</param>
    /// <param name="count">Number of mechanics words reported by the compiled program.</param>
    /// <param name="indexed">Returns the compiled address and value for a mechanics-word index.</param>
    /// <param name="read">Reads a mechanics word by its bank-relative address.</param>
    /// <param name="ownsByte">Reports whether the compiled byte classifier owns a full bank address.</param>
    /// <param name="bank">Bank containing the mechanics program.</param>
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

    /// <summary>Checks presentation operand offsets and confirms none is accepted as a mechanics word.</summary>
    /// <param name="expected">Native bank-relative presentation operand offsets.</param>
    /// <param name="count">Number of presentation words exposed by the compiled program.</param>
    /// <param name="actual">Returns the compiled presentation address for an index.</param>
    /// <param name="readMechanics">Mechanics reader used to verify presentation addresses are rejected.</param>
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
