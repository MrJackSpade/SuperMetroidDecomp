using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.KiHunters.Artwork;

/// <summary>
/// The presentation boundary of the Ki-Hunter's programs (#641): the bank-$A8 spritemap each
/// <see cref="KiHunterFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class KiHunterFrameDefinitions
{

    /// <summary>The installed frame each <see cref="KiHunterFrame"/> shows.</summary>
    internal static EnemyFrameTable<KiHunterFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.KiHunterA8Eb2e },
        { new(Pose: 1), EnemyFrameId.KiHunterA8Eb58 },
        { new(Pose: 10), EnemyFrameId.KiHunterA8Ec8b },
        { new(Pose: 11), EnemyFrameId.KiHunterA8Ec97 },
        { new(Pose: 12), EnemyFrameId.KiHunterA8Eca8 },
        { new(Pose: 13), EnemyFrameId.KiHunterA8Ecc0 },
        { new(Pose: 14), EnemyFrameId.KiHunterA8Ecea },
        { new(Pose: 15), EnemyFrameId.KiHunterA8Ed14 },
        { new(Pose: 16), EnemyFrameId.KiHunterA8Ed3e },
        { new(Pose: 17), EnemyFrameId.KiHunterA8Ed68 },
        { new(Pose: 18), EnemyFrameId.KiHunterA8Ed92 },
        { new(Pose: 19), EnemyFrameId.KiHunterA8Edbc },
        { new(Pose: 2), EnemyFrameId.KiHunterA8Eb82 },
        { new(Pose: 20), EnemyFrameId.KiHunterA8Ede6 },
        { new(Pose: 21), EnemyFrameId.KiHunterA8Ee10 },
        { new(Pose: 22), EnemyFrameId.KiHunterA8Ee3a },
        { new(Pose: 23), EnemyFrameId.KiHunterA8Ee64 },
        { new(Pose: 24), EnemyFrameId.KiHunterA8Ee8e },
        { new(Pose: 25), EnemyFrameId.KiHunterA8Eeb8 },
        { new(Pose: 26), EnemyFrameId.KiHunterA8Eee2 },
        { new(Pose: 27), EnemyFrameId.KiHunterA8Ef0c },
        { new(Pose: 28), EnemyFrameId.KiHunterA8Ef36 },
        { new(Pose: 29), EnemyFrameId.KiHunterA8Ef60 },
        { new(Pose: 3), EnemyFrameId.KiHunterA8Ebac },
        { new(Pose: 30), EnemyFrameId.KiHunterA8Ef8a },
        { new(Pose: 31), EnemyFrameId.KiHunterA8Efb4 },
        { new(Pose: 32), EnemyFrameId.KiHunterA8Efde },
        { new(Pose: 33), EnemyFrameId.KiHunterA8F00d },
        { new(Pose: 34), EnemyFrameId.KiHunterA8F03c },
        { new(Pose: 35), EnemyFrameId.KiHunterA8F06b },
        { new(Pose: 36), EnemyFrameId.KiHunterA8F09a },
        { new(Pose: 37), EnemyFrameId.KiHunterA8F0c4 },
        { new(Pose: 38), EnemyFrameId.KiHunterA8F0f3 },
        { new(Pose: 39), EnemyFrameId.KiHunterA8F122 },
        { new(Pose: 4), EnemyFrameId.KiHunterA8Ebc2 },
        { new(Pose: 40), EnemyFrameId.KiHunterA8F151 },
        { new(Pose: 5), EnemyFrameId.KiHunterA8Ebce },
        { new(Pose: 6), EnemyFrameId.KiHunterA8Ebf7 },
        { new(Pose: 7), EnemyFrameId.KiHunterA8Ec21 },
        { new(Pose: 8), EnemyFrameId.KiHunterA8Ec4b },
        { new(Pose: 9), EnemyFrameId.KiHunterA8Ec75 },
    };
}
