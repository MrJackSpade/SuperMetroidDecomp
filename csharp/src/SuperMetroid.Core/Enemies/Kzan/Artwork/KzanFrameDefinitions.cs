namespace SuperMetroid.Core.Enemies.Kzan.Artwork;

/// <summary>
/// The presentation boundary of Kzan's programs (#641): the bank-$A6 spritemap each
/// <see cref="KzanFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class KzanFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(KzanFrame frame) => frame.Pose switch
    {
        0 => 0x8ce5,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Kzan pose."),
    };
}
