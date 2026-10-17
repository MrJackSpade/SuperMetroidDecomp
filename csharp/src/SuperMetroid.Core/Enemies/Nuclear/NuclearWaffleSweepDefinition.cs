namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One Puromi/Nuclear Waffle sweep geometry definition, in angle units of 1/256 turn.</summary>
/// <param name="StartAngle">Angle each sweep restarts from.</param>
/// <param name="EndAngle">Angle the links clamp to; the final damaging link reaching it ends the sweep.</param>
/// <param name="SegmentSpacing">Signed angle between same-kind links, subtracted successively from each link angle.</param>
/// <param name="InterleavedSegmentOffset">Signed half-pitch staggering the damaging links against the cosmetic ones.</param>
/// <param name="SecondTurnThreshold">Rising joint-turn threshold.</param>
/// <param name="FirstTurnThreshold">Falling joint-turn threshold.</param>
internal readonly record struct NuclearWaffleSweepDefinition(
    int StartAngle,
    int EndAngle,
    short SegmentSpacing,
    short InterleavedSegmentOffset,
    int SecondTurnThreshold,
    int FirstTurnThreshold);
