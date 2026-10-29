namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>A part as a canvas holds it (#1466): the part's name in its boss's sheet and the cells it still shows.</summary>
/// <param name="Part">The part's name in the boss's body sheet.</param>
/// <param name="Shows">The cells it still shows, never empty.</param>
internal sealed record PlacedBossBodyPart(string Part, BossBodyFootprint Shows);
