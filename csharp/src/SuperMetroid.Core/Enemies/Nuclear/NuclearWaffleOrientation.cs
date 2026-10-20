namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One orientation result returned by native helper <c>$A6:98E7</c>.</summary>
/// <param name="Turn">The sense of the turn effect's sprite-object program; direction and crossed threshold choose it.</param>
/// <param name="State">Which turn threshold the angle has crossed; transitions spawn overlays, and only the rising turn queues the turn sound.</param>
public readonly record struct NuclearWaffleOrientation(
    RotationDirection Turn,
    NuclearWaffleTurnState State);
