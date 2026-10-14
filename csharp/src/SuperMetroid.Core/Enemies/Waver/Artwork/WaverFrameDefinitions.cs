namespace SuperMetroid.Core.Enemies.Waver.Artwork;

/// <summary>
/// The presentation boundary of Waver's programs (#641): the bank-$A3 spritemap each
/// <see cref="WaverFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class WaverFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 10;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(WaverFrame frame) => frame.Pose switch
    {
        0 => 0x881e,
        1 => 0x8834,
        2 => 0x884a,
        3 => 0x885b,
        4 => 0x8871,
        5 => 0x8887,
        6 => 0x889d,
        7 => 0x88b3,
        8 => 0x88c4,
        9 => 0x88da,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Waver pose."),
    };
}
