using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Gunship.Artwork;

/// <summary>
/// The presentation boundary of the gunship's hull and entrance pad programs (#641): the bank-$A2 spritemap each
/// <see cref="GunshipFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class GunshipFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(GunshipFrame frame) => frame.Pose switch
    {
        0 => 0xad81,
        1 => 0xaddd,
        2 => 0xae89,
        3 => 0xae9f,
        4 => 0xaeb5,
        5 => 0xaecb,
        6 => 0xaef5,
        7 => 0xaf1f,
        8 => 0xaf49,
        9 => 0xaf73,
        10 => 0xaf9d,
        11 => 0xafc7,
        12 => 0xafdd,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined gunship pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(GunshipFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.GunshipTopHull,
        { Pose: 1 } => EnemyFrameId.GunshipBottomHull,
        { Pose: 10 } => EnemyFrameId.GunshipEntrance10,
        { Pose: 11 } => EnemyFrameId.GunshipEntrance01,
        { Pose: 12 } => EnemyFrameId.GunshipEntrance00,
        { Pose: 2 } => EnemyFrameId.GunshipEntrance02,
        { Pose: 3 } => EnemyFrameId.GunshipEntrance03,
        { Pose: 4 } => EnemyFrameId.GunshipEntrance04,
        { Pose: 5 } => EnemyFrameId.GunshipEntrance05,
        { Pose: 6 } => EnemyFrameId.GunshipEntrance06,
        { Pose: 7 } => EnemyFrameId.GunshipEntrance07,
        { Pose: 8 } => EnemyFrameId.GunshipEntrance08,
        { Pose: 9 } => EnemyFrameId.GunshipEntrance09,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the GunshipFrame programs show."),
    };
}
