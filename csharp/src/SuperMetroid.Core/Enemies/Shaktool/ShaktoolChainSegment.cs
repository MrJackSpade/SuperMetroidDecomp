namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// One segment of a Shaktool chain: the enemy place it occupies, the definition of the part it is there and its state,
/// which the chain holds from its start and the segment's setup initializes (#2554).
/// </summary>
/// <param name="Place">The enemy place the segment occupies.</param>
/// <param name="Definition">The segment's definition: the saw, arm or head it is.</param>
/// <param name="State">The segment's state.</param>
internal readonly record struct ShaktoolChainSegment(RoomEnemyPlace Place, ShaktoolSegmentDefinition Definition, ShaktoolSegmentState State)
{
    /// <summary>
    /// The state the rest of the chain reaches at the segment: its own, or, once a hit killed it this update, the emptied
    /// record the kill left in its place, which the chain reads and writes until the kill's deletion of the chain
    /// (<see cref="RoomEnemyPlace.ClearedByKill{TRecord}"/>, <see cref="Compatibility.RemovedEnemyLeftovers"/>).
    /// </summary>
    internal ShaktoolSegmentState Reached => Place.ClearedByKill<ShaktoolSegmentState>() ?? State;
}
