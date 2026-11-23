namespace SuperMetroid.Core.Enemies;

/// <summary>What common normal-shot damage did to the enemy a shot reached (#2255), and the health it took.</summary>
/// <param name="Hit">Whether the shot froze the enemy outright, was a dud, or hurt it while it was frozen or not.</param>
/// <param name="HealthLost">The health the enemy lost: none unless the shot hurt it, and none when a lethal Ice-bearing beam froze it instead.</param>
internal readonly partial record struct NormalShotOutcome(NormalShotHit Hit, int HealthLost);
