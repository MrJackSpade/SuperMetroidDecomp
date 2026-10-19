using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Flies.Artwork;

/// <summary>
/// The presentation boundary of the Mellow, Mella and Memu programs (#641): the bank-$A2 spritemap each
/// <see cref="FlyFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class FlyFrameDefinitions
{

    /// <summary>The installed frame each <see cref="FlyFrame"/> shows.</summary>
    internal static EnemyFrameTable<FlyFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.FlyShared0 },
        { new(Pose: 1), EnemyFrameId.FlyShared1 },
        { new(Pose: 2), EnemyFrameId.FlyShared2 },
        { new(Pose: 3), EnemyFrameId.FlyShared3 },
    };
}
