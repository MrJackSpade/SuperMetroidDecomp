namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Whether a Brinstar pipe bug is the strong kind (#1472).</summary>
/// <param name="Kind">Which kind of pipe bug it is.</param>
internal sealed record BrinstarPipeBugSpawnSettings(BrinstarPipeBugKind Kind) : EnemySpawnSettings;
