using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boyon.Artwork;

/// <summary>
/// The presentation boundary of Boyon's programs (#641): the bank-$A2 spritemap each
/// <see cref="BoyonFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class BoyonFrameDefinitions
{

    /// <summary>The installed frame each <see cref="BoyonFrame"/> shows.</summary>
    internal static EnemyFrameTable<BoyonFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.BoyonIdle0 },
        { new(Pose: 1), EnemyFrameId.BoyonIdle1 },
        { new(Pose: 2), EnemyFrameId.BoyonIdle2 },
        { new(Pose: 3), EnemyFrameId.BoyonBounce0 },
        { new(Pose: 4), EnemyFrameId.BoyonBounce1 },
        { new(Pose: 5), EnemyFrameId.BoyonBounce2 },
        { new(Pose: 6), EnemyFrameId.BoyonBounce3 },
    };
}
