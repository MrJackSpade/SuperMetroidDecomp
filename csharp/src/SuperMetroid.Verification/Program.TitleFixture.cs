using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // Extract from the fixture's own source, including constructed/patched ROMs.
    // Supplying retail artwork here would erase the synthetic visual assertions.
    /// <summary>
    /// Builds a title sequence using artwork and palette data extracted from the supplied source,
    /// keeping constructed or patched fixture data consistent with its rendering state.
    /// </summary>
    /// <param name="source">Address space from which title graphics, palette, and any unprovided gradient are extracted.</param>
    /// <param name="audio">Optional audio state attached to the title sequence.</param>
    /// <param name="titleGradientPresentation">Optional preloaded gradient presentation; when absent, the gradient is extracted from <paramref name="source"/>.</param>
    private static TitleSequenceState CreateTitleFixture(ISnesAddressSpace source,
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
