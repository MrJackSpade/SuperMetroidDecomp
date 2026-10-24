using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>Builds title-state fixtures from the supplied cartridge source and its matching extracted artwork.</summary>
internal static class TitleFixtureVerification
{
    /// <summary>Extracts graphics, palette, and optional gradient from the fixture's own source so patched-ROM visual assertions remain observable.</summary>
    internal static TitleSequenceState CreateTitleFixture(ISnesAddressSpace source,
        CartridgeAudioState? audio = null, TitleGradientPresentation? titleGradientPresentation = null)
    {
        var files = TitleGraphicsExtractor.Extract(source);
        var graphics = TitleGraphicsPresentation.Load(
            new MemoryStream(files[TitleGraphicsFormat.FloorTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.FloorMapFile]),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile]));
        var palette = TitlePaletteFile.Read(new MemoryStream(TitlePaletteExtractor.Extract(source)));
        var gradient = titleGradientPresentation ?? TitleGradientPresentation.Load(
            new MemoryStream(TitleGradientExtractor.Extract(source)));
        return new TitleSequenceState(audio, gradient, palette, graphics, MenuSheetFixtures.WithTitle(graphics));
    }
}
