namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Physical X offsets used by Ridley's private Ceres-door draw hook. The
/// draw hook selects only phases 0..3 with <c>timer &amp; 3</c>
/// and reads overlapping 16-bit words from the shake table. Those
/// words are 0x0000, 0xFC00, 0xFFFC, and 0xFFFF.
/// Sprite X positions keep only nine bits, so the effective signed offsets are exactly
/// 0, 0, -4, and -1. This decodes the consumed bytes directly from the word
/// reads and nine-bit sprite positions; all 65,536 timer values alias those four phases.
/// The ninth X bit of each overlapping word agrees with sign extension of its low
/// byte: the high bytes are 00, FC, FF, FF. Phase 3 consumes the first byte;
/// the remaining table bytes are not read by this caller.
/// </summary>
internal static class CeresDoorQuakeDefinitions
{
    /// <summary>The negative four-pixel quake impulse, following the neutral word.</summary>
    internal const int Impulse = -4;
}
