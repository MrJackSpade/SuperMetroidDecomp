namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Whether a Brinstar pipe bug is the strong kind (#1472).</summary>
/// <param name="Strong">Whether it is the strong kind.</param>
internal sealed record BrinstarPipeBugSpawnSettings(bool Strong) : EnemySpawnSettings;
