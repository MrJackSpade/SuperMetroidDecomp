using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.Sbug.Artwork;

/// <summary>Development-tool members of <see cref="SbugFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class SbugFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(SbugFrame frame) => frame.Pose switch
    {
        0 => 0xa67d,
        1 => 0xa684,
        2 => 0xa68b,
        3 => 0xa692,
        4 => 0xa699,
        5 => 0xa6a0,
        6 => 0xa6a7,
        7 => 0xa6ae,
        8 => 0xa6b5,
        9 => 0xa6bc,
        10 => 0xa6c3,
        11 => 0xa6ca,
        12 => 0xa6d1,
        13 => 0xa6d8,
        14 => 0xa6df,
        15 => 0xa6e6,
        16 => 0xa6ed,
        17 => 0xa6f4,
        18 => 0xa6fb,
        19 => 0xa702,
        20 => 0xa709,
        21 => 0xa710,
        22 => 0xa717,
        23 => 0xa71e,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Sbug pose."),
    };
}
