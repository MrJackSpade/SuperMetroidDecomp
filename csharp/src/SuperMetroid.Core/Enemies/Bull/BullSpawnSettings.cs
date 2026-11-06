namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>How a Bull speeds up and how fast it goes (#1472, #1519).</summary>
/// <param name="Intervals">The updates between its acceleration steps and between its deceleration steps.</param>
/// <param name="MaximumSpeed">Its maximum speed, in 256ths of a pixel per update.</param>
internal sealed record BullSpawnSettings(BullSpeedIntervals Intervals, int MaximumSpeed) : EnemySpawnSettings;
