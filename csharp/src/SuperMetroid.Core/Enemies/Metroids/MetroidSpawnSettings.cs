namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>How much ice damage freezes a Metroid (#1472, #1519).</summary>
/// <param name="IceDamageToFreeze">The ice damage that freezes it.</param>
internal sealed record MetroidSpawnSettings(int IceDamageToFreeze) : EnemySpawnSettings;
