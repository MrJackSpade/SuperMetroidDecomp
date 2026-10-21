namespace SuperMetroid.Core.Enemies.YappingMaws.Artwork;

/// <summary>Development-tool members of <see cref="YappingMawFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class YappingMawFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(YappingMawFrame frame) => frame.Pose switch
    {
        0 => 0xa8ee,
        1 => 0xa909,
        2 => 0xa91a,
        3 => 0xa935,
        4 => 0xa946,
        5 => 0xa961,
        6 => 0xa972,
        7 => 0xa98d,
        8 => 0xa99e,
        9 => 0xa9b9,
        10 => 0xa9ca,
        11 => 0xa9e5,
        12 => 0xa9f6,
        13 => 0xaa11,
        14 => 0xaa22,
        15 => 0xaa3d,
        16 => 0xaa4e,
        17 => 0xaa69,
        18 => 0xaa7a,
        19 => 0xaa95,
        20 => 0xaaa6,
        21 => 0xaac1,
        22 => 0xaad2,
        23 => 0xaaed,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Yapping Maw pose."),
    };
}
