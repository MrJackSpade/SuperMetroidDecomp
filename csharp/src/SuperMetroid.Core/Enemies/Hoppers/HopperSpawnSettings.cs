namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The surface a hopper's placement mounts it on (#1472, #2338).</summary>
/// <param name="Surface">The floor or the ceiling.</param>
internal sealed partial record HopperSpawnSettings(HopperSurface Surface) : EnemySpawnSettings;
