using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.MorphBallEye.Artwork;

/// <summary>Development-tool members of <see cref="MorphBallEyeFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class MorphBallEyeFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MorphBallEyeFrame frame) => frame.Pose switch
    {
        0 => 0x91df,
        1 => 0x91e6,
        2 => 0x91ed,
        3 => 0x91f4,
        4 => 0x91fb,
        5 => 0x9202,
        6 => 0x9209,
        7 => 0x9210,
        8 => 0x9217,
        9 => 0x921e,
        10 => 0x9225,
        11 => 0x922c,
        12 => 0x9233,
        13 => 0x923a,
        14 => 0x9241,
        15 => 0x9257,
        16 => 0x926d,
        17 => 0x9283,
        18 => 0x9299,
        19 => 0x92a5,
        20 => 0x92b1,
        21 => 0x92bd,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Morph Ball eye pose."),
    };
}
