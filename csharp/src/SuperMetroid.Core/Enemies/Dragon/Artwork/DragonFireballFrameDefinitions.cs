namespace SuperMetroid.Core.Enemies.Dragon.Artwork;

/// <summary>
/// The presentation boundary of Dragon's fireball programs (#641): the installed artwork key of each
/// <see cref="DragonFireballFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class DragonFireballFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 8;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(DragonFireballFrame frame) => frame.Pose switch
    {
        0 => 0xb4c1,
        1 => 0xb4c5,
        2 => 0xb4cd,
        3 => 0xb4d1,
        4 => 0xb4d9,
        5 => 0xb4dd,
        6 => 0xb4e5,
        7 => 0xb4e9,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Dragon fireball pose."),
    };
}
