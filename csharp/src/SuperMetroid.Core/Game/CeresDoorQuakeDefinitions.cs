namespace SuperMetroid.Core.Game;

/// <summary>
/// Physical X offsets used by Ridley's private Ceres-door draw hook. The native
/// <c>$A6:A2FF-A30D</c> caller selects only phases 0..3 with <c>timer &amp; 3</c>
/// and reads overlapping 16-bit words at <c>$A6:A321 + phase</c>. In the pinned
/// NTSC J/U v1.0 ROM those words are $0000, $FC00, $FFFC, and $FFFF.
/// OAM keeps only nine X bits, so the effective signed offsets are exactly
/// 0, 0, -4, and -1. This decodes the consumed bytes directly from the native word
/// reads and OAM packing; all 65,536 timer values alias those four phases.
/// The ninth X bit of each overlapping word agrees with sign extension of its low
/// byte: the high bytes are 00, FC, FF, FF. Phase 3 consumes the first byte at $A6:A325;
/// bytes $A6:A326-A328 are not read by this caller.
/// </summary>
internal static class CeresDoorQuakeDefinitions
{
    /// <summary>$A6:A323 is the negative four-pixel quake impulse, following the neutral word at $A6:A321.</summary>
    private const short Impulse = -4;

    /// <summary>Returns the signed offset selected by the earthquake timer's low two bits.</summary>
    internal static sbyte XOffset(ushort earthquakeTimer)
    {
        int phase = earthquakeTimer & 3;
        int word = phase < sizeof(ushort) ? 0 : Impulse;
        return unchecked((sbyte)(word >> (8 * (phase & 1))));
    }
}
