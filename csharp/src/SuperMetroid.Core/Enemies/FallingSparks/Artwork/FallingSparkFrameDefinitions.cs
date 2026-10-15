namespace SuperMetroid.Core.Enemies.FallingSparks.Artwork;

/// <summary>
/// The presentation boundary of the falling spark's programs (#641): the installed artwork key of each
/// <see cref="FallingSparkFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class FallingSparkFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 14;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(FallingSparkFrame frame) => frame.Pose switch
    {
        0 => 0xf355,
        1 => 0xf359,
        2 => 0xf35d,
        3 => 0xf365,
        4 => 0xf369,
        5 => 0xf36d,
        6 => 0xf371,
        7 => 0xf375,
        8 => 0xf379,
        9 => 0xf37d,
        10 => 0xf381,
        11 => 0xf385,
        12 => 0xf389,
        13 => 0xf38d,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined falling spark pose."),
    };
}
