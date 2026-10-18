using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Skree.Artwork;

/// <summary>
/// The presentation boundary of the Skree's and Metaree's programs (#641): the bank-$A3 spritemap each
/// <see cref="SkreeMetareeFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class SkreeMetareeFrameDefinitions
{

    /// <summary>The installed frame each <see cref="SkreeMetareeFrame"/> shows.</summary>
    internal static EnemyFrameTable<SkreeMetareeFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.MetareeIdle0 },
        { new(Pose: 1), EnemyFrameId.MetareePrepare1 },
        { new(Pose: 2), EnemyFrameId.MetareeIdle1 },
        { new(Pose: 3), EnemyFrameId.MetareeIdle2 },
        { new(Pose: 4), EnemyFrameId.MetareeIdle3 },
        { new(Pose: 5), EnemyFrameId.SkreeIdle0 },
        { new(Pose: 6), EnemyFrameId.SkreePrepare1 },
        { new(Pose: 7), EnemyFrameId.SkreeIdle1 },
        { new(Pose: 8), EnemyFrameId.SkreeIdle2 },
        { new(Pose: 9), EnemyFrameId.SkreeIdle3 },
    };
}
