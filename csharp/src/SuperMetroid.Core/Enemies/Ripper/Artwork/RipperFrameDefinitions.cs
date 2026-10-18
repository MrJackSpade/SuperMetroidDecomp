using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Ripper.Artwork;

/// <summary>
/// The presentation boundary of the Ripper family's programs (#641): the bank-$A2 spritemap each
/// <see cref="RipperFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class RipperFrameDefinitions
{

    /// <summary><c>Spritemap_GRipper_Ripper2_Frozen_FacingLeft</c> at $A2:E43F, pinned while a GRipper or Ripper II facing left is frozen.</summary>
    internal const ushort FrozenFacingLeftSpritemap = 0xe43f;

    /// <summary><c>Spritemap_GRipper_Ripper2_Frozen_FacingRight</c> at $A2:E44B, pinned while a GRipper or Ripper II facing right is frozen.</summary>
    internal const ushort FrozenFacingRightSpritemap = 0xe44b;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(RipperFrame frame) => frame.Pose switch
    {
        0 => 0xe3c5,
        1 => 0xe3db,
        2 => 0xe3ec,
        3 => 0xe402,
        4 => 0xe418,
        5 => 0xe429,
        6 => 0xe527,
        7 => 0xe533,
        8 => 0xe53f,
        9 => 0xe54b,
        10 => 0xe557,
        11 => 0xe563,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ripper pose."),
    };

    /// <summary>The installed frame each <see cref="RipperFrame"/> shows.</summary>
    internal static EnemyFrameTable<RipperFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.RipperSharedLeft0 },
        { new(Pose: 1), EnemyFrameId.RipperSharedLeft1 },
        { new(Pose: 10), EnemyFrameId.RipperRight1 },
        { new(Pose: 11), EnemyFrameId.RipperRight2 },
        { new(Pose: 2), EnemyFrameId.RipperSharedLeft2 },
        { new(Pose: 3), EnemyFrameId.RipperSharedRight0 },
        { new(Pose: 4), EnemyFrameId.RipperSharedRight1 },
        { new(Pose: 5), EnemyFrameId.RipperSharedRight2 },
        { new(Pose: 6), EnemyFrameId.RipperLeft0 },
        { new(Pose: 7), EnemyFrameId.RipperLeft1 },
        { new(Pose: 8), EnemyFrameId.RipperLeft2 },
        { new(Pose: 9), EnemyFrameId.RipperRight0 },
    };
}
