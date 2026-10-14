namespace SuperMetroid.Core.Game;

/// <summary>One Puromi/Nuclear Waffle sweep geometry definition.</summary>
internal readonly record struct NuclearWaffleSweepDefinition(
    ushort StartAngle,
    ushort EndAngle,
    short SegmentSpacing,
    short InterleavedSegmentOffset,
    ushort SecondTurnThreshold,
    ushort FirstTurnThreshold);
