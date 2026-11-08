using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// Three independently chosen olive-grey paints shared by the drained brain and corpse.
/// The head/neck art assigns these to cortex/tissue highlight, outline and darkest
/// mouth/neck tissue. Shade ramps, plate grays, repeated colors and neutrals are calculated.
/// Sources: MotherBrainPalettes_TransitionToFromGrey_7,
/// TransitionMotherBrainPaletteToGrey_DrainedByBabyMetroid.palette7, and
/// TransitionMotherBrainPaletteToGrey_RealDeath.palette7.
/// </summary>
internal static class MotherBrainDrainedPaintDefinitions
{
    /// <summary>Drained/revival-start/corpse-final color zero: shared cortex and lower-face tissue highlight, repeated at color eight.</summary>
    internal static readonly RgbColor Highlight = new(198, 206, 156);
    /// <summary>Drained/revival-start/corpse-final color three: silhouette and internal olive-grey outlines.</summary>
    internal static readonly RgbColor Outline = new(41, 41, 24);
    /// <summary>Drained/revival-start/corpse-final color twelve: darkest lower-face and neck tissue, sprite palette index thirteen.</summary>
    internal static readonly RgbColor DarkestTissue = new(74, 82, 49);

    /// <summary>
    /// Exact bounded RGB8 representation (193,200,155) of Highlight for the tissue
    /// gradient. Low bits are interpolation precision selected by the supplied gradient;
    /// this does not claim knowledge of the historical palette-authoring tool.
    /// </summary>
    internal static Rgb8 HighlightRgb8 =>
        new(Highlight.FadeLevels.Red * 8 + 1, Highlight.FadeLevels.Green * 8, Highlight.FadeLevels.Blue * 8 + 3);

    /// <summary>Exact bounded RGB8 representation (79,83,55) of DarkestTissue for that same four-interval gradient.</summary>
    internal static Rgb8 DarkestTissueRgb8 =>
        new(DarkestTissue.FadeLevels.Red * 8 + 7, DarkestTissue.FadeLevels.Green * 8 + 3, DarkestTissue.FadeLevels.Blue * 8 + 7);
}
