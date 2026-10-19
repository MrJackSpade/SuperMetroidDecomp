using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;

/// <summary>Development-tool members of <see cref="MaridiaLargeSnailFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class MaridiaLargeSnailFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MaridiaLargeSnailFrame frame) => frame.Pose switch
    {
        0 => 0xcb87,
        1 => 0xcb91,
        2 => 0xcb9b,
        3 => 0xcba5,
        4 => 0xcbaf,
        5 => 0xcbb9,
        6 => 0xcbc3,
        7 => 0xcbcd,
        8 => 0xcbd7,
        9 => 0xcbe1,
        10 => 0xcbeb,
        11 => 0xcbf5,
        12 => 0xcbff,
        13 => 0xcc09,
        14 => 0xcc13,
        15 => 0xcc1d,
        16 => 0xcc27,
        17 => 0xcc31,
        18 => 0xcc3b,
        19 => 0xcc45,
        20 => 0xcc4f,
        21 => 0xcc59,
        22 => 0xcc63,
        23 => 0xcc6d,
        24 => 0xcc77,
        25 => 0xcc81,
        26 => 0xcc8b,
        27 => 0xcc95,
        28 => 0xcc9f,
        29 => 0xcca9,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Oum pose."),
    };
}
