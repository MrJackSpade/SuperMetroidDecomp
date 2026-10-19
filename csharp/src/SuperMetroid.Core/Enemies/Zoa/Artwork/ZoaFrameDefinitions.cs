using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Zoa.Artwork;

/// <summary>
/// The presentation boundary of Zoa's programs (#641): the bank-$A3 spritemap each
/// <see cref="ZoaFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ZoaFrameDefinitions
{

    /// <summary>The installed frame each <see cref="ZoaFrame"/> shows.</summary>
    internal static EnemyFrameTable<ZoaFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.ZoaShootLeft0 },
        { new(Pose: 1), EnemyFrameId.ZoaShootLeft1 },
        { new(Pose: 10), EnemyFrameId.ZoaRiseRight0 },
        { new(Pose: 11), EnemyFrameId.ZoaRiseRight2 },
        { new(Pose: 2), EnemyFrameId.ZoaShootLeft2 },
        { new(Pose: 3), EnemyFrameId.ZoaRiseLeft1 },
        { new(Pose: 4), EnemyFrameId.ZoaRiseLeft0 },
        { new(Pose: 5), EnemyFrameId.ZoaRiseLeft2 },
        { new(Pose: 6), EnemyFrameId.ZoaShootRight0 },
        { new(Pose: 7), EnemyFrameId.ZoaShootRight1 },
        { new(Pose: 8), EnemyFrameId.ZoaShootRight2 },
        { new(Pose: 9), EnemyFrameId.ZoaRiseRight1 },
    };
}
