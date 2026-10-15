namespace SuperMetroid.Core.Enemies.FuneNamihe.Artwork;

/// <summary>
/// The presentation boundary of Fune and Namihe's fireball programs (#641): the installed artwork key of each
/// <see cref="FuneNamiheFireballFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class FuneNamiheFireballFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 6;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(FuneNamiheFireballFrame frame) => frame.Pose switch
    {
        0 => 0xde98,
        1 => 0xde9c,
        2 => 0xdea0,
        3 => 0xdea8,
        4 => 0xdeac,
        5 => 0xdeb0,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Fune/Namihe fireball pose."),
    };

    /// <summary>
    /// The bank-$8D spritemap the native operand of <paramref name="frame"/> selects, for artwork catalogs that resolve
    /// the frame through its spritemap rather than an installed composition.
    /// </summary>
    internal static ushort Spritemap(FuneNamiheFireballFrame frame) => frame.Pose switch
    {
        0 => 0xaab9,
        1 => 0xaac0,
        2 => 0xaac7,
        3 => 0xaace,
        4 => 0xaad5,
        5 => 0xaadc,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Fune/Namihe fireball pose."),
    };

    /// <summary>The pose whose installed artwork key is <paramref name="key"/>, if one is.</summary>
    internal static bool TryFrame(ushort key, out FuneNamiheFireballFrame frame)
    {
        for (int pose = 0; pose < PoseCount; pose++)
        {
            frame = new(pose);
            if (PresentationKey(frame) == key)
                return true;
        }
        frame = default;
        return false;
    }
}
