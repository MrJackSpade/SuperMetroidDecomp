namespace SuperMetroid.Core.Enemies.Powamp.Artwork;

/// <summary>Development-tool members of <see cref="PowampFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class PowampFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(PowampFrame frame) => frame.Pose switch
    {
        0 => 0xc675,
        1 => 0xc67c,
        2 => 0xc683,
        3 => 0xc68a,
        4 => 0xc691,
        5 => 0xc698,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Powamp pose."),
    };
}
