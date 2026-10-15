namespace SuperMetroid.Core.Enemies.Alcoon.Artwork;

/// <summary>
/// The presentation boundary of Alcoon's fireball programs (#641): the installed artwork key of each
/// <see cref="AlcoonFireballFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class AlcoonFireballFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 4;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(AlcoonFireballFrame frame) => frame.Pose switch
    {
        0 => 0x9ea0,
        1 => 0x9ea4,
        2 => 0x9ea8,
        3 => 0x9eac,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Alcoon fireball pose."),
    };

    /// <summary>
    /// The bank-$8D spritemap the native operand of <paramref name="frame"/> selects, for artwork catalogs that resolve
    /// the frame through its spritemap rather than an installed composition.
    /// </summary>
    internal static ushort Spritemap(AlcoonFireballFrame frame) => frame.Pose switch
    {
        0 => 0x8404,
        1 => 0x840b,
        2 => 0x8412,
        3 => 0x8419,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Alcoon fireball pose."),
    };

    /// <summary>The pose whose installed artwork key is <paramref name="key"/>, if one is.</summary>
    internal static bool TryFrame(ushort key, out AlcoonFireballFrame frame)
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
