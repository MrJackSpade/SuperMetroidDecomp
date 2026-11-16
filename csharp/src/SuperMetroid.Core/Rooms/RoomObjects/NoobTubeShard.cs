namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The ten glass shards the n00b tube bursts into (#2044), each with the animation its spawn starts: its flight out of the
/// tube and its swaying fall.
/// </summary>
internal enum NoobTubeShard
{
    /// <summary>The shard that flies outward and down.</summary>
    DownOutward,
    /// <summary>The shard that flies outward and up.</summary>
    UpOutward,
    /// <summary>The shard that flies steeply up.</summary>
    UpSteep,
    /// <summary>The shard that flies diagonally up from nearer the crack.</summary>
    UpDiagonalInner,
    /// <summary>The shard that flies diagonally up from farther along the tube.</summary>
    UpDiagonalOuter,
    /// <summary>The shard that flies diagonally down.</summary>
    DownDiagonal,
    /// <summary>The shard that drops nearly straight down from farther along the tube.</summary>
    DownOuter,
    /// <summary>The shard that flies outward, almost level.</summary>
    Outward,
    /// <summary>The shard that flies straight up from the crack and has no mirror image.</summary>
    Up,
    /// <summary>The shard that drops nearly straight down from nearer the crack.</summary>
    DownInner,
}
