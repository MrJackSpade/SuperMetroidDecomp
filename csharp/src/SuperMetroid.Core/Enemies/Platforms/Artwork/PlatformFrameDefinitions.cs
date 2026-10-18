using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Platforms.Artwork;

/// <summary>
/// The presentation boundary of the Tripper's and Kamer's programs (#641): the bank-$A3 spritemap each
/// <see cref="PlatformFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class PlatformFrameDefinitions
{

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
