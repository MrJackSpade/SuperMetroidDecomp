namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>Which way a face block looks and how near Samus must come (#1472).</summary>
/// <param name="TriggerDistance">The distance, 0 through 65535 pixels, Samus must come within.</param>
/// <param name="Facing">The way it looks.</param>
internal sealed record FaceBlockSpawnSettings(int TriggerDistance, HorizontalDirection Facing) : EnemySpawnSettings
{
    /// <summary>The distance, 0 through 65535 pixels, Samus must come within.</summary>
    public int TriggerDistance { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, TriggerDistance);
}
