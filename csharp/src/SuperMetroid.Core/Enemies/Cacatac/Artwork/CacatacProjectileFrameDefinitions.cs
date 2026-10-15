namespace SuperMetroid.Core.Enemies.Cacatac.Artwork;

/// <summary>
/// The presentation boundary of the Cacatac's spike programs (#641): the installed artwork key of each
/// <see cref="CacatacProjectileFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class CacatacProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 10;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(CacatacProjectileFrame frame) => frame.Pose switch
    {
        0 => 0xd930,
        1 => 0xd936,
        2 => 0xd93c,
        3 => 0xd942,
        4 => 0xd948,
        5 => 0xd94e,
        6 => 0xd954,
        7 => 0xd95a,
        8 => 0xd960,
        9 => 0xd966,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Cacatac spike pose."),
    };
}
