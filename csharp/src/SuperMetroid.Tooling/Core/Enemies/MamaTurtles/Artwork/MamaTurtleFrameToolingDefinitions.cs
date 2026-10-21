namespace SuperMetroid.Core.Enemies.MamaTurtles.Artwork;

/// <summary>Development-tool members of <see cref="MamaTurtleFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class MamaTurtleFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MamaTurtleFrame frame) => frame.Pose switch
    {
        0 => 0x94d9,
        1 => 0x94e0,
        2 => 0x94e7,
        3 => 0x94ee,
        4 => 0x94f5,
        5 => 0x9501,
        6 => 0x950d,
        7 => 0x9519,
        8 => 0x9520,
        9 => 0x9527,
        10 => 0x952e,
        11 => 0x9535,
        12 => 0x9555,
        13 => 0x959d,
        14 => 0x95e5,
        15 => 0x96c9,
        16 => 0x96e9,
        17 => 0x9733,
        18 => 0x973a,
        19 => 0x9741,
        20 => 0x9748,
        21 => 0x974f,
        22 => 0x975b,
        23 => 0x9767,
        24 => 0x9773,
        25 => 0x978f,
        26 => 0x97af,
        27 => 0x97f7,
        28 => 0x983f,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Mama Turtle pose."),
    };
}
