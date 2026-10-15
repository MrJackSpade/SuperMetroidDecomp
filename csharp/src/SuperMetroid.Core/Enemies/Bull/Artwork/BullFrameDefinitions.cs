namespace SuperMetroid.Core.Enemies.Bull.Artwork;

/// <summary>
/// The presentation boundary of Bull's programs (#641): the bank-$A8 spritemap each
/// <see cref="BullFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class BullFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 3;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(BullFrame frame) => frame.Pose switch
    {
        0 => 0xdb76,
        1 => 0xdb8c,
        2 => 0xdba2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Bull pose."),
    };
}
