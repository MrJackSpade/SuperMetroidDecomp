namespace SuperMetroid.Core.Enemies.Metroids.Artwork;

/// <summary>Development-tool members of <see cref="MetroidFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class MetroidFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MetroidFrame frame) => frame.Pose switch
    {
        0 => 0xf10d,
        1 => 0xf137,
        2 => 0xf157,
        3 => 0xf181,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Metroid pose."),
    };
}
