using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boulder.Artwork;

/// <summary>
/// The presentation boundary of the Boulder's programs (#641): the bank-$A6 spritemap each
/// <see cref="BoulderFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class BoulderFrameDefinitions
{

    /// <summary>The installed frame each <see cref="BoulderFrame"/> shows.</summary>
    internal static EnemyFrameTable<BoulderFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.BoulderRoll0 },
        { new(Pose: 1), EnemyFrameId.BoulderRoll1 },
        { new(Pose: 2), EnemyFrameId.BoulderRoll2 },
        { new(Pose: 3), EnemyFrameId.BoulderRoll3 },
        { new(Pose: 4), EnemyFrameId.BoulderRoll4 },
        { new(Pose: 5), EnemyFrameId.BoulderRoll5 },
        { new(Pose: 6), EnemyFrameId.BoulderRoll6 },
        { new(Pose: 7), EnemyFrameId.BoulderRoll7 },
    };
}
