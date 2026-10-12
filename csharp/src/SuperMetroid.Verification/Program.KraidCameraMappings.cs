using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Represents kraid camera mappings verification.</summary>
internal static class KraidCameraMappingsVerification
{
    /// <summary>Verifies kraid initial scroll mapping.</summary>
    internal static void VerifyKraidInitialScrollMapping(SuperMetroidAddressSpace rom)
    {
        // Read the two LDA immediate operands stored to Scrolls and Scrolls+2.
        int[] operandAddresses = [0xa7a9eb, 0xa7a9ec, 0xa7a9f2, 0xa7a9f3];
        AssertEqual(operandAddresses.Length, KraidCameraDefinitions.ScreenCount, "Kraid initial screen count");
        for (int screen = 0; screen < operandAddresses.Length; screen++)
            AssertEqual(KraidInitialScrollMappingRomByte(rom, operandAddresses[screen]), (byte)KraidCameraDefinitions.InitialScroll(screen),
                $"Kraid initial scroll screen {screen}");
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => KraidCameraDefinitions.InitialScroll(invalid),
                "Initial scroll preserves span index bounds");
    }

    /// <summary>Kraid initial scroll mapping rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid initial scroll mapping")]
    private static byte KraidInitialScrollMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Verifies kraid grown scroll mapping.</summary>
    internal static void VerifyKraidGrownScrollMapping(SuperMetroidAddressSpace rom)
    {
        int[] operandAddresses = [0xa7c0a8, 0xa7c0a9, 0xa7c0af, 0xa7c0b0];
        AssertEqual(operandAddresses.Length, KraidCameraDefinitions.ScreenCount, "Kraid grown screen count");
        for (int screen = 0; screen < operandAddresses.Length; screen++)
            AssertEqual(KraidGrownScrollMappingRomByte(rom, operandAddresses[screen]), (byte)KraidCameraDefinitions.GrownScroll(screen),
                $"Kraid grown scroll screen {screen}");
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => KraidCameraDefinitions.GrownScroll(invalid),
                "Grown scroll preserves span index bounds");
    }

    /// <summary>Kraid grown scroll mapping rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid grown scroll mapping")]
    private static byte KraidGrownScrollMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
