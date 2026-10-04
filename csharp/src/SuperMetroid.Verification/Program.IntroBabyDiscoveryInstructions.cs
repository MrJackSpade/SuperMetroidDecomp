using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyIntroBabyDiscoveryInstructions(ISnesAddressSpace rom)
    {
        foreach ((int start, int end) in new[] { (0xcb33, 0xcb9f), (0xcc2b, 0xcc47), (0xce53, 0xce55) })
        {
            for (int pointer = start; pointer < end; pointer++)
            {
                AssertEqual(rom.ReadByte(0x8b0000 + pointer), IntroBabyDiscoveryInstructionDefinitions.ReadByte((ushort)pointer), "original discovery instruction byte");
                if (pointer == end - 1) continue;
                AssertEqual((ushort)(rom.ReadByte(0x8b0000 + pointer) | rom.ReadByte(0x8b0001 + pointer) << 8),
                    IntroBabyDiscoveryInstructionDefinitions.ReadWord((ushort)pointer), "original discovery aligned or overlapping word");
            }
            AssertThrows<InvalidDataException>(() => IntroBabyDiscoveryInstructionDefinitions.ReadWord((ushort)(end - 1)), "discovery word cannot cross source-region boundary");
        }
        foreach (ushort pointer in new ushort[] { 0, 0xcb32, 0xcb9f, 0xcc2a, 0xcc47, 0xce52, 0xce55, ushort.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => IntroBabyDiscoveryInstructionDefinitions.ReadByte(pointer), "discovery byte boundary");
            AssertThrows<InvalidDataException>(() => IntroBabyDiscoveryInstructionDefinitions.ReadWord(pointer), "discovery word boundary");
        }
    }
}
