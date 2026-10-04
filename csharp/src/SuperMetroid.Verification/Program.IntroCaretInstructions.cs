using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyIntroCaretInstructions(ISnesAddressSpace rom)
    {
        for (int pointer = 0xcbfb; pointer < 0xcc0f; pointer++)
        {
            AssertEqual(rom.ReadByte(0x8b0000 + pointer), IntroCaretInstructionDefinitions.ReadByte((ushort)pointer),
                "original caret instruction byte");
            if (pointer == 0xcc0e) continue;
            AssertTrue(IntroCaretInstructionDefinitions.TryReadWord((ushort)pointer, out ushort word), "caret word is admitted");
            AssertEqual((ushort)(rom.ReadByte(0x8b0000 + pointer) | rom.ReadByte(0x8b0001 + pointer) << 8), word,
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
}
