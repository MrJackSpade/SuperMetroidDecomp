namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>One segment of a Shaktool chain: the enemy place it occupies and the definition of the part it is there.</summary>
/// <param name="Place">The enemy place the segment occupies.</param>
/// <param name="Definition">The segment's definition: the saw, arm or head it is.</param>
internal readonly partial record struct ShaktoolChainSegment(RoomEnemyPlace Place, ShaktoolSegmentDefinition Definition);
