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
        AudioQueues? audio = null, TitleGradientPresentation? titleGradientPresentation = null) =>
        ReadTitleFixtureContent(source, titleGradientPresentation).NewState(audio);

    /// <summary>
    /// Extracts the title content a fixture's states draw from the fixture's own source: its graphics, backdrop, gradient
    /// and the sheets they compose, so several states of one source share one installation of it, as a host's do.
    /// </summary>
    internal static TitleFixtureContent ReadTitleFixtureContent(ISnesAddressSpace source,
        TitleGradientPresentation? titleGradientPresentation = null)
    {
        var graphics = TitleFixtureGraphics(source);
        var palette = TitlePaletteFile.Read(new MemoryStream(TitlePaletteExtractor.Extract(source)));
        var gradient = titleGradientPresentation ?? TitleGradientFile.Read(
            new MemoryStream(TitleGradientExtractor.Extract(source)));
        return new TitleFixtureContent(graphics, palette.Backdrop, gradient, MenuSheetFixtures.WithTitle(graphics));
    }

    /// <summary>The title graphics <paramref name="source"/>'s title fixture draws, read from its extraction.</summary>
    private static TitleGraphicsCharacters TitleFixtureGraphics(ISnesAddressSpace source)
    {
        var files = TitleGraphicsExtractor.Extract(source);
        return TitleGraphicsCharacters.Read(
            new MemoryStream(files[TitleGraphicsFormat.FloorTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.FloorMapFile]),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile]));
    }
}

/// <summary>The title content one fixture source installs, which every title state of that source draws.</summary>
/// <param name="Graphics">The title's characters and compositions.</param>
/// <param name="Backdrop">The title palette's backdrop colour.</param>
/// <param name="Gradient">The title gradient.</param>
/// <param name="Sheets">The sprite sheets the graphics compose, beside the installation's others.</param>
internal sealed record TitleFixtureContent(TitleGraphicsCharacters Graphics, SuperMetroid.Core.Assets.RgbColor Backdrop,
    TitleGradientPresentation Gradient, SuperMetroid.Core.Assets.InstalledSpriteSheets Sheets)
{
    /// <summary>A new title state drawing this content.</summary>
    internal TitleSequenceState NewState(AudioQueues? audio = null) => new(audio, Gradient, Backdrop, Sheets);
}
