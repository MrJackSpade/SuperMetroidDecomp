namespace SuperMetroid.Core.Enemies.Puyo.Artwork;

/// <summary>Development-tool members of <see cref="PuyoFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class PuyoFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(PuyoFrame frame) => frame.Pose switch
    {
        0 => 0x9df6,
        1 => 0x9e02,
        2 => 0x9e0e,
        3 => 0x9e1a,
        4 => 0x9e26,
        5 => 0x9e37,
        6 => 0x9e4d,
        7 => 0x9e5e,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Puyo pose."),
    };
}
