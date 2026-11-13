using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Frontend.Map.Artwork;

/// <summary>Checks native pause, file-select and world-map colors.</summary>
internal static class MapStaticPalettesVerification
{
    /// <summary>Compares stock palettes with the cartridge and the six stock world selections.</summary>
    internal static void VerifyMapStaticPaletteIntegration(ISnesAddressSpace bus,
        AreaMapPresentation original)
    {
        var palette = new SnesCgram();
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(palette, bus, MapSourceDefinitions.Artwork.PauseColors);
        MapStaticPalettes installed = InstalledFixtures.ImportOf(original).Palettes;
        AssertTrue(palette.Colors.SequenceEqual(installed.Pause), "all static pause colors match cartridge");
        AssertEqual(palette.Colors[0], original.Backdrops.Pause, "the pause backdrop is the cartridge's backdrop colour");
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(palette, bus, MapSourceDefinitions.Menu.Colors);
        AssertTrue(palette.Colors.SequenceEqual(installed.FileSelect), "all static file-select colors match cartridge");
        AssertEqual(palette.Colors[0], original.Backdrops.FileSelect, "the file-select backdrop is the cartridge's backdrop colour");
        var nativeWorld = new FileSelectAreaMapGraphics(AreaId.Crateria, original.Backdrops, MenuSheetFixtures.For(original));
        var world = new FileSelectAreaMapGraphics(AreaId.Crateria, original.Backdrops, MenuSheetFixtures.For(original));
        foreach (AreaId area in AreaIds.Zebes)
        {
            nativeWorld.SelectArea(area); world.SelectArea(area);
            AssertEqual(installed.World(area)[0], world.screen.Backdrop, "the selected area shows its world-map backdrop");
            AssertTrue(SuperMetroid.TestSupport.FrameRendering.ScenePixels.Same(nativeWorld.BackgroundsScene(), world.BackgroundsScene()), "all six stock world selections render identical pixels");
        }
        Console.WriteLine("Static map palettes: all six native world selections and pause/file-select colors pass.");
    }
}
