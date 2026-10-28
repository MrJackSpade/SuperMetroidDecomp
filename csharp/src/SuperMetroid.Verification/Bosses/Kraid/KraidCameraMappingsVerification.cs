using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Compares Kraid's pre-growth and grown four-screen scroll mappings with immediate operands in bank $A7.</summary>
internal static class KraidCameraMappingsVerification
{
    /// <summary>Checks the two LDA immediate pairs that initialize Kraid's screen scrolls, including invalid indexes.</summary>
    internal static void VerifyKraidInitialScrollMapping(SuperMetroidAddressSpace rom)
    {
        // Read the two LDA immediate operands stored to Scrolls and Scrolls+2.
        int[] operandAddresses = [0xa7a9eb, 0xa7a9ec, 0xa7a9f2, 0xa7a9f3];
        AssertEqual(operandAddresses.Length, KraidCameraDefinitions.ScreenCount, "Kraid initial screen count");
        for (int screen = 0; screen < operandAddresses.Length; screen++)
            AssertEqual(KraidInitialScrollMappingRomByte(rom, operandAddresses[screen]), (byte)KraidCameraDefinitions.InitialScroll(screen),
                $"Kraid initial scroll screen {screen}");
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => KraidCameraDefinitions.InitialScroll(invalid),
                "Initial scroll preserves span index bounds");
    }

    /// <summary>Reads one byte from the native initial scroll instructions.</summary>
    private static byte KraidInitialScrollMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the grown-phase screen scroll bytes and preserves four-screen index bounds.</summary>
    internal static void VerifyKraidGrownScrollMapping(SuperMetroidAddressSpace rom)
    {
        int[] operandAddresses = [0xa7c0a8, 0xa7c0a9, 0xa7c0af, 0xa7c0b0];
        AssertEqual(operandAddresses.Length, KraidCameraDefinitions.ScreenCount, "Kraid grown screen count");
        for (int screen = 0; screen < operandAddresses.Length; screen++)
            AssertEqual(KraidGrownScrollMappingRomByte(rom, operandAddresses[screen]), (byte)KraidCameraDefinitions.GrownScroll(screen),
                $"Kraid grown scroll screen {screen}");
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => KraidCameraDefinitions.GrownScroll(invalid),
                "Grown scroll preserves span index bounds");
    }

    /// <summary>Reads one byte from the native grown scroll instructions.</summary>
    private static byte KraidGrownScrollMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
