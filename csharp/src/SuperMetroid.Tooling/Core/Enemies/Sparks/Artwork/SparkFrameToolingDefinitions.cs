namespace SuperMetroid.Core.Enemies.Sparks.Artwork;

/// <summary>Development-tool members of <see cref="SparkFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class SparkFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(SparkFrame frame) => frame.Pose switch
    {
        0 => 0x804d,
        1 => 0xe71f,
        2 => 0xe72b,
        3 => 0xe737,
        4 => 0xe743,
        5 => 0xe74f,
        6 => 0xe76a,
        7 => 0xe785,
        8 => 0xe79b,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Spark pose."),
    };
}
