namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A part as it stands on a boss body layer (#1466): the cells of it still shown, which later parts have not covered.
/// </summary>
/// <param name="Part">The part.</param>
/// <param name="Shows">The cells it still shows, never empty.</param>
public sealed record BossBodyPlacement(BossBodyPart Part, BossBodyFootprint Shows);
