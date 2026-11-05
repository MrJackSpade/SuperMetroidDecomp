using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>Builds title-state fixtures from the supplied cartridge source and its matching extracted artwork.</summary>
internal static class TitleFixtureVerification
{
    /// <summary>Extracts graphics, palette, and optional gradient from the fixture's own source so patched-ROM visual assertions remain observable.</summary>
    internal static TitleSequenceState CreateTitleFixture(ISnesAddressSpace source,
        AudioQueues? audio = null, TitleGradientPresentation? titleGradientPresentation = null)
    {
        var graphics = TitleFixtureGraphics(source);
        var palette = TitlePaletteFile.Read(new MemoryStream(TitlePaletteExtractor.Extract(source)));
        var gradient = titleGradientPresentation ?? TitleGradientFile.Read(
            new MemoryStream(TitleGradientExtractor.Extract(source)));
        return new TitleSequenceState(audio, gradient, palette.Backdrop, graphics, MenuSheetFixtures.WithTitle(graphics));
    }

    /// <summary>The title graphics <paramref name="source"/>'s title fixture draws, read from its extraction.</summary>
    internal static TitleGraphicsCharacters TitleFixtureGraphics(ISnesAddressSpace source)
    {
        var files = TitleGraphicsExtractor.Extract(source);
        return TitleGraphicsCharacters.Read(
            new MemoryStream(files[TitleGraphicsFormat.FloorTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.FloorMapFile]),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile]));
    }
}
