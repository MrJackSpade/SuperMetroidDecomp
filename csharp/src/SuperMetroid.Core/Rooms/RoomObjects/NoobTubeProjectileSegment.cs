namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The n00b tube's crack, glass shard and released-air bubble animation segments (#641), in authored order. Each shard
/// has one flicker, showing it at its column and mirrored across the tube, or hiding it, which it plays through its
/// flight out of the tube and again through its swaying fall (#2376); every shard starts its fall alike. The animation
/// its spawn starts is keyed by the shard (<see cref="NoobTubeShard"/>, #2044).
/// </summary>
internal enum NoobTubeProjectileSegment
{
    /// <summary>The crack spreading across the glass, shattering and its pieces falling.</summary>
    Crack,
    /// <summary>The fallen crack's last pieces, after which it is deleted.</summary>
    CrackFallen,
    /// <summary>The deletion that ends the crack and every shard (#2276).</summary>
    Deletion,
    /// <summary>The flicker of the shard that flies outward and down.</summary>
    ShardDownOutwardFlicker,
    /// <summary>A shard turning to its swaying fall (#2376): every shard starts its fall alike.</summary>
    ShardFallStart,
    /// <summary>The flicker of the shard that flies outward and up.</summary>
    ShardUpOutwardFlicker,
    /// <summary>The flicker of the shard that flies steeply up.</summary>
    ShardUpSteepFlicker,
    /// <summary>The flicker of the shard that flies diagonally up from nearer the crack.</summary>
    ShardUpDiagonalInnerFlicker,
    /// <summary>The flicker of the shard that flies diagonally up from farther along the tube.</summary>
    ShardUpDiagonalOuterFlicker,
    /// <summary>The flicker of the shard that flies diagonally down.</summary>
    ShardDownDiagonalFlicker,
    /// <summary>The flicker of the shard that drops nearly straight down from farther along the tube.</summary>
    ShardDownOuterFlicker,
    /// <summary>The flicker of the shard that flies outward, almost level.</summary>
    ShardOutwardFlicker,
    /// <summary>The flicker of the shard that flies straight up from the crack, hiding it on alternate updates: it has no mirror image.</summary>
    ShardUpFlicker,
    /// <summary>The flicker of the shard that drops nearly straight down from nearer the crack.</summary>
    ShardDownInnerFlicker,
    /// <summary>A released-air bubble rising out of the tube, then swaying as it falls.</summary>
    ReleasedAirBubble,
}
