using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.LavaJumpers.Artwork;

/// <summary>
/// The presentation boundary of the Norfair lava jumper's programs (#641): the bank-$A2 spritemap each
/// <see cref="NorfairLavaJumperFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class NorfairLavaJumperFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(NorfairLavaJumperFrame frame) => frame.Pose switch
    {
        0 => 0x804d,
        1 => 0xc02c,
        2 => 0xc042,
        3 => 0xc058,
        4 => 0xc064,
        5 => 0xc070,
        6 => 0xc086,
        7 => 0xc09c,
        8 => 0xc0ad,
        9 => 0xc0b9,
        10 => 0xc0c5,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Norfair lava jumper pose."),
    };

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
