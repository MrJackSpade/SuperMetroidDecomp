namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Which segment of a Shaktool a placement is, and how it starts moving (#1472).</summary>
/// <param name="Motion">How it starts moving.</param>
/// <param name="Segment">The segment, 0 through 6.</param>
internal sealed record ShaktoolSpawnSettings(ShaktoolMotionFlags Motion, int Segment) : EnemySpawnSettings
{
    /// <summary>The segment, 0 through 6.</summary>
    public int Segment { get; init; } = Ensure.BetweenInclusive(0, short.MaxValue, Segment);
}
