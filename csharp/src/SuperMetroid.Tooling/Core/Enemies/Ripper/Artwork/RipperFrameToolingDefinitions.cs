using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.Ripper.Artwork;

/// <summary>Development-tool members of <see cref="RipperFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class RipperFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(RipperFrame frame) => frame.Pose switch
    {
        0 => 0xe3c5,
        1 => 0xe3db,
        2 => 0xe3ec,
        3 => 0xe402,
        4 => 0xe418,
        5 => 0xe429,
        6 => 0xe527,
        7 => 0xe533,
        8 => 0xe53f,
        9 => 0xe54b,
        10 => 0xe557,
        11 => 0xe563,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ripper pose."),
    };
}
