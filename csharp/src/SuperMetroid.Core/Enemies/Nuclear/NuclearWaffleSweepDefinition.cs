namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One Puromi/Nuclear Waffle sweep geometry definition.</summary>
/// <param name="StartAngle">Angle each sweep restarts from.</param>
/// <param name="EndAngle">Angle the links clamp to; the final damaging link reaching it ends the sweep.</param>
/// <param name="SegmentSpacing">
/// Signed steps between same-kind links, subtracted successively from each link angle: positive when the links trail at
/// smaller angles.
/// </param>
/// <param name="InterleavedSegmentOffset">Signed steps, half the spacing, staggering the damaging links against the cosmetic ones.</param>
/// <param name="RisingTurnThreshold">Rising joint-turn threshold.</param>
/// <param name="FallingTurnThreshold">Falling joint-turn threshold.</param>
internal readonly record struct NuclearWaffleSweepDefinition(
    NuclearWaffleSweepAngle StartAngle,
    NuclearWaffleSweepAngle EndAngle,
    int SegmentSpacing,
    int InterleavedSegmentOffset,
    NuclearWaffleSweepAngle RisingTurnThreshold,
    NuclearWaffleSweepAngle FallingTurnThreshold);
