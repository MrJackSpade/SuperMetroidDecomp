namespace SuperMetroid.Core.Enemies.LavaJumpers.Artwork;

/// <summary>Development-tool members of <see cref="NorfairLavaJumperFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class NorfairLavaJumperFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
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
}
