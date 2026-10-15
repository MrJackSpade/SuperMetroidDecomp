namespace SuperMetroid.Core.Rooms.Doors.Artwork;

/// <summary>
/// The presentation boundary of the eye-door sweat drop's programs (#1296): the installed artwork key of each
/// <see cref="EyeDoorSweatFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class EyeDoorSweatFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 4;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(EyeDoorSweatFrame frame) => frame.Pose switch
    {
        0 => 0xb617,
        1 => 0xb621,
        2 => 0xb625,
        3 => 0xb629,
        _ => throw new InvalidOperationException($"EyeDoorSweat pose {frame.Pose} is undefined."),
    };
}
