using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the four native bank-$A7 initial-scroll operands against the compiled per-screen mapping and its index bounds.</summary>
    /// <param name="rom">Retail address space containing the initial camera-scroll instruction operands.</param>
    private static void VerifyKraidInitialScrollMapping(SuperMetroidAddressSpace rom)
    {
        // Read the two LDA immediate operands stored to Scrolls and Scrolls+2.
        int[] operandAddresses = [0xa7a9eb, 0xa7a9ec, 0xa7a9f2, 0xa7a9f3];
        AssertEqual(operandAddresses.Length, KraidCameraDefinitions.ScreenCount, "Kraid initial screen count");
        for (int screen = 0; screen < operandAddresses.Length; screen++)
            AssertEqual(rom.ReadByte(operandAddresses[screen]), (byte)KraidCameraDefinitions.InitialScroll(screen),
                $"Kraid initial scroll screen {screen}");
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => KraidCameraDefinitions.InitialScroll(invalid),
                "Initial scroll preserves span index bounds");
    }

    /// <summary>Checks the four native bank-$A7 grown-Kraid scroll operands against the compiled per-screen mapping and its index bounds.</summary>
    /// <param name="rom">Retail address space containing the grown-Kraid camera-scroll instruction operands.</param>
    private static void VerifyKraidGrownScrollMapping(SuperMetroidAddressSpace rom)
    {
        int[] operandAddresses = [0xa7c0a8, 0xa7c0a9, 0xa7c0af, 0xa7c0b0];
        AssertEqual(operandAddresses.Length, KraidCameraDefinitions.ScreenCount, "Kraid grown screen count");
        for (int screen = 0; screen < operandAddresses.Length; screen++)
            AssertEqual(rom.ReadByte(operandAddresses[screen]), (byte)KraidCameraDefinitions.GrownScroll(screen),
                $"Kraid grown scroll screen {screen}");
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => KraidCameraDefinitions.GrownScroll(invalid),
                "Grown scroll preserves span index bounds");
    }
}
