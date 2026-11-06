namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Which segment of a Shaktool a placement is, and how it starts moving (#1472, #1519).</summary>
/// <param name="Motion">How it starts moving.</param>
/// <param name="Segment">The segment of the chain it is, one of <see cref="ShaktoolSegmentDefinitions.Segments"/>.</param>
internal sealed record ShaktoolSpawnSettings(ShaktoolMotionFlags Motion, ShaktoolSegmentDefinition Segment) : EnemySpawnSettings;
