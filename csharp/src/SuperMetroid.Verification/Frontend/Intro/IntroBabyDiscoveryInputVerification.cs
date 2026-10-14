using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the two admitted intro Baby discovery input ranges against retail bank-$91 bytes.</summary>
internal static class IntroBabyDiscoveryInputVerification
{
    /// <summary>Compares every byte and overlapping word, then checks both range boundaries reject reads that cross them.</summary>
    internal static void VerifyIntroBabyDiscoveryInput(ISnesAddressSpace rom)
    {
        foreach ((int start, int end) in new[] { (0x860d, 0x864f), (0x877e, 0x8784) })
        {
            for (int pointer = start; pointer < end; pointer++)
            {
                AssertEqual(IntroBabyDiscoveryInputRomByte(rom, 0x910000 + pointer), IntroBabyDiscoveryInputDefinitions.ReadByte((ushort)pointer), "original discovery input byte");
                if (pointer == end - 1) continue;
                AssertEqual((ushort)(SnesLittleEndian.Word(IntroBabyDiscoveryInputRomByte(rom, 0x910000 + pointer), IntroBabyDiscoveryInputRomByte(rom, 0x910001 + pointer))),
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

    /// <summary>Reads a native source byte through the cartridge oracle for the range comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro Baby discovery input")]
    private static byte IntroBabyDiscoveryInputRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
