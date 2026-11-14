using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// How the import builds and reads an ending colour resource (#1464, #1512): the logo crossfade's colours by step and
/// layer, and a whole screen's or a run's colours, which it pre-colours the ending's sheets in.
/// </summary>
internal static class EndingPaletteReading
{
    /// <summary>Building the logo crossfade's resource.</summary>
    extension(EndingPalette)
    {
        /// <summary>The logo crossfade's decoded colours: its steps in order, each step's background colours before its sprite colours.</summary>
        /// <exception cref="ArgumentException">The colours are not every step's.</exception>
        internal static EndingPalette LogoCrossfade(ImmutableArray<RgbColor> colours) => new(colours, EndingPalette.Kind.LogoCrossfade);
    }

    /// <summary>Reading a resource's colours.</summary>
    extension(EndingPalette palette)
    {
        /// <summary>Whether this resource is a whole screen's colours.</summary>
        internal bool IsScreen => palette.kind == EndingPalette.Kind.Screen;

        /// <summary>
        /// The colours in resource order: a whole screen's background colours, then its sprite colours; a run's in order;
        /// the crossfade's steps in order, each step's background colours before its sprite colours.
        /// </summary>
        internal ImmutableArray<RgbColor> Colours => palette.colours;

        /// <summary>A whole screen's sprite colours.</summary>
        /// <exception cref="InvalidOperationException">This resource is not a whole screen's.</exception>
        internal ReadOnlySpan<RgbColor> Sprites =>
            palette.RequireScreen().AsSpan(ScreenColourImage.ColorCount / 2, ScreenColourImage.ColorCount / 2);

        /// <summary>The logo crossfade's colour <paramref name="colour"/> of <paramref name="layer"/> at crossfade step <paramref name="step"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The step, layer or colour is outside the crossfade.</exception>
        /// <exception cref="InvalidOperationException">This resource is not the logo crossfade.</exception>
        internal RgbColor LogoCrossfadeColour(int step, EndingLogoLayer layer, int colour)
        {
            if (palette.kind != EndingPalette.Kind.LogoCrossfade)
                throw new InvalidOperationException("Only the logo crossfade is read by step and layer.");
            Ensure.BetweenInclusive(0, EndingLogoDefinitions.CrossfadeSteps - 1, step);
            Ensure.BetweenInclusive(0, EndingPalette.CrossfadeLayerColours - 1, colour);
            int place = layer switch
            {
                EndingLogoLayer.Background => 0,
                EndingLogoLayer.Sprites => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Undefined logo layer."),
            };
            return palette.colours[(step * 2 + place) * EndingPalette.CrossfadeLayerColours + colour];
        }
    }
}
