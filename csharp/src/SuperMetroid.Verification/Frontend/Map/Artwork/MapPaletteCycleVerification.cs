using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.Map.Artwork;

/// <summary>Checks installed map highlight palettes in file-select arrows and pause and map screens.</summary>
internal static class MapPaletteCycleVerification
{
    /// <summary>Checks the installed arrows' palette and highlight cycle's frame count against the cartridge.</summary>
    internal static void VerifyMapPaletteCycleIntegration(ISnesAddressSpace bus,
        AreaMapPresentation original)
    {
        var arrows = new FileSelectMapAnimations(original.Arrows);
        arrows.StepArrows(_ => true);
        var arrowOam = ScreenSpritesFixtures.Blank();
        arrowOam.BeginFrame(); arrows.DrawArrows(arrowOam, arrows.HighlightShading, MenuSheetFixtures.MapSprites(InstalledFixtures.ImportOf(original).Sprites)); arrowOam.FinishFrame();
        ushort nativeArrowPalette = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), MapAnimationNativeSources.AnimatedSpritePalette);
        AssertTrue(arrowOam.NextByteOffset > 0, "arrow palette test draws actual sprites");
        for (int offset = 0; offset < arrowOam.NextByteOffset; offset += 4)
            AssertEqual(LittleEndianBytes.HighByte(nativeArrowPalette), (byte)(arrowOam.LowTable[offset + 3] & 14), "each file-select arrow uses cartridge animated palette");
        AssertEqual(MapAnimationNativeSources.PaletteFrameCount, original.HighlightCycle.FrameCount, "imported native highlight frame count");
        Console.WriteLine("Map palette cycle: the arrows use the cartridge's animated palette and the highlight cycle has the cartridge's frame count.");
    }
}
