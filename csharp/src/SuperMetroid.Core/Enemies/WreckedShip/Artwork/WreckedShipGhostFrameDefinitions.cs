namespace SuperMetroid.Core.Enemies.WreckedShip.Artwork;

/// <summary>
/// The presentation boundary of the Wrecked Ship ghost's programs (#641): the bank-$A8 spritemap each
/// <see cref="WreckedShipGhostFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class WreckedShipGhostFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(WreckedShipGhostFrame frame) => frame.Pose switch
    {
        0 => 0x9e46,
        1 => 0x9e5c,
        2 => 0x9e72,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Wrecked Ship ghost pose."),
    };
}
