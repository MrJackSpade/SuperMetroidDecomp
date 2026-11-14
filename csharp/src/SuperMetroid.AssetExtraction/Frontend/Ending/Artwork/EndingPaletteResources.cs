using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// Builds the ending's colour resources from the colours the import decoded (#1502, #1512): a whole screen's by owner, a
/// run of colours, or the logo crossfade's, each kept as decoded.
/// </summary>
public static class EndingPaletteResources
{
    /// <summary>Keeps a whole ending screen's selected colours, which the import pipeline decoded (#1512).</summary>
    /// <param name="colours">The screen's background colours, then its sprite colours.</param>
    public static EndingPalette FromScreen(ReadOnlySpan<RgbColor> colours) => new([.. colours], screen: true);

    /// <summary>Keeps one ending colour run's selected colours, which the import pipeline decoded (#1512).</summary>
    /// <param name="colours">The selected colours.</param>
    public static EndingPalette FromRun(ReadOnlySpan<RgbColor> colours) => new([.. colours], screen: false);

    /// <summary>
    /// Keeps the logo crossfade's selected colours, which the import pipeline decoded by step and layer (#1512): its steps in
    /// order, each step's background colours before its sprite colours.
    /// </summary>
    /// <param name="colours">The crossfade's colours.</param>
    internal static EndingPalette FromCrossfade(EndingLogoCrossfadeColours colours)
    {
        Ensure.NotNull(colours);
        var ordered = new List<RgbColor>(EndingLogoCrossfadeImage.ColourCount);
        for (int step = 0; step < EndingLogoDefinitions.CrossfadeSteps; step++)
            foreach (EndingLogoLayer layer in (EndingLogoLayer[])[EndingLogoLayer.Background, EndingLogoLayer.Sprites])
                for (int colour = 0; colour < EndingLogoCrossfadeColours.ColoursPerLayer; colour++)
                    ordered.Add(colours.Colour(step, layer, colour));
        return EndingPalette.LogoCrossfade([.. ordered]);
    }
}
