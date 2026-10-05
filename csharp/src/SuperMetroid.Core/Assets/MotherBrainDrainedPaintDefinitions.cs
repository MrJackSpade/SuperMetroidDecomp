namespace SuperMetroid.Core.Assets;

/// <summary>
/// Three independently chosen olive-grey paints shared by the drained brain and corpse.
/// The native head/neck art assigns these to cortex/tissue highlight, outline and darkest
/// mouth/neck tissue. Shade ramps, plate grays, repeated colors and neutrals are calculated.
/// Native sources: MotherBrainPalettes_TransitionToFromGrey_7,
/// TransitionMotherBrainPaletteToGrey_DrainedByBabyMetroid.palette7, and
/// TransitionMotherBrainPaletteToGrey_RealDeath.palette7.
/// </summary>
internal static class MotherBrainDrainedPaintDefinitions
{
    /// <summary>$AD:EEB8/F0BF/F1EB, drained/revival-start/corpse-final color zero: shared cortex and lower-face tissue highlight, repeated at color eight.</summary>
    internal const ushort Highlight = 0x4f38;
    /// <summary>$AD:EEBE/F0C5/F1F1, drained/revival-start/corpse-final color three: silhouette and internal olive-grey outlines.</summary>
    internal const ushort Outline = 0x0ca5;
    /// <summary>$AD:EED0/F0D7/F203, drained/revival-start/corpse-final color twelve: darkest lower-face and neck tissue, native sprite palette index thirteen.</summary>
    internal const ushort DarkestTissue = 0x1949;

    /// <summary>
    /// Exact bounded RGB8 representation (193,200,155) of Highlight for the native tissue
    /// gradient. Low bits are interpolation precision selected by the supplied gradient;
    /// this does not claim knowledge of the historical palette-authoring tool.
    /// </summary>
    internal static (int Red, int Green, int Blue) HighlightRgb8 =>
        ((Highlight & 31) * 8 + 1, (Highlight >> 5 & 31) * 8, (Highlight >> 10 & 31) * 8 + 3);

    /// <summary>Exact bounded RGB8 representation (79,83,55) of DarkestTissue for that same four-interval native gradient.</summary>
    internal static (int Red, int Green, int Blue) DarkestTissueRgb8 =>
        ((DarkestTissue & 31) * 8 + 7, (DarkestTissue >> 5 & 31) * 8 + 3, (DarkestTissue >> 10 & 31) * 8 + 7);
}
