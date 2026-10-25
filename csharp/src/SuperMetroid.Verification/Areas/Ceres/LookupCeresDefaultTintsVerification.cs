using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>Compares calculated Ceres haze channels with retail fade flags and imported stock colors.</summary>
internal static class LookupCeresDefaultTintsVerification
{
    /// <summary>Checks the red haze channel for live and dead Ceres states.</summary>
    internal static void VerifyCeresDefaultRed(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyCeresDefaultComponent(rom, stock, 0x20, color => color.Red);
    /// <summary>Checks the green haze channel for live and dead Ceres states.</summary>
    internal static void VerifyCeresDefaultGreen(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyCeresDefaultComponent(rom, stock, 0x40, color => color.Green);
    /// <summary>Checks the blue haze channel for live and dead Ceres states.</summary>
    internal static void VerifyCeresDefaultBlue(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock) => VerifyCeresDefaultComponent(rom, stock, 0x80, color => color.Blue);

    /// <summary>Checks that channel flags use the last value before native CMP #$0010 stops the fade.</summary>
    private static void VerifyCeresDefaultComponent(ISnesAddressSpace rom, RoomFxPaletteBlendCatalog stock,
        int channelMask, Func<RgbColor, int> component)
    {
        int stopCounter = ReadVerificationWord(rom, 0x88de43);
        foreach (bool dead in new[] { false, true })
        {
            ushort channelFlags = ReadVerificationWord(rom, dead ? 0x88de16 : 0x88de11);
            int expected = (channelFlags & channelMask) != 0 ? stopCounter - 1 : 0;
            var scalar = RoomFxPaletteBlendDefinitions.StockCeresHazeTint(dead);
            AssertEqual(expected, channelMask == 0x20 ? scalar.Red : channelMask == 0x40 ? scalar.Green : scalar.Blue,
                "Scalar tint view uses the same native component");
            AssertEqual(expected, component(RoomFxPaletteBlendDefinitions.StockCeresHaze(dead ? CeresHazeColor.Red : CeresHazeColor.Blue)), "Original Ceres default channel/amplitude");
            AssertEqual(expected, component(dead ? RoomFxPaletteBlendDefinitions.StockCeresHazeRed : RoomFxPaletteBlendDefinitions.StockCeresHazeBlue), "Named default tint alias");
            AssertEqual(expected, component(dead ? stock.CeresHazeRed : stock.CeresHazeBlue), "Imported default tint channel");
        }
    }

    /// <summary>Checks the native fade stop counter and that stock tints are calculated rather than cached fields.</summary>
    internal static void VerifyCeresDefaultTintStructure(ISnesAddressSpace rom)
    {
        AssertEqual(ReadVerificationWord(rom, 0x88de43), CeresHazeDefinitions.FadeSteps, "Original native fade stop counter");
        foreach (string name in new[] { "StockCeresHazeBlue", "StockCeresHazeRed" })
            AssertTrue(!MetadataShape.DeclaresField(typeof(RoomFxPaletteBlendDefinitions), $"<{name}>k__BackingField", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic),
                "Default tint is calculated rather than stored in a static record");
    }
}
