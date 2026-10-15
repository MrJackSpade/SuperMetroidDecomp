namespace SuperMetroid.Core.Enemies.Nuclear.Artwork;

/// <summary>
/// The presentation boundary of the Nuclear Waffle's body programs (#641): the installed artwork key of each
/// <see cref="NuclearWaffleProjectileFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class NuclearWaffleProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 12;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(NuclearWaffleProjectileFrame frame) => frame.Pose switch
    {
        0 => 0xbb60,
        1 => 0xbb64,
        2 => 0xbb68,
        3 => 0xbb6c,
        4 => 0xbb70,
        5 => 0xbb74,
        6 => 0xbb78,
        7 => 0xbb7c,
        8 => 0xbb80,
        9 => 0xbb84,
        10 => 0xbb88,
        11 => 0xbb8c,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Nuclear Waffle body pose."),
    };
}
