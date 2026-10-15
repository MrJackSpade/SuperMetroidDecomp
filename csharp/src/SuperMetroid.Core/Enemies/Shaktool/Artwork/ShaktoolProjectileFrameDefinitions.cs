namespace SuperMetroid.Core.Enemies.Shaktool.Artwork;

/// <summary>
/// The presentation boundary of Shaktool's attack circles programs (#641): the installed artwork key of each
/// <see cref="ShaktoolProjectileFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class ShaktoolProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 8;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(ShaktoolProjectileFrame frame) => frame.Pose switch
    {
        0 => 0xbd6a,
        1 => 0xbd6e,
        2 => 0xbd72,
        3 => 0xbd7a,
        4 => 0xbd82,
        5 => 0xbd86,
        6 => 0xbd8e,
        7 => 0xbd96,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Shaktool attack circle pose."),
    };
}
