using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the compiled Baby-discovery input bytes and words against the cartridge and verifies their region boundaries are enforced.</summary>
    /// <param name="rom">Cartridge address space containing the native discovery-input instruction bytes.</param>
    private static void VerifyIntroBabyDiscoveryInput(ISnesAddressSpace rom)
    {
        foreach ((int start, int end) in new[] { (0x860d, 0x864f), (0x877e, 0x8784) })
        {
            for (int pointer = start; pointer < end; pointer++)
            {
                AssertEqual(rom.ReadByte(0x910000 + pointer), IntroBabyDiscoveryInputDefinitions.ReadByte((ushort)pointer), "original discovery input byte");
                if (pointer == end - 1) continue;
                AssertEqual((ushort)(rom.ReadByte(0x910000 + pointer) | rom.ReadByte(0x910001 + pointer) << 8),
                    IntroBabyDiscoveryInputDefinitions.ReadWord((ushort)pointer), "original discovery aligned or overlapping word");
            }
            AssertThrows<InvalidDataException>(() => IntroBabyDiscoveryInputDefinitions.ReadWord((ushort)(end - 1)), "discovery word cannot cross source-region boundary");
        }
        foreach (ushort pointer in new ushort[] { 0, 0x860c, 0x864f, 0x877d, 0x8784, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => IntroBabyDiscoveryInputDefinitions.ReadByte(pointer), "discovery byte boundary");
            AssertThrows<InvalidDataException>(() => IntroBabyDiscoveryInputDefinitions.ReadWord(pointer), "discovery word boundary");
        }
    }
}
