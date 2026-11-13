using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The presentation boundary of Spore Spawn's body (#641): the frame each
/// <see cref="SporeSpawnFrame"/> shows. The opening and open poses are consecutive eighteen-byte roots.
/// </summary>
internal static class SporeSpawnFrameDefinitions
{

    /// <summary>The installed frame each <see cref="SporeSpawnFrame"/> shows.</summary>
    internal static EnemyFrameTable<SporeSpawnFrame> ShownFrames { get; } = new()
    {
        { new(Kind: SporeSpawnFrameKind.Closed, Pose: 0), EnemyFrameId.SporeSpawnInitialAlive },
        { new(Kind: SporeSpawnFrameKind.Dead, Pose: 0), EnemyFrameId.SporeSpawnInitialDead },
        { new(Kind: SporeSpawnFrameKind.Open, Pose: 0), EnemyFrameId.SporeSpawnOpenPulseFirst },
        { new(Kind: SporeSpawnFrameKind.Open, Pose: 1), EnemyFrameId.SporeSpawnOpenPulseSecond },
        { new(Kind: SporeSpawnFrameKind.Open, Pose: 2), EnemyFrameId.SporeSpawnOpenPulseThird },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 0), EnemyFrameId.SporeSpawnOpeningFirst },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 1), EnemyFrameId.SporeSpawnOpeningSecond },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 2), EnemyFrameId.SporeSpawnOpeningThird },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 3), EnemyFrameId.SporeSpawnOpeningFourth },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 4), EnemyFrameId.SporeSpawnOpeningFifth },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 5), EnemyFrameId.SporeSpawnDeath },
        { new(Kind: SporeSpawnFrameKind.Opening, Pose: 6), EnemyFrameId.SporeSpawnOpeningSeventh },
    };
}
