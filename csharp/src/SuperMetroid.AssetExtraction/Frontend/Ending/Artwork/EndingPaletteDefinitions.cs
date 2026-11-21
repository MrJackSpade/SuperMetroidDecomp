using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The kinds and colour counts of the ending's colour resources the import decodes (#1544).</summary>
internal static class EndingPaletteDefinitions
{
    /// <summary>The colours of one sixteen-colour run, which the final gunship and the logo's first colours are.</summary>
    private const int RunColorCount = 16;

    /// <summary>
    /// Each resource's kind and full size, not its individual load lengths: four whole screens, two 16-color runs, and
    /// sixteen pairs of 16-color crossfade layers.
    /// </summary>
    internal static readonly IReadOnlyDictionary<EndingPaletteId, EndingPaletteDefinition> Palettes =
        new Dictionary<EndingPaletteId, EndingPaletteDefinition>
        {
            [EndingPaletteId.Escape] = new(EndingPaletteKind.WholeScreen, ScreenColourImage.ColorCount),
            [EndingPaletteId.PostCredits] = new(EndingPaletteKind.WholeScreen, ScreenColourImage.ColorCount),
            [EndingPaletteId.Credits] = new(EndingPaletteKind.WholeScreen, ScreenColourImage.ColorCount),
            [EndingPaletteId.Explosion] = new(EndingPaletteKind.WholeScreen, ScreenColourImage.ColorCount),
            [EndingPaletteId.FinalGunship] = new(EndingPaletteKind.Run, RunColorCount),
            [EndingPaletteId.LogoInitial] = new(EndingPaletteKind.Run, RunColorCount),
            [EndingPaletteId.LogoCrossfade] = new(EndingPaletteKind.LogoCrossfade, EndingLogoDefinitions.CrossfadeSteps * 2 * RunColorCount),
        };
}
