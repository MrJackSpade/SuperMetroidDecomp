namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>Which way a face block looks and how near Samus must come (#1472, #1519).</summary>
/// <param name="TriggerDistance">The distance, in pixels, Samus must come within on each axis.</param>
/// <param name="Facing">The way it looks.</param>
internal sealed record FaceBlockSpawnSettings(int TriggerDistance, HorizontalDirection Facing) : EnemySpawnSettings;
