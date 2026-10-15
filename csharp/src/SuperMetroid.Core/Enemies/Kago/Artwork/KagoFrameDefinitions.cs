namespace SuperMetroid.Core.Enemies.Kago.Artwork;

/// <summary>
/// The presentation boundary of Kago's programs (#641): the bank-$A8 spritemap each
/// <see cref="KagoFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class KagoFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 3;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(KagoFrame frame) => frame.Pose switch
    {
        0 => 0xabda,
        1 => 0xabf0,
        2 => 0xac06,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Kago pose."),
    };
}
