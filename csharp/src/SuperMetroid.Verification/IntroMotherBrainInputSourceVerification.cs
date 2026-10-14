using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro;

/// <summary>Checks the compiled intro Mother Brain input stream against its two bank-$91 source spans.</summary>
internal static class IntroMotherBrainInputSourceVerification
{
    /// <summary>Compares every native byte and overlapping word and verifies both stream boundaries reject out-of-range reads.</summary>
    internal static void VerifyIntroMotherBrainInputSource(ISnesAddressSpace rom)
    {
        foreach ((int start, int end) in new[] { (0x8694, 0x86fe), (0x8784, 0x878a) })
        {
            for (int pointer = start; pointer < end; pointer++)
            {
                AssertEqual(IntroMotherBrainInputSourceRomByte(rom, 0x910000 + pointer), IntroMotherBrainInputDefinitions.ReadByte((ushort)pointer), "original Mother Brain input byte");
                if (pointer == end - 1) continue;
                AssertEqual((ushort)(SnesLittleEndian.Word(IntroMotherBrainInputSourceRomByte(rom, 0x910000 + pointer), IntroMotherBrainInputSourceRomByte(rom, 0x910001 + pointer))),
                    IntroMotherBrainInputDefinitions.ReadWord((ushort)pointer), "original Mother Brain aligned or overlapping word");
            }
            AssertThrows<InvalidDataException>(() => IntroMotherBrainInputDefinitions.ReadWord((ushort)(end - 1)), "Mother Brain word cannot cross source-region boundary");
        }
        foreach (ushort pointer in new ushort[] { 0, 0x8693, 0x86fe, 0x8783, 0x878a, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => IntroMotherBrainInputDefinitions.ReadByte(pointer), "Mother Brain byte boundary");
            AssertThrows<InvalidDataException>(() => IntroMotherBrainInputDefinitions.ReadWord(pointer), "Mother Brain word boundary");
        }
    }

    /// <summary>Reads a native Mother Brain input byte only for the cartridge-oracle comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro Mother Brain input source")]
    private static byte IntroMotherBrainInputSourceRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
