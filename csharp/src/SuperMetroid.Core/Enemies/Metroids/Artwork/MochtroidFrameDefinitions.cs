namespace SuperMetroid.Core.Enemies.Metroids.Artwork;

/// <summary>
/// The presentation boundary of Mochtroid's programs (#641): the bank-$A3 spritemap each
/// <see cref="MochtroidFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MochtroidFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MochtroidFrame frame) => frame.Pose switch
    {
        0 => 0xa9b0,
        1 => 0xa9d0,
        2 => 0xa9f0,
        3 => 0xaa06,
        4 => 0xaa1c,
        5 => 0xaa32,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Mochtroid pose."),
    };
}
