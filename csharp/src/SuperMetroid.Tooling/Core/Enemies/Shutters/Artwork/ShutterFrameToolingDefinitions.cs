namespace SuperMetroid.Core.Enemies.Shutters.Artwork;

/// <summary>Development-tool members of <see cref="ShutterFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class ShutterFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ShutterFrame frame) => frame.Pose switch
    {
        0 => 0xed44,
        1 => 0xed57,
        2 => 0xed74,
        3 => 0xed9b,
        4 => 0xedb1,
        5 => 0xf468,
        6 => 0xf474,
        7 => 0xf480,
        8 => 0xf48c,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined shutter pose."),
    };
}
