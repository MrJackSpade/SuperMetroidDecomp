using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Gunship.Artwork;

/// <summary>
/// The presentation boundary of the gunship's hull and entrance pad programs (#641): the bank-$A2 spritemap each
/// <see cref="GunshipFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class GunshipFrameDefinitions
{

    /// <summary>The installed frame each <see cref="GunshipFrame"/> shows.</summary>
    internal static EnemyFrameTable<GunshipFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.GunshipTopHull },
        { new(Pose: 1), EnemyFrameId.GunshipBottomHull },
        { new(Pose: 10), EnemyFrameId.GunshipEntrance10 },
        { new(Pose: 11), EnemyFrameId.GunshipEntrance01 },
        { new(Pose: 12), EnemyFrameId.GunshipEntrance00 },
        { new(Pose: 2), EnemyFrameId.GunshipEntrance02 },
        { new(Pose: 3), EnemyFrameId.GunshipEntrance03 },
        { new(Pose: 4), EnemyFrameId.GunshipEntrance04 },
        { new(Pose: 5), EnemyFrameId.GunshipEntrance05 },
        { new(Pose: 6), EnemyFrameId.GunshipEntrance06 },
        { new(Pose: 7), EnemyFrameId.GunshipEntrance07 },
        { new(Pose: 8), EnemyFrameId.GunshipEntrance08 },
        { new(Pose: 9), EnemyFrameId.GunshipEntrance09 },
    };
}
