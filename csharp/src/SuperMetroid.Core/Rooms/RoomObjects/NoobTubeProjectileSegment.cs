namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The n00b tube's crack, glass shard and released-air bubble animation segments (#641), in authored order. Each shard
/// has its flight out of the tube, the start of its fall and its swaying fall, each flickering it at its column and mirrored across the tube, or
/// hiding it; the animation its spawn starts is keyed by the shard (<see cref="NoobTubeShard"/>, #2044).
/// </summary>
internal enum NoobTubeProjectileSegment
{
    /// <summary>The crack spreading across the glass, shattering and its pieces falling.</summary>
    Crack,
    /// <summary>The fallen crack's last pieces, after which it is deleted.</summary>
    CrackFallen,
    /// <summary>The deletion that ends the crack and every shard (#2276).</summary>
    Deletion,
    /// <summary>The shard that flies outward and down, flying out of the tube.</summary>
    ShardDownOutwardFlying,
    /// <summary>The shard that flies outward and down, starting its fall (#2276).</summary>
    ShardDownOutwardFallStart,
    /// <summary>The shard that flies outward and down, swaying as it falls.</summary>
    ShardDownOutwardFalling,
    /// <summary>The shard that flies outward and up, flying out of the tube.</summary>
    ShardUpOutwardFlying,
    /// <summary>The shard that flies outward and up, starting its fall (#2276).</summary>
    ShardUpOutwardFallStart,
    /// <summary>The shard that flies outward and up, swaying as it falls.</summary>
    ShardUpOutwardFalling,
    /// <summary>The shard that flies steeply up, flying out of the tube.</summary>
    ShardUpSteepFlying,
    /// <summary>The shard that flies steeply up, starting its fall (#2276).</summary>
    ShardUpSteepFallStart,
    /// <summary>The shard that flies steeply up, swaying as it falls.</summary>
    ShardUpSteepFalling,
    /// <summary>The shard that flies diagonally up from nearer the crack, flying out of the tube.</summary>
    ShardUpDiagonalInnerFlying,
    /// <summary>The shard that flies diagonally up from nearer the crack, starting its fall (#2276).</summary>
    ShardUpDiagonalInnerFallStart,
    /// <summary>The shard that flies diagonally up from nearer the crack, swaying as it falls.</summary>
    ShardUpDiagonalInnerFalling,
    /// <summary>The shard that flies diagonally up from farther along the tube, flying out of the tube.</summary>
    ShardUpDiagonalOuterFlying,
    /// <summary>The shard that flies diagonally up from farther along the tube, starting its fall (#2276).</summary>
    ShardUpDiagonalOuterFallStart,
    /// <summary>The shard that flies diagonally up from farther along the tube, swaying as it falls.</summary>
    ShardUpDiagonalOuterFalling,
    /// <summary>The shard that flies diagonally down, flying out of the tube.</summary>
    ShardDownDiagonalFlying,
    /// <summary>The shard that flies diagonally down, starting its fall (#2276).</summary>
    ShardDownDiagonalFallStart,
    /// <summary>The shard that flies diagonally down, swaying as it falls.</summary>
    ShardDownDiagonalFalling,
    /// <summary>The shard that drops nearly straight down from farther along the tube, flying out of the tube.</summary>
    ShardDownOuterFlying,
    /// <summary>The shard that drops nearly straight down from farther along the tube, starting its fall (#2276).</summary>
    ShardDownOuterFallStart,
    /// <summary>The shard that drops nearly straight down from farther along the tube, swaying as it falls.</summary>
    ShardDownOuterFalling,
    /// <summary>The shard that flies outward, almost level, flying out of the tube.</summary>
    ShardOutwardFlying,
    /// <summary>The shard that flies outward, almost level, starting its fall (#2276).</summary>
    ShardOutwardFallStart,
    /// <summary>The shard that flies outward, almost level, swaying as it falls.</summary>
    ShardOutwardFalling,
    /// <summary>The shard that flies straight up from the crack and has no mirror image, flying out of the tube.</summary>
    ShardUpFlying,
    /// <summary>The shard that flies straight up from the crack and has no mirror image, starting its fall (#2276).</summary>
    ShardUpFallStart,
    /// <summary>The shard that flies straight up from the crack and has no mirror image, swaying as it falls.</summary>
    ShardUpFalling,
    /// <summary>The shard that drops nearly straight down from nearer the crack, flying out of the tube.</summary>
    ShardDownInnerFlying,
    /// <summary>The shard that drops nearly straight down from nearer the crack, starting its fall (#2276).</summary>
    ShardDownInnerFallStart,
    /// <summary>The shard that drops nearly straight down from nearer the crack, swaying as it falls.</summary>
    ShardDownInnerFalling,
    /// <summary>A released-air bubble rising out of the tube, then swaying as it falls.</summary>
    ReleasedAirBubble,
}
