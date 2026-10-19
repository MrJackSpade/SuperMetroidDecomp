using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Beetom.Artwork;

/// <summary>
/// The presentation boundary of the Beetom's programs (#641): the bank-$A8 spritemap each
/// <see cref="BeetomFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class BeetomFrameDefinitions
{

    /// <summary>The installed frame each <see cref="BeetomFrame"/> shows.</summary>
    internal static EnemyFrameTable<BeetomFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.BeetomLeftCrawl0 },
        { new(Pose: 1), EnemyFrameId.BeetomLeftCrawl1 },
        { new(Pose: 10), EnemyFrameId.BeetomLeftDrain5 },
        { new(Pose: 11), EnemyFrameId.BeetomRightCrawl0 },
        { new(Pose: 12), EnemyFrameId.BeetomRightCrawl1 },
        { new(Pose: 13), EnemyFrameId.BeetomRightCrawl2 },
        { new(Pose: 14), EnemyFrameId.BeetomRightHop0 },
        { new(Pose: 15), EnemyFrameId.BeetomRightHop1 },
        { new(Pose: 16), EnemyFrameId.BeetomRightDrain0 },
        { new(Pose: 17), EnemyFrameId.BeetomRightDrain1 },
        { new(Pose: 18), EnemyFrameId.BeetomRightDrain2 },
        { new(Pose: 19), EnemyFrameId.BeetomRightDrain3 },
        { new(Pose: 2), EnemyFrameId.BeetomLeftCrawl2 },
        { new(Pose: 20), EnemyFrameId.BeetomRightDrain4 },
        { new(Pose: 21), EnemyFrameId.BeetomRightDrain5 },
        { new(Pose: 3), EnemyFrameId.BeetomLeftHop0 },
        { new(Pose: 4), EnemyFrameId.BeetomLeftHop1 },
        { new(Pose: 5), EnemyFrameId.BeetomLeftDrain0 },
        { new(Pose: 6), EnemyFrameId.BeetomLeftDrain1 },
        { new(Pose: 7), EnemyFrameId.BeetomLeftDrain2 },
        { new(Pose: 8), EnemyFrameId.BeetomLeftDrain3 },
        { new(Pose: 9), EnemyFrameId.BeetomLeftDrain4 },
    };
}
