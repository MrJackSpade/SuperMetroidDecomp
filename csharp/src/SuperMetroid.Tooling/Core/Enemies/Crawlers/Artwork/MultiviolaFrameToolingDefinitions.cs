namespace SuperMetroid.Core.Enemies.Crawlers.Artwork;

/// <summary>Development-tool members of <see cref="MultiviolaFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class MultiviolaFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MultiviolaFrame frame) => frame.Pose switch
    {
        0 => 0xb4aa,
        1 => 0xb4b1,
        2 => 0xb4b8,
        3 => 0xb4bf,
        4 => 0xb4c6,
        5 => 0xb4cd,
        6 => 0xb4d4,
        7 => 0xb4db,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Multiviola pose."),
    };
}
