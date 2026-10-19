using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shutters.Artwork;

/// <summary>
/// The presentation boundary of the shutters' and Kamer's programs (#641): the bank-$A2 spritemap each
/// <see cref="ShutterFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ShutterFrameDefinitions
{

    /// <summary>The installed frame each <see cref="ShutterFrame"/> shows.</summary>
    internal static EnemyFrameTable<ShutterFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.ShutterGrowing10px },
        { new(Pose: 1), EnemyFrameId.ShutterGrowing20px },
        { new(Pose: 2), EnemyFrameId.ShutterGrowing30px },
        { new(Pose: 3), EnemyFrameId.ShutterVertical40px },
        { new(Pose: 4), EnemyFrameId.ShutterHorizontal },
        { new(Pose: 5), EnemyFrameId.KamerPlatform0 },
        { new(Pose: 6), EnemyFrameId.KamerPlatform1 },
        { new(Pose: 7), EnemyFrameId.KamerPlatform2 },
        { new(Pose: 8), EnemyFrameId.KamerPlatform3 },
    };
}
