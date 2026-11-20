using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Which half of a Zebetite pair a placement is (#1472).</summary>
/// <param name="Half">The half.</param>
internal sealed record ZebetiteSpawnSettings(ZebetiteHalf Half) : EnemySpawnSettings;
