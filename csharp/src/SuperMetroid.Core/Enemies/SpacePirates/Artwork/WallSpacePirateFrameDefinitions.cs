using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates.Artwork;

/// <summary>
/// The presentation boundary of the wall Space Pirates' programs (#641): the bank-$B2 spritemap each
/// <see cref="WallSpacePirateFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class WallSpacePirateFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(WallSpacePirateFrame frame) => frame.Pose switch
    {
        0 => 0x88a0,
        1 => 0x88b2,
        2 => 0x88c4,
        3 => 0x88d6,
        4 => 0x88e8,
        5 => 0x88fa,
        6 => 0x890c,
        7 => 0x891e,
        8 => 0x8928,
        9 => 0x8932,
        10 => 0x8944,
        11 => 0x8956,
        12 => 0x8968,
        13 => 0x897a,
        14 => 0x898c,
        15 => 0x899e,
        16 => 0x89b0,
        17 => 0x89ba,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Wall Space Pirate pose."),
    };

    /// <summary>The installed frame each <see cref="WallSpacePirateFrame"/> shows.</summary>
    internal static EnemyFrameTable<WallSpacePirateFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.WallPirateClimbLeft0 },
        { new(Pose: 1), EnemyFrameId.WallPirateClimbLeft1 },
        { new(Pose: 10), EnemyFrameId.WallPirateClimbRight1 },
        { new(Pose: 11), EnemyFrameId.WallPirateClimbRight2 },
        { new(Pose: 12), EnemyFrameId.WallPirateClimbRight3 },
        { new(Pose: 13), EnemyFrameId.WallPirateClimbRight4 },
        { new(Pose: 14), EnemyFrameId.WallPirateFireJumpLeft0 },
        { new(Pose: 15), EnemyFrameId.WallPirateFireJumpLeft1 },
        { new(Pose: 16), EnemyFrameId.WallPirateFireJumpLeft2 },
        { new(Pose: 17), EnemyFrameId.WallPirateFireJumpLeft3 },
        { new(Pose: 2), EnemyFrameId.WallPirateClimbLeft2 },
        { new(Pose: 3), EnemyFrameId.WallPirateClimbLeft3 },
        { new(Pose: 4), EnemyFrameId.WallPirateClimbLeft4 },
        { new(Pose: 5), EnemyFrameId.WallPirateFireJumpRight0 },
        { new(Pose: 6), EnemyFrameId.WallPirateFireJumpRight1 },
        { new(Pose: 7), EnemyFrameId.WallPirateFireJumpRight2 },
        { new(Pose: 8), EnemyFrameId.WallPirateFireJumpRight3 },
        { new(Pose: 9), EnemyFrameId.WallPirateClimbRight0 },
    };
}
