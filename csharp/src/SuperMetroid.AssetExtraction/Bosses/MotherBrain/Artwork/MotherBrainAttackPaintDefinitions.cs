using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// Two independent attack paints and channel-composed luminous colors. Ring,
/// bomb and beam artwork assigns these colors to fixed sprite pixels; intervening
/// shades calculate by interpolation, and the shell highlight shares recovery light.
/// </summary>
internal static class MotherBrainAttackPaintDefinitions
{
    /// <summary>Palette_MotherBrain_Attacks index one: full green and blue form the cyan ring highlight.</summary>
    internal static readonly RgbColor RingHighlight = new(0, 255, 255);

    /// <summary>Palette_MotherBrain_Attacks index three: chosen dark cyan ring edge, visible in the onion-ring frames.</summary>
    internal static readonly RgbColor RingEdge = new(0, 148, 173);

    /// <summary>Palette_MotherBrain_Attacks index four: full red and green form the yellow beam/bomb core.</summary>
    internal static readonly RgbColor CoreHighlight = new(255, 255, 0);

    /// <summary>Palette_MotherBrain_Attacks index seven: full red forms the beam/bomb core's outer edge.</summary>
    internal static readonly RgbColor CoreEdge = new(255, 0, 0);

    /// <summary>Palette_MotherBrain_Attacks index fifteen: chosen dark gray bomb casing outline, visible in the bomb frames.</summary>
    internal static readonly RgbColor ShellOutline = new(24, 24, 24);
}
