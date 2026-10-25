using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.LavaJumpers.Artwork;

/// <summary>
/// The presentation boundary of the Norfair lava jumper's programs (#641): the bank-$A2 spritemap each
/// <see cref="NorfairLavaJumperFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class NorfairLavaJumperFrameDefinitions
{

    /// <summary>The installed frame each <see cref="NorfairLavaJumperFrame"/> shows.</summary>
    internal static EnemyFrameTable<NorfairLavaJumperFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.NorfairLavaJumperPose0 },
        { new(Pose: 1), EnemyFrameId.NorfairLavaJumperPose1 },
        { new(Pose: 10), EnemyFrameId.NorfairLavaJumperPose10 },
        { new(Pose: 2), EnemyFrameId.NorfairLavaJumperPose2 },
        { new(Pose: 3), EnemyFrameId.NorfairLavaJumperPose3 },
        { new(Pose: 4), EnemyFrameId.NorfairLavaJumperPose4 },
        { new(Pose: 5), EnemyFrameId.NorfairLavaJumperPose5 },
        { new(Pose: 6), EnemyFrameId.NorfairLavaJumperPose6 },
        { new(Pose: 7), EnemyFrameId.NorfairLavaJumperPose7 },
        { new(Pose: 8), EnemyFrameId.NorfairLavaJumperPose8 },
        { new(Pose: 9), EnemyFrameId.NorfairLavaJumperPose9 },
    };
}
