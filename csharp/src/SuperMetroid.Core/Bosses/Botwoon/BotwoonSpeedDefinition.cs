namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's health-dependent movement, body-history spacing and spit speed.</summary>
/// <param name="MovementSpeed">Pixels per direct move toward a hole, or path steps taken per authored-path update.</param>
/// <param name="SegmentSpacingEntries">How many head-history entries apart successive body segments trail.</param>
/// <param name="SpitSpeed">The spit's speed.</param>
public readonly record struct BotwoonSpeedDefinition(int MovementSpeed, int SegmentSpacingEntries, int SpitSpeed);
