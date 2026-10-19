using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.Beetom.Artwork;

/// <summary>Development-tool members of <see cref="BeetomFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class BeetomFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(BeetomFrame frame) => frame.Pose switch
    {
        0 => 0xbed3,
        1 => 0xbeee,
        2 => 0xbf09,
        3 => 0xbf24,
        4 => 0xbf3f,
        5 => 0xbf5a,
        6 => 0xbf75,
        7 => 0xbf90,
        8 => 0xbfab,
        9 => 0xbfcb,
        10 => 0xbfeb,
        11 => 0xc00b,
        12 => 0xc026,
        13 => 0xc041,
        14 => 0xc05c,
        15 => 0xc077,
        16 => 0xc092,
        17 => 0xc0ad,
        18 => 0xc0c8,
        19 => 0xc0e3,
        20 => 0xc103,
        21 => 0xc123,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Beetom pose."),
    };
}
