namespace SuperMetroid.Core.Rooms.Plms.Artwork;

/// <summary>
/// The presentation boundary of the downward shot gate's programs (#641): the installed artwork key of each
/// <see cref="DownwardGateProjectileFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class DownwardGateProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 9;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(DownwardGateProjectileFrame frame) => frame.Pose switch
    {
        0 => 0xe546,
        1 => 0xe54c,
        2 => 0xe552,
        3 => 0xe558,
        4 => 0xe564,
        5 => 0xe56e,
        6 => 0xe574,
        7 => 0xe57a,
        8 => 0xe580,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined downward gate pose."),
    };
}
