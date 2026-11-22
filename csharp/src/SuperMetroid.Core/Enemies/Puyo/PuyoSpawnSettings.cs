namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>How a Puyo hops (#1472, #1519).</summary>
/// <param name="HopCooldown">The updates between hops.</param>
/// <param name="HopDistance">The distance in pixels within which Samus promotes its next ordinary hop to the big hop.</param>
internal sealed partial record PuyoSpawnSettings(int HopCooldown, int HopDistance) : EnemySpawnSettings;
