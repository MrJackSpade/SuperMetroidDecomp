namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One Puromi/Nuclear Waffle sweep geometry definition.</summary>
/// <param name="StartAngle">Angle each sweep restarts from.</param>
/// <param name="EndAngle">Angle the links clamp to; the final damaging link reaching it ends the sweep.</param>
/// <param name="SegmentSpacing">
/// Steps between same-kind links, each trailing the one before it behind the head, the way the sweep runs (#2439).
/// </param>
/// <param name="InterleavedSegmentOffset">
/// Steps, half the spacing, the damaging links stand ahead of the cosmetic ones, staggering the two kinds (#2439).
/// </param>
/// <param name="RisingTurnThreshold">Rising joint-turn threshold.</param>
/// <param name="FallingTurnThreshold">Falling joint-turn threshold.</param>
internal readonly record struct NuclearWaffleSweepDefinition(
    NuclearWaffleSweepAngle StartAngle,
    NuclearWaffleSweepAngle EndAngle,
    int SegmentSpacing,
    int InterleavedSegmentOffset,
    NuclearWaffleSweepAngle RisingTurnThreshold,
    NuclearWaffleSweepAngle FallingTurnThreshold);
