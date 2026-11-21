using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>One decoration the Tourian entrance statue scene's first placement spawns (#2557).</summary>
/// <param name="Kind">The projectile kind the decoration is.</param>
/// <param name="Segment">The segment its animation starts at.</param>
/// <param name="Place">The room point, in whole pixels, it stands at, tied there as the statue descends.</param>
internal sealed record TourianStatueDecoration(RoomEnemyProjectileKind Kind, TourianStatueProjectileSegment Segment, Position16<RoomPixels> Place);
