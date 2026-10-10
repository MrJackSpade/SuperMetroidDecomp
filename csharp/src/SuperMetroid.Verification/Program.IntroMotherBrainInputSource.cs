using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies that the extracted Mother Brain input regions match the ROM and that reads
    /// reject pointers or words outside each region's bounds.
    /// </summary>
    /// <param name="rom">ROM address space containing the native bank-$91 input data.</param>
    private static void VerifyIntroMotherBrainInputSource(ISnesAddressSpace rom)
    {
        foreach ((int start, int end) in new[] { (0x8694, 0x86fe), (0x8784, 0x878a) })
        {
            for (int pointer = start; pointer < end; pointer++)
            {
                AssertEqual(rom.ReadByte(0x910000 + pointer), IntroMotherBrainInputDefinitions.ReadByte((ushort)pointer), "original Mother Brain input byte");
                if (pointer == end - 1) continue;
                AssertEqual((ushort)(rom.ReadByte(0x910000 + pointer) | rom.ReadByte(0x910001 + pointer) << 8),
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
}
