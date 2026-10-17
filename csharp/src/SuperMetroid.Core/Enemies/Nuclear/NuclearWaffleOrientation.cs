namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One orientation result returned by native helper <c>$A6:98E7</c>.</summary>
/// <param name="TurnsCounterClockwise">Whether the turn effect runs the counterclockwise sprite-object program instead of the clockwise one; direction and crossed threshold choose it.</param>
/// <param name="State">Which turn threshold the angle has crossed; transitions spawn overlays, and only the rising turn queues the turn sound.</param>
public readonly record struct NuclearWaffleOrientation(
    bool TurnsCounterClockwise,
    NuclearWaffleTurnState State);
