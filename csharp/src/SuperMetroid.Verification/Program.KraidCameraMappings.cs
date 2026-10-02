using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
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
