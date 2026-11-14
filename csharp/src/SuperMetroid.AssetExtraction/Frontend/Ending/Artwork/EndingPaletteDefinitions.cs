using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The colour counts of the ending's colour resources the import decodes (#1544).</summary>
internal static class EndingPaletteDefinitions
{
    /// <summary>Full resource sizes, not individual load lengths:
    /// four whole screens, two 16-color runs, and sixteen pairs
    /// of 16-color crossfade layers.</summary>
    internal static readonly IReadOnlyDictionary<EndingPaletteId, int> ColorCounts =
        new Dictionary<EndingPaletteId, int>
        {
            [EndingPaletteId.Escape] = ScreenColourImage.ColorCount,
            [EndingPaletteId.PostCredits] = ScreenColourImage.ColorCount,
            [EndingPaletteId.Credits] = ScreenColourImage.ColorCount,
            [EndingPaletteId.Explosion] = ScreenColourImage.ColorCount,
            [EndingPaletteId.FinalGunship] = 16,
            [EndingPaletteId.LogoInitial] = 16,
            [EndingPaletteId.LogoCrossfade] = EndingLogoDefinitions.CrossfadeSteps * 2 * 16,
        };
}
