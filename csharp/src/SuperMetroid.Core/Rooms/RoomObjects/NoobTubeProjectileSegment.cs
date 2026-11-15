namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The n00b tube's crack, glass shard and released-air bubble animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Each shard has the animation its spawn enters, its flight out of the tube and its swaying fall, each flickering it at
/// its column and mirrored across the tube, or hiding it.
/// </summary>
internal enum NoobTubeProjectileSegment
{
    /// <summary>The crack spreading across the glass, shattering and its pieces falling.</summary>
    Crack,
    /// <summary>The fallen crack's last pieces, after which it is deleted.</summary>
    CrackFallen,
    /// <summary>The shard that flies outward and down: the animation its spawn enters, which flies it.</summary>
    ShardDownOutward,
    /// <summary>The shard that flies outward and down, flying out of the tube.</summary>
    ShardDownOutwardFlying,
    /// <summary>The shard that flies outward and down, swaying as it falls.</summary>
    ShardDownOutwardFalling,
    /// <summary>The shard that flies outward and up: the animation its spawn enters, which flies it.</summary>
    ShardUpOutward,
    /// <summary>The shard that flies outward and up, flying out of the tube.</summary>
    ShardUpOutwardFlying,
    /// <summary>The shard that flies outward and up, swaying as it falls.</summary>
    ShardUpOutwardFalling,
    /// <summary>The shard that flies steeply up: the animation its spawn enters, which flies it.</summary>
    ShardUpSteep,
    /// <summary>The shard that flies steeply up, flying out of the tube.</summary>
    ShardUpSteepFlying,
    /// <summary>The shard that flies steeply up, swaying as it falls.</summary>
    ShardUpSteepFalling,
    /// <summary>The shard that flies diagonally up from nearer the crack: the animation its spawn enters, which flies it.</summary>
    ShardUpDiagonalInner,
    /// <summary>The shard that flies diagonally up from nearer the crack, flying out of the tube.</summary>
    ShardUpDiagonalInnerFlying,
    /// <summary>The shard that flies diagonally up from nearer the crack, swaying as it falls.</summary>
    ShardUpDiagonalInnerFalling,
    /// <summary>The shard that flies diagonally up from farther along the tube: the animation its spawn enters, which flies it.</summary>
    ShardUpDiagonalOuter,
    /// <summary>The shard that flies diagonally up from farther along the tube, flying out of the tube.</summary>
    ShardUpDiagonalOuterFlying,
    /// <summary>The shard that flies diagonally up from farther along the tube, swaying as it falls.</summary>
    ShardUpDiagonalOuterFalling,
    /// <summary>The shard that flies diagonally down: the animation its spawn enters, which flies it.</summary>
    ShardDownDiagonal,
    /// <summary>The shard that flies diagonally down, flying out of the tube.</summary>
    ShardDownDiagonalFlying,
    /// <summary>The shard that flies diagonally down, swaying as it falls.</summary>
    ShardDownDiagonalFalling,
    /// <summary>The shard that drops nearly straight down from farther along the tube: the animation its spawn enters, which flies it.</summary>
    ShardDownOuter,
    /// <summary>The shard that drops nearly straight down from farther along the tube, flying out of the tube.</summary>
    ShardDownOuterFlying,
    /// <summary>The shard that drops nearly straight down from farther along the tube, swaying as it falls.</summary>
    ShardDownOuterFalling,
    /// <summary>The shard that flies outward, almost level: the animation its spawn enters, which flies it.</summary>
    ShardOutward,
    /// <summary>The shard that flies outward, almost level, flying out of the tube.</summary>
    ShardOutwardFlying,
    /// <summary>The shard that flies outward, almost level, swaying as it falls.</summary>
    ShardOutwardFalling,
    /// <summary>The shard that flies straight up from the crack and has no mirror image: the animation its spawn enters, which flies it.</summary>
    ShardUp,
    /// <summary>The shard that flies straight up from the crack and has no mirror image, flying out of the tube.</summary>
    ShardUpFlying,
    /// <summary>The shard that flies straight up from the crack and has no mirror image, swaying as it falls.</summary>
    ShardUpFalling,
    /// <summary>The shard that drops nearly straight down from nearer the crack: the animation its spawn enters, which flies it.</summary>
    ShardDownInner,
    /// <summary>The shard that drops nearly straight down from nearer the crack, flying out of the tube.</summary>
    ShardDownInnerFlying,
    /// <summary>The shard that drops nearly straight down from nearer the crack, swaying as it falls.</summary>
    ShardDownInnerFalling,
    /// <summary>A released-air bubble rising out of the tube, then swaying as it falls.</summary>
    ReleasedAirBubble,
}
