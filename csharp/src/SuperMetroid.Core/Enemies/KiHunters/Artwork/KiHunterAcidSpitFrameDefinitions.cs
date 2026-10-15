namespace SuperMetroid.Core.Enemies.KiHunters.Artwork;

/// <summary>
/// The presentation boundary of the Ki-Hunter's acid spit programs (#641): the installed artwork key of each
/// <see cref="KiHunterAcidSpitFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class KiHunterAcidSpitFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 19;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(KiHunterAcidSpitFrame frame) => frame.Pose switch
    {
        0 => 0xcf36,
        1 => 0xcf3a,
        2 => 0xcf3e,
        3 => 0xcf42,
        4 => 0xcf46,
        5 => 0xcf4e,
        6 => 0xcf52,
        7 => 0xcf5a,
        8 => 0xcf5e,
        9 => 0xcf62,
        10 => 0xcf66,
        11 => 0xcf6a,
        12 => 0xcf70,
        13 => 0xcf74,
        14 => 0xcf78,
        15 => 0xcf7c,
        16 => 0xcf80,
        17 => 0xcf88,
        18 => 0xcf8c,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ki-Hunter acid spit pose."),
    };
}
