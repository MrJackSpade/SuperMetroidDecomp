namespace SuperMetroid.Core.Enemies.Shaktool.Artwork;

/// <summary>
/// The presentation boundary of the Shaktool's programs (#641): the bank-$AA spritemap each
/// <see cref="ShaktoolFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ShaktoolFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xaa;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ShaktoolFrame frame) => frame.Pose switch
    {
        0 => 0xdf5c,
        1 => 0xdf63,
        2 => 0xdf6a,
        3 => 0xdf71,
        4 => 0xdf78,
        5 => 0xdf8e,
        6 => 0xdfa4,
        7 => 0xdfba,
        8 => 0xdfd0,
        9 => 0xdfe6,
        10 => 0xdffc,
        11 => 0xe012,
        12 => 0xe028,
        13 => 0xe02f,
        14 => 0xe036,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Shaktool pose."),
    };
}
