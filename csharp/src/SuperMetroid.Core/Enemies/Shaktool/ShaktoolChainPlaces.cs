namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The places one Shaktool segment found when it was placed (#1542). The room lists the chain's seven segments in a
/// row from its first enemy, saw first.
/// </summary>
/// <param name="Chain">The chain the segment belongs to: its seven segments, leading saw first, and their shared motion (#2452).</param>
/// <param name="Previous">The enemy the room lists right before the segment, which it orbits; null for the leading saw.</param>
/// <param name="Next">The enemy the room lists right after the segment, which the centre faces; null for a segment listed last.</param>
internal sealed partial record ShaktoolChainPlaces(ShaktoolChain Chain, RoomEnemyPlace? Previous, RoomEnemyPlace? Next);
