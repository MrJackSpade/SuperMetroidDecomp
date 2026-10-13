using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Checks Kraid lint and nail mechanics boundaries separately from their visual operands.</summary>
internal static class KraidSmallProgramsVerification
{
    /// <summary>Checks the four compiled Kraid lint control words against native addresses and values.</summary>
    internal static void VerifyKraidLintMechanicsMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidSmallMechanics), () => VerifyKraidSmallMechanics(rom, [0x8afe, 0x8b02, 0x8b04, 0x8b08],
            KraidLintInstructionProgramDefinitions.MechanicsWordCount,
            index => { var word = KraidLintInstructionProgramDefinitions.MechanicsWord(index); return (word.Address, word.Value); },
            KraidLintInstructionProgramDefinitions.ReadMechanicsWord, KraidLintInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte));

    /// <summary>Checks the ten compiled Kraid nail control words against native addresses and values.</summary>
    internal static void VerifyKraidNailMechanicsMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidSmallMechanics), () => VerifyKraidSmallMechanics(rom, [0x8b0a, 0x8b0e, 0x8b12, 0x8b16, 0x8b1a, 0x8b1e, 0x8b22, 0x8b26, 0x8b2a, 0x8b2c],
            KraidNailInstructionProgramDefinitions.MechanicsWordCount,
            index => { var word = KraidNailInstructionProgramDefinitions.MechanicsWord(index); return (word.Address, word.Value); },
            KraidNailInstructionProgramDefinitions.ReadMechanicsWord, KraidNailInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte));

    /// <summary>Checks the two Kraid lint visual offsets remain outside compiled mechanics.</summary>
    internal static void VerifyKraidLintPresentationMapping() =>
        Suite(nameof(VerifyKraidSmallPresentation), () => VerifyKraidSmallPresentation([0x8b00, 0x8b06], KraidLintInstructionProgramDefinitions.PresentationWordCount,
            KraidLintInstructionProgramDefinitions.PresentationWordAddress, KraidLintInstructionProgramDefinitions.ReadMechanicsWord));

    /// <summary>Checks eight Kraid nail visual offsets remain outside compiled mechanics.</summary>
    internal static void VerifyKraidNailPresentationMapping() =>
        Suite(nameof(VerifyKraidSmallPresentation), () => VerifyKraidSmallPresentation([0x8b0c, 0x8b10, 0x8b14, 0x8b18, 0x8b1c, 0x8b20, 0x8b24, 0x8b28],
            KraidNailInstructionProgramDefinitions.PresentationWordCount,
            KraidNailInstructionProgramDefinitionsTooling.PresentationWordAddress, KraidNailInstructionProgramDefinitions.ReadMechanicsWord));

    /// <summary>Checks indexed mechanics words, odd-address rejection, and complete byte ownership against native data.</summary>
    /// <remarks>Original address lists come from pinned native programs; expected values come from the cartridge.</remarks>
    internal static void VerifyKraidSmallMechanics(SuperMetroidAddressSpace rom, ushort[] addresses, int count,
        Func<int, (ushort Address, ushort Value)> indexed, Func<ushort, ushort> read, Func<int, bool> ownsByte, int bank = 0xa7)
    {
        AssertEqual(addresses.Length, count, "small Kraid mechanics count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            int full = ((int)new SnesAddress(checked((byte)bank), address));
            ushort expected = (ushort)(SnesLittleEndian.Word(KraidSmallMechanicsRomByte(rom, full), KraidSmallMechanicsRomByte(rom, full + 1)));
            AssertEqual((address, expected), indexed(index), "native small Kraid mechanics address/value");
            AssertEqual(expected, read(address), "native small Kraid mechanics read");
            bytes.Add(address);
            bytes.Add(address + 1);
            AssertThrows<InvalidDataException>(() => read((ushort)(address + 1)), "odd mechanics address rejected");
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(bytes.Contains(address), ownsByte(((int)new SnesAddress(checked((byte)bank), checked((ushort)address)))), "small Kraid mechanics byte domain");
        AssertTrue(!ownsByte(((int)new SnesAddress(checked((byte)(bank ^ 1)), addresses[0]))), "wrong mechanics bank rejected");
        AssertThrows<InvalidDataException>(() => read((ushort)(addresses[^1] + 2)), "adjacent program excluded");
        AssertThrows<IndexOutOfRangeException>(() => indexed(-1), "negative mechanics index");
        AssertThrows<IndexOutOfRangeException>(() => indexed(addresses.Length), "mechanics index past end");
    }

    /// <summary>Reads a native Kraid control byte for the independent word comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid small mechanics")]
    private static byte KraidSmallMechanicsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks presentation offsets, index bounds, and rejection by the mechanics reader.</summary>
    internal static void VerifyKraidSmallPresentation(ushort[] expected, int count,
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
