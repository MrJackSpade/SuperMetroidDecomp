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
        { new(Kind: SporeSpawnFrameKind.Closed, Pose: 0), EnemyFrameId.SporeSpawnOamEE6F },
        { new(Kind: SporeSpawnFrameKind.Dead, Pose: 0), EnemyFrameId.SporeSpawnOamEE65 },
        { new(Kind: SporeSpawnFrameKind.Open, Pose: 0), EnemyFrameId.SporeSpawnOamEF3D },
        { new(Kind: SporeSpawnFrameKind.Open, Pose: 1), EnemyFrameId.SporeSpawnOamEF4F },
        { new(Kind: SporeSpawnFrameKind.Open, Pose: 2), EnemyFrameId.SporeSpawnOamEF61 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 0), EnemyFrameId.SporeSpawnOamEE79 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 1), EnemyFrameId.SporeSpawnOamEE8B },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 2), EnemyFrameId.SporeSpawnOamEE9D },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 3), EnemyFrameId.SporeSpawnOamEEAF },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 4), EnemyFrameId.SporeSpawnOamEEC1 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 5), EnemyFrameId.SporeSpawnOamEED3 },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 6), EnemyFrameId.SporeSpawnOamEEE5 },
    };
}
