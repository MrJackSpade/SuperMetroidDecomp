using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares the compiled scientist instruction regions with ROM bytes and verifies that byte and word reads enforce their source-region bounds.</summary>
    /// <param name="rom">Retail address space containing the native scientist instruction sequences.</param>
    private static void VerifyIntroScientistInstructions(ISnesAddressSpace rom)
    {
        foreach ((int start, int end) in new[] { (0xcb9f, 0xcbcd), (0xcbcd, 0xcc2b), (0xce53, 0xce55) })
        {
            for (int pointer = start; pointer < end; pointer++)
            {
                AssertEqual(rom.ReadByte(0x8b0000 + pointer), IntroScientistInstructionDefinitions.ReadByte((ushort)pointer), "original scientist instruction byte");
                if (pointer == end - 1) continue;
                AssertEqual((ushort)(rom.ReadByte(0x8b0000 + pointer) | rom.ReadByte(0x8b0001 + pointer) << 8),
                    IntroScientistInstructionDefinitions.ReadWord((ushort)pointer), "original scientist aligned or overlapping word");
            }
            AssertThrows<InvalidDataException>(() => IntroScientistInstructionDefinitions.ReadWord((ushort)(end - 1)), "scientist word cannot cross source-region boundary");
        }
        foreach (ushort pointer in new ushort[] { 0, 0xcb9e, 0xcc2b, 0xce52, 0xce55, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => IntroScientistInstructionDefinitions.ReadByte(pointer), "scientist byte boundary");
            AssertThrows<InvalidDataException>(() => IntroScientistInstructionDefinitions.ReadWord(pointer), "scientist word boundary");
        }
    }
}
