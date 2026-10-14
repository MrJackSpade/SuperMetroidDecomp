using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Builds title-state fixtures from the supplied cartridge source and its matching extracted artwork.</summary>
internal static class TitleFixtureVerification
{
    /// <summary>Extracts graphics, palette, and optional gradient from the fixture's own source so patched-ROM visual assertions remain observable.</summary>
    internal static TitleSequenceState CreateTitleFixture(ISnesAddressSpace source,
        CartridgeAudioState? audio = null, TitleGradientPresentation? titleGradientPresentation = null)
    {
        var files = TitleGraphicsExtractor.Extract(source);
        var graphics = TitleGraphicsPresentation.Load(
            new MemoryStream(files[TitleGraphicsFormat.Mode7TilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.Mode7MapFile]),
            new MemoryStream(files[TitleGraphicsFormat.ObjectTilesFile]),
            new MemoryStream(files[TitleGraphicsFormat.BabyTilesFile]));
        var palette = TitlePalettePresentation.Load(new MemoryStream(TitlePaletteExtractor.Extract(source)));
        var gradient = titleGradientPresentation ?? TitleGradientPresentation.Load(
            new MemoryStream(TitleGradientExtractor.Extract(source)));
        return new TitleSequenceState(source, audio, gradient, palette, graphics);
    }
}
