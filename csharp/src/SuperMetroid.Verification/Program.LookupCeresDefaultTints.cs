using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresDefaultRed(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyCeresDefaultComponent(rom, stock, 0x20, color => color.Red);
    private static void VerifyCeresDefaultGreen(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyCeresDefaultComponent(rom, stock, 0x40, color => color.Green);
    private static void VerifyCeresDefaultBlue(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyCeresDefaultComponent(rom, stock, 0x80, color => color.Blue);

    private static void VerifyCeresDefaultComponent(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock,
        int channelMask, Func<PaletteRgb5, int> component)
    {
        // Native CMP #0010 exits before another table write. Final prior counter is15.
        int stopCounter = ReadVerificationWord(rom, 0x88de43);
        foreach (bool dead in new[] { false, true })
        {
            ushort channelFlags = ReadVerificationWord(rom, dead ? 0x88de16 : 0x88de11);
            int expected = (channelFlags & channelMask) != 0 ? stopCounter - 1 : 0;
            var scalar = RoomFxPaletteBlendDefinitions.StockCeresHazeComponents(dead);
            AssertEqual(expected, channelMask == 0x20 ? scalar.Red : channelMask == 0x40 ? scalar.Green : scalar.Blue,
                "Scalar tint view uses the same native component");
            AssertEqual(expected, component(RoomFxPaletteBlendDefinitions.StockCeresHaze(dead)), "Original Ceres default channel/amplitude");
            AssertEqual(expected, component(dead ? RoomFxPaletteBlendDefinitions.StockCeresHazeRed : RoomFxPaletteBlendDefinitions.StockCeresHazeBlue), "Named default tint alias");
            AssertEqual(expected, component(dead ? stock.CeresHazeRed : stock.CeresHazeBlue), "Imported default tint channel");
        }
    }

    private static void VerifyCeresDefaultTintStructure(ISnesAddressSpace rom)
    {
        AssertEqual(ReadVerificationWord(rom, 0x88de43), CeresHazeDefinitions.FadeSteps, "Original native fade stop counter");
        foreach (string name in new[] { "StockCeresHazeBlue", "StockCeresHazeRed" })
            AssertTrue(typeof(RoomFxPaletteBlendDefinitions).GetField($"<{name}>k__BackingField",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic) is null,
                "Default tint is calculated rather than stored in a static record");
    }
}
