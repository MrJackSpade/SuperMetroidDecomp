namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The chain one Shaktool segment joined when it was placed (#1542). The room lists the chain's seven segments in a
/// row from its first enemy, saw first: a segment orbits the enemy listed right before it, and the centre faces the one
/// listed right after it (<see cref="RoomEnemyListing"/>).
/// </summary>
/// <param name="Chain">The chain the segment belongs to: its seven segments, leading saw first, and their shared motion (#2452).</param>
internal sealed record ShaktoolChainPlaces(ShaktoolChain Chain);
