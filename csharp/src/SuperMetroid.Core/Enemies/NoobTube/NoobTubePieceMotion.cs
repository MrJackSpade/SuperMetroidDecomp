namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// How a piece of the broken n00b tube moves itself (#1843): the crack, a glass shard or a released-air bubble.
/// </summary>
internal enum NoobTubePieceMotion
{
    /// <summary>Flickers the n00b-tube crack.</summary>
    CrackFlickering,

    /// <summary>Drops the detached n00b-tube crack.</summary>
    CrackFalling,

    /// <summary>Flies a newly emitted n00b-tube shard.</summary>
    ShardFlying,

    /// <summary>Sways and drops a n00b-tube shard after its first flight.</summary>
    ShardFalling,

    /// <summary>Moves a released-air bubble vertically.</summary>
    BubbleFlying,

    /// <summary>Sways and drops a released-air bubble.</summary>
    BubbleFalling,
}
