namespace SuperMetroid.Core.Enemies.Zoa.Artwork;

/// <summary>Development-tool members of <see cref="ZoaFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class ZoaFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ZoaFrame frame) => frame.Pose switch
    {
        0 => 0xb55f,
        1 => 0xb566,
        2 => 0xb56d,
        3 => 0xb574,
        4 => 0xb57b,
        5 => 0xb582,
        6 => 0xb589,
        7 => 0xb590,
        8 => 0xb597,
        9 => 0xb59e,
        10 => 0xb5a5,
        11 => 0xb5ac,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Zoa pose."),
    };
}
