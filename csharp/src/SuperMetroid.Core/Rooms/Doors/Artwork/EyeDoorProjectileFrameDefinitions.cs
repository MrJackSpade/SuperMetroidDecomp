namespace SuperMetroid.Core.Rooms.Doors.Artwork;

/// <summary>
/// The presentation boundary of the eye door's projectile programs (#641): the installed artwork key of each
/// <see cref="EyeDoorProjectileFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class EyeDoorProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 11;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(EyeDoorProjectileFrame frame) => frame.Pose switch
    {
        0 => 0xb5db,
        1 => 0xb5df,
        2 => 0xb5e3,
        3 => 0xb5ed,
        4 => 0xb5f7,
        5 => 0xb5fb,
        6 => 0xb5ff,
        7 => 0xb605,
        8 => 0xb609,
        9 => 0xb60d,
        10 => 0xb611,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined eye door projectile pose."),
    };
}
