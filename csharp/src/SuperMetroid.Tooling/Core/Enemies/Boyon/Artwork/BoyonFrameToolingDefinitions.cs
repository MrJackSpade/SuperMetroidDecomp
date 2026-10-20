namespace SuperMetroid.Core.Enemies.Boyon.Artwork;

/// <summary>Development-tool members of <see cref="BoyonFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class BoyonFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(BoyonFrame frame) => frame.Pose switch
    {
        0 => 0x88da,
        1 => 0x88e1,
        2 => 0x88e8,
        3 => 0x88ef,
        4 => 0x88f6,
        5 => 0x88fd,
        6 => 0x8904,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Boyon pose."),
    };
}
