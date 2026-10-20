namespace SuperMetroid.Core.Enemies.Rios.Artwork;

/// <summary>Development-tool members of <see cref="NorfairRioFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class NorfairRioFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(NorfairRioFrame frame) => frame.Pose switch
    {
        0 => 0xc427,
        1 => 0xc442,
        2 => 0xc45d,
        3 => 0xc493,
        4 => 0xc49f,
        5 => 0xc4ab,
        6 => 0xc4b7,
        7 => 0xc4d2,
        8 => 0xc4ed,
        9 => 0xc508,
        10 => 0xc523,
        11 => 0xc534,
        12 => 0xc54f,
        13 => 0xc56a,
        14 => 0xc585,
        15 => 0xc5a0,
        16 => 0xc5bb,
        17 => 0xc5d6,
        18 => 0xc5e2,
        19 => 0xc5ee,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Norfair Rio pose."),
    };
}
