using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the compiled intro caret program against its retail bank-$8B instruction slice.</summary>
internal static class IntroCaretInstructionsVerification
{
    /// <summary>Compares bytes and overlapping words in $CBFB-$CC0E and rejects boundary-crossing words.</summary>
    internal static void VerifyIntroCaretInstructions(ISnesAddressSpace rom)
    {
        for (int pointer = 0xcbfb; pointer < 0xcc0f; pointer++)
        {
            AssertEqual(IntroCaretInstructionsRomByte(rom, 0x8b0000 + pointer), IntroCaretInstructionDefinitions.ReadByte((ushort)pointer),
                "original caret instruction byte");
            if (pointer == 0xcc0e) continue;
            AssertTrue(IntroCaretInstructionDefinitions.TryReadWord((ushort)pointer, out ushort word), "caret word is admitted");
            AssertEqual((ushort)(SnesLittleEndian.Word(IntroCaretInstructionsRomByte(rom, 0x8b0000 + pointer), IntroCaretInstructionsRomByte(rom, 0x8b0001 + pointer))), word,
                "original caret aligned or overlapping word");
        }
        foreach (ushort pointer in new ushort[] { 0, 0xcbfa, 0xcc0f, ushort.MaxValue })
        {
            AssertTrue(!IntroCaretInstructionDefinitions.TryReadWord(pointer, out ushort word), "other actor instructions are rejected");
            AssertEqual((ushort)0, word, "rejected caret word is zero");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroCaretInstructionDefinitions.ReadByte(pointer), "caret byte boundary");
        }
        AssertThrows<InvalidDataException>(() => IntroCaretInstructionDefinitions.TryReadWord(0xcc0e, out _),
            "caret terminal byte cannot begin a word");
    }

    /// <summary>Reads one cartridge byte only for the caret-program oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro caret instructions")]
    private static byte IntroCaretInstructionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
