namespace SuperMetroid.Core.Game;

/// <summary>
/// Physical X offsets used by Ridley's private Ceres-door draw hook. The native
/// <c>$A6:A2FF-A30D</c> caller selects only phases 0..3 with <c>timer &amp; 3</c>
/// and reads overlapping 16-bit words at <c>$A6:A321 + phase</c>. In the pinned
/// NTSC J/U v1.0 ROM those words are $0000, $FC00, $FFFC, and $FFFF.
/// OAM keeps only nine X bits, so the effective signed offsets are exactly
/// 0, 0, -4, and -1. This derives the compact lookup from the native word
/// reads and OAM packing; all 65,536 timer values alias those four phases.
/// </summary>
internal static class CeresDoorQuakeDefinitions
{
    private static readonly sbyte[] XOffsets = [0, 0, -4, -1];

    /// <summary>Returns the signed offset selected by the earthquake timer's low two bits.</summary>
    internal static sbyte XOffset(ushort earthquakeTimer) =>
        XOffsets[earthquakeTimer & 3];
}
