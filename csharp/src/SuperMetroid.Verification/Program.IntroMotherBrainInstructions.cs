using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares the compiled Mother Brain instruction bytes and aligned or overlapping words with bank-$8B, including rejection at source-region boundaries.</summary>
    /// <param name="rom">Retail address space containing the Mother Brain instruction region.</param>
    private static void VerifyIntroMotherBrainInstructions(ISnesAddressSpace rom)
    {
        foreach ((int start, int end) in new[] { (0xcb05, 0xcb33) })
        {
            for (int pointer = start; pointer < end; pointer++)
            {
                AssertEqual(rom.ReadByte(0x8b0000 + pointer), IntroMotherBrainInstructionDefinitions.ReadByte((ushort)pointer), "original Mother Brain instruction byte");
                if (pointer == end - 1) continue;
                AssertEqual((ushort)(rom.ReadByte(0x8b0000 + pointer) | rom.ReadByte(0x8b0001 + pointer) << 8),
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
}
