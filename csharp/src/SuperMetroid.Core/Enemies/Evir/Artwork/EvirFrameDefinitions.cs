using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Evir.Artwork;

/// <summary>
/// The presentation boundary of the Evir's programs (#641): the bank-$A8 spritemap each
/// <see cref="EvirFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class EvirFrameDefinitions
{

    /// <summary>The installed frame each <see cref="EvirFrame"/> shows.</summary>
    internal static EnemyFrameTable<EvirFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.EvirBodyLeft0 },
        { new(Pose: 1), EnemyFrameId.EvirBodyLeft1 },
        { new(Pose: 10), EnemyFrameId.EvirArmsLeft4 },
        { new(Pose: 11), EnemyFrameId.EvirProjectileRegenerating },
        { new(Pose: 12), EnemyFrameId.EvirProjectileFlight },
        { new(Pose: 13), EnemyFrameId.EvirBodyRight0 },
        { new(Pose: 14), EnemyFrameId.EvirBodyRight1 },
        { new(Pose: 15), EnemyFrameId.EvirBodyRight2 },
        { new(Pose: 16), EnemyFrameId.EvirBodyRight3 },
        { new(Pose: 17), EnemyFrameId.EvirBodyRight4 },
        { new(Pose: 18), EnemyFrameId.EvirBodyRight5 },
        { new(Pose: 19), EnemyFrameId.EvirArmsRight0 },
        { new(Pose: 2), EnemyFrameId.EvirBodyLeft2 },
        { new(Pose: 20), EnemyFrameId.EvirArmsRight1 },
        { new(Pose: 21), EnemyFrameId.EvirArmsRight2 },
        { new(Pose: 22), EnemyFrameId.EvirArmsRight3 },
        { new(Pose: 23), EnemyFrameId.EvirArmsRight4 },
        { new(Pose: 3), EnemyFrameId.EvirBodyLeft3 },
        { new(Pose: 4), EnemyFrameId.EvirBodyLeft4 },
        { new(Pose: 5), EnemyFrameId.EvirBodyLeft5 },
        { new(Pose: 6), EnemyFrameId.EvirArmsLeft0 },
        { new(Pose: 7), EnemyFrameId.EvirArmsLeft1 },
        { new(Pose: 8), EnemyFrameId.EvirArmsLeft2 },
        { new(Pose: 9), EnemyFrameId.EvirArmsLeft3 },
    };
}
