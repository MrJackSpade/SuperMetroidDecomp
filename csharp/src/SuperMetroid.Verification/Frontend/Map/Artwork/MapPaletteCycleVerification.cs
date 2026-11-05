using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
namespace SuperMetroid.Verification.Frontend.Map.Artwork;

/// <summary>Checks installed map highlight palettes in file-select arrows and pause and map screens.</summary>
internal static class MapPaletteCycleVerification
{
    /// <summary>Compares native animation cadence and CGRAM with the installed highlight cycle.</summary>
    internal static void VerifyMapPaletteCycleIntegration(ISnesAddressSpace bus,
        AreaMapPresentation original, ImportedAreaMap[] rules)
    {
        var guard = NewMapDataGuard(bus, rules);
        var arrows = new FileSelectMapAnimations(original.Arrows);
        arrows.StepArrows(_ => true);
        var arrowOam = ScreenSpritesFixtures.Blank();
        arrowOam.BeginFrame(); arrows.DrawArrows(arrowOam, arrows.HighlightShading, MenuSheetFixtures.MapSprites(InstalledAreaMapPresentation.SpritesOf(original))); arrowOam.FinishFrame();
        ushort nativeArrowPalette = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), MapAnimationNativeSources.AnimatedSpritePalette);
        AssertTrue(arrowOam.NextByteOffset > 0, "arrow palette test draws actual sprites");
        for (int offset = 0; offset < arrowOam.NextByteOffset; offset += 4)
            AssertEqual(LittleEndianBytes.HighByte(nativeArrowPalette), (byte)(arrowOam.LowTable[offset + 3] & 14), "each file-select arrow uses cartridge animated palette");
        var native = new MapPaletteAnimation(MapSpriteScreen.Pause);
        native.Bind(original.HighlightCycle);
        var installed = new MapPaletteAnimation(MapSpriteScreen.Pause);
        installed.Bind(original.HighlightCycle);
        AssertEqual(MapAnimationNativeSources.PaletteFrameCount, original.HighlightCycle.FrameCount, "imported native highlight frame count");
        int loops = 0;
        for (int tick = 0; tick < 600; tick++)
        {
            bool looped = native.Step();
            if (looped) loops++;
            AssertEqual(looped, installed.Step(), "imported highlight requests loop sound on native tick");
            AssertEqual(native.Shading, installed.Shading, "the highlight names the same frame's shade throughout the imported cycle");
        }
        AssertTrue(loops > 5, "highlight parity crosses multiple loop boundaries");

        Console.WriteLine("Map palette cycle: native colors/loop ticks and full pause pixels pass.");
    }
}
