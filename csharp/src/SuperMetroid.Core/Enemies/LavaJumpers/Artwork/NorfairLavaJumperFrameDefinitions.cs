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
        { new(Pose: 0), EnemyFrameId.NorfairLavaJumper804d },
        { new(Pose: 1), EnemyFrameId.NorfairLavaJumperC02c },
        { new(Pose: 10), EnemyFrameId.NorfairLavaJumperC0c5 },
        { new(Pose: 2), EnemyFrameId.NorfairLavaJumperC042 },
        { new(Pose: 3), EnemyFrameId.NorfairLavaJumperC058 },
        { new(Pose: 4), EnemyFrameId.NorfairLavaJumperC064 },
        { new(Pose: 5), EnemyFrameId.NorfairLavaJumperC070 },
        { new(Pose: 6), EnemyFrameId.NorfairLavaJumperC086 },
        { new(Pose: 7), EnemyFrameId.NorfairLavaJumperC09c },
        { new(Pose: 8), EnemyFrameId.NorfairLavaJumperC0ad },
        { new(Pose: 9), EnemyFrameId.NorfairLavaJumperC0b9 },
    };
}
