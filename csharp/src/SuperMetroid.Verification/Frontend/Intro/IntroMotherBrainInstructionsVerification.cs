using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks compiled intro Mother Brain instructions against the pinned bank-$8B slice.</summary>
internal static class IntroMotherBrainInstructionsVerification
{
    /// <summary>Compares bytes and overlapping words in $CB05-$CB32 and rejects out-of-slice reads.</summary>
    internal static void VerifyIntroMotherBrainInstructions(ISnesAddressSpace rom)
    {
        foreach ((int start, int end) in new[] { (0xcb05, 0xcb33) })
        {
            for (int pointer = start; pointer < end; pointer++)
            {
                AssertEqual(IntroMotherBrainInstructionsRomByte(rom, 0x8b0000 + pointer), IntroMotherBrainInstructionDefinitions.ReadByte((ushort)pointer), "original Mother Brain instruction byte");
                if (pointer == end - 1) continue;
                AssertEqual((ushort)(SnesLittleEndian.Word(IntroMotherBrainInstructionsRomByte(rom, 0x8b0000 + pointer), IntroMotherBrainInstructionsRomByte(rom, 0x8b0001 + pointer))),
                    IntroMotherBrainInstructionDefinitions.ReadWord((ushort)pointer), "original Mother Brain aligned or overlapping word");
            }
            AssertThrows<InvalidDataException>(() => IntroMotherBrainInstructionDefinitions.ReadWord((ushort)(end - 1)), "Mother Brain word cannot cross source-region boundary");
        }
        foreach (ushort pointer in new ushort[] { 0, 0xcb04, 0xcb33, ushort.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => IntroMotherBrainInstructionDefinitions.ReadByte(pointer), "Mother Brain byte boundary");
            AssertThrows<InvalidDataException>(() => IntroMotherBrainInstructionDefinitions.ReadWord(pointer), "Mother Brain word boundary");
        }
    }

    /// <summary>Reads one native byte for the cartridge-only comparison path.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro Mother Brain instructions")]
    private static byte IntroMotherBrainInstructionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
