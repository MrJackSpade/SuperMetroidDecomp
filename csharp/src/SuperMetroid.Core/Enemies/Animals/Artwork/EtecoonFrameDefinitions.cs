namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>
/// The presentation boundary of the Etecoon's programs (#641): the bank-$A7 spritemap each
/// <see cref="EtecoonFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class EtecoonFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa7;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EtecoonFrame frame) => frame.Pose switch
    {
        0 => 0xeeed,
        1 => 0xeefe,
        2 => 0xef0a,
        3 => 0xef16,
        4 => 0xef27,
        5 => 0xef38,
        6 => 0xef49,
        7 => 0xef5a,
        8 => 0xef6b,
        9 => 0xef90,
        10 => 0xefb5,
        11 => 0xefda,
        12 => 0xefff,
        13 => 0xf024,
        14 => 0xf06e,
        15 => 0xf089,
        16 => 0xf09a,
        17 => 0xf0a6,
        18 => 0xf0b2,
        19 => 0xf0c3,
        20 => 0xf0d4,
        21 => 0xf0e5,
        22 => 0xf0f6,
        23 => 0xf107,
        24 => 0xf12c,
        25 => 0xf151,
        26 => 0xf176,
        27 => 0xf19b,
        28 => 0xf1c0,
        29 => 0xf1e5,
        30 => 0xf20a,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Etecoon pose."),
    };
}
