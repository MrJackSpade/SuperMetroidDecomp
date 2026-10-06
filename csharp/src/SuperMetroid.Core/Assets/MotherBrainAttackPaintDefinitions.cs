namespace SuperMetroid.Core.Assets;

/// <summary>
/// Two independent attack paints and channel-composed luminous colors. Native ring,
/// bomb and beam artwork assigns these colors to fixed sprite pixels; intervening
/// shades calculate by interpolation, and the shell highlight shares recovery light.
/// </summary>
internal static class MotherBrainAttackPaintDefinitions
{
    /// <summary>$A9:94B4, Palette_MotherBrain_Attacks index one: full green and blue form the cyan ring highlight.</summary>
    internal const ushort RingHighlight = (31 << 5) | (31 << 10);

    /// <summary>$A9:94B8, Palette_MotherBrain_Attacks index three: chosen dark cyan ring edge, visible in $8D:82C6.</summary>
    internal const ushort RingEdge = 0x5640;

    /// <summary>$A9:94BA, Palette_MotherBrain_Attacks index four: full red and green form the yellow beam/bomb core.</summary>
    internal const ushort CoreHighlight = 31 | (31 << 5);

    /// <summary>$A9:94C0, Palette_MotherBrain_Attacks index seven: full red forms the beam/bomb core's outer edge.</summary>
    internal const ushort CoreEdge = 31;

    /// <summary>$A9:94D0, Palette_MotherBrain_Attacks index fifteen: chosen dark gray bomb casing outline, visible in $8D:830C.</summary>
    internal const ushort ShellOutline = 0x0c63;
}
