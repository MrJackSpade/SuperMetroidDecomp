using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The presentation boundary of Spore Spawn's body (#641): the bank-$A5 extended spritemap each
/// <see cref="SporeSpawnFrame"/> shows. The opening and open poses are consecutive eighteen-byte roots.
/// </summary>
internal static class SporeSpawnFrameDefinitions
{

    /// <summary>The installed frame each <see cref="SporeSpawnFrame"/> shows.</summary>
    internal static EnemyFrameTable<SporeSpawnFrame> ShownFrames { get; } = new()
    {
        { new(Kind: SporeSpawnFrameKind.Closed, Pose: 0), EnemyFrameId.SporeSpawnClosed0 },
        { new(Kind: SporeSpawnFrameKind.Dead, Pose: 0), EnemyFrameId.SporeSpawnDead0 },
        { new(Kind: SporeSpawnFrameKind.Open, Pose: 0), EnemyFrameId.SporeSpawnOpen0 },
        { new(Kind: SporeSpawnFrameKind.Open, Pose: 1), EnemyFrameId.SporeSpawnOpen1 },
        { new(Kind: SporeSpawnFrameKind.Open, Pose: 2), EnemyFrameId.SporeSpawnOpen2 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 0), EnemyFrameId.SporeSpawnOpening0 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 1), EnemyFrameId.SporeSpawnOpening1 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 2), EnemyFrameId.SporeSpawnOpening2 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 3), EnemyFrameId.SporeSpawnOpening3 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 4), EnemyFrameId.SporeSpawnOpening4 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 5), EnemyFrameId.SporeSpawnOpening5 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 6), EnemyFrameId.SporeSpawnOpening6 },
    };
}
