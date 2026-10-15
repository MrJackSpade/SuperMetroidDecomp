namespace SuperMetroid.Core.Enemies.Kago.Artwork;

/// <summary>
/// The presentation boundary of the Kago's bugs programs (#641): the installed artwork key of each
/// <see cref="KagoBugProjectileFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class KagoBugProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 11;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(KagoBugProjectileFrame frame) => frame.Pose switch
    {
        0 => 0xd03e,
        1 => 0xd044,
        2 => 0xd04c,
        3 => 0xd054,
        4 => 0xd058,
        5 => 0xd05e,
        6 => 0xd068,
        7 => 0xd06c,
        8 => 0xd070,
        9 => 0xd074,
        10 => 0xd078,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Kago bug pose."),
    };
}
