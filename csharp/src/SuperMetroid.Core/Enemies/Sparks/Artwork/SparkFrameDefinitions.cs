using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Sparks.Artwork;

/// <summary>
/// The presentation boundary of the Wrecked Ship Spark's programs (#641): the bank-$A8 spritemap each
/// <see cref="SparkFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class SparkFrameDefinitions
{

    /// <summary>The installed frame each <see cref="SparkFrame"/> shows.</summary>
    internal static EnemyFrameTable<SparkFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.NorfairLavaJumperPose0 },
        { new(Pose: 1), EnemyFrameId.Spark04 },
        { new(Pose: 2), EnemyFrameId.Spark05 },
        { new(Pose: 3), EnemyFrameId.Spark06 },
        { new(Pose: 4), EnemyFrameId.Spark07 },
        { new(Pose: 5), EnemyFrameId.Spark00 },
        { new(Pose: 6), EnemyFrameId.Spark01 },
        { new(Pose: 7), EnemyFrameId.Spark02 },
        { new(Pose: 8), EnemyFrameId.Spark03 },
    };
}
