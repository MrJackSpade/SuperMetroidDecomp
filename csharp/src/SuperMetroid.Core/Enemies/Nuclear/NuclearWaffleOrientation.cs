namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One orientation result returned by native helper <c>$A6:98E7</c>.</summary>
/// <param name="TurnSpriteVariant">Turn-effect selector 0 for the clockwise sprite-object program or 1 for its counterclockwise counterpart; direction and crossed threshold choose it.</param>
/// <param name="State">Native explosion reason 0 before a joint turn, 1 at the rising threshold, or 2 at the falling threshold; transitions spawn overlays, with state 2 suppressing the turn sound.</param>
public readonly record struct NuclearWaffleOrientation(
    ushort TurnSpriteVariant,
    ushort State);
