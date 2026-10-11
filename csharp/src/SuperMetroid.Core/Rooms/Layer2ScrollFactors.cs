using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The room state's per-axis BG2 scroll modes (state-header bytes 12 and 13, copied to
/// <c>$0917/$0918</c> by <c>LoadStateHeader</c>). On each axis zero copies BG1, one freezes the axis, and
/// any other value uses its even portion as a parallax multiplier over 128; the low bit also stops
/// bank $80 streaming that axis.
/// </summary>
/// <param name="X">Horizontal mode byte.</param>
/// <param name="Y">Vertical mode byte.</param>
[SpatialComponents("Value type: owns the horizontal and vertical BG2 scroll mode bytes of one room state.")]
public readonly record struct Layer2ScrollFactors(byte X, byte Y);
