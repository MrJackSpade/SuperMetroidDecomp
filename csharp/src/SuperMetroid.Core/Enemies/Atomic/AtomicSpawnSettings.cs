namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>Which way an Atomic spins and how fast it chases Samus (#1472, #1519).</summary>
/// <param name="Spin">The spinning animation it starts with.</param>
/// <param name="Speed">The speed it moves at on each axis toward Samus, as a positive displacement per update.</param>
internal sealed record AtomicSpawnSettings(AtomicSegment Spin, SignedSixteenSixteen Speed) : EnemySpawnSettings;
