namespace SuperMetroid.Core.Enemies.Corpses.Artwork;

/// <summary>
/// The presentation boundary of Dead Torizo's programs (#641): the bank-$A9 spritemap each
/// <see cref="DeadTorizoFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class DeadTorizoFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa9;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(DeadTorizoFrame frame) => frame.Pose switch
    {
        0 => 0xd6e2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Dead Torizo pose."),
    };
}
