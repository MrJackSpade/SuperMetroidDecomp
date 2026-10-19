using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.Cacatac.Artwork;

/// <summary>Development-tool members of <see cref="CacatacFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class CacatacFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(CacatacFrame frame) => frame.Pose switch
    {
        0 => 0xa0bb,
        1 => 0xa0db,
        2 => 0xa0fb,
        3 => 0xa11b,
        4 => 0xa13b,
        5 => 0xa15b,
        6 => 0xa17b,
        7 => 0xa19b,
        8 => 0xa1bb,
        9 => 0xa1ef,
        10 => 0xa223,
        11 => 0xa243,
        12 => 0xa263,
        13 => 0xa283,
        14 => 0xa2a3,
        15 => 0xa2c3,
        16 => 0xa2e3,
        17 => 0xa303,
        18 => 0xa323,
        19 => 0xa357,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Cacatac pose."),
    };
}
