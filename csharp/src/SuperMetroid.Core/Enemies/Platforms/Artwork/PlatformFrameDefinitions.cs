using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Platforms.Artwork;

/// <summary>
/// The presentation boundary of the Tripper's and Kamer's programs (#641): the bank-$A3 spritemap each
/// <see cref="PlatformFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class PlatformFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(PlatformFrame frame) => frame.Pose switch
    {
        0 => 0x9f29,
        1 => 0x9f3a,
        2 => 0x9f4b,
        3 => 0x9f5c,
        4 => 0x9f6d,
        5 => 0x9f7e,
        6 => 0x9f8f,
        7 => 0x9fa5,
        8 => 0x9fb6,
        9 => 0x9fcc,
        10 => 0x9fe2,
        11 => 0x9ff3,
        12 => 0xa021,
        13 => 0xa02d,
        14 => 0xa039,
        15 => 0xa045,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Tripper or Kamer pose."),
    };

    /// <summary>The installed frame each <see cref="PlatformFrame"/> shows.</summary>
    internal static EnemyFrameTable<PlatformFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.TripperMovingLeft0 },
        { new(Pose: 1), EnemyFrameId.TripperMovingLeft1 },
        { new(Pose: 10), EnemyFrameId.TripperStillMovingRight1 },
        { new(Pose: 11), EnemyFrameId.TripperStillMovingRight2 },
        { new(Pose: 12), EnemyFrameId.TripperKamerPlatform0 },
        { new(Pose: 13), EnemyFrameId.TripperKamerPlatform1 },
        { new(Pose: 14), EnemyFrameId.TripperKamerPlatform2 },
        { new(Pose: 15), EnemyFrameId.TripperKamerPlatform3 },
        { new(Pose: 2), EnemyFrameId.TripperMovingLeft2 },
        { new(Pose: 3), EnemyFrameId.TripperMovingRight0 },
        { new(Pose: 4), EnemyFrameId.TripperMovingRight1 },
        { new(Pose: 5), EnemyFrameId.TripperMovingRight2 },
        { new(Pose: 6), EnemyFrameId.TripperStillMovingLeft0 },
        { new(Pose: 7), EnemyFrameId.TripperStillMovingLeft1 },
        { new(Pose: 8), EnemyFrameId.TripperStillMovingLeft2 },
        { new(Pose: 9), EnemyFrameId.TripperStillMovingRight0 },
    };
}
