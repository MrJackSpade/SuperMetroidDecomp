namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The presentation boundary of the Ceres steam vents' programs (#641): the bank-$A6 spritemap each
/// <see cref="CeresSteamFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class CeresSteamFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(CeresSteamFrame frame) => frame.Pose switch
    {
        0 => 0xf142,
        1 => 0xf14c,
        2 => 0xf156,
        3 => 0xf160,
        4 => 0xf16a,
        5 => 0xf174,
        6 => 0xf17e,
        7 => 0xf188,
        8 => 0xf192,
        9 => 0xf19c,
        10 => 0xf1a6,
        11 => 0xf1b0,
        12 => 0xf1ba,
        13 => 0xf1c4,
        14 => 0xf1ce,
        15 => 0xf1d8,
        16 => 0xf1e2,
        17 => 0xf1ec,
        18 => 0xf1f6,
        19 => 0xf200,
        20 => 0xf20a,
        21 => 0xf214,
        22 => 0xf21e,
        23 => 0xf228,
        24 => 0xf232,
        25 => 0xf23c,
        26 => 0xf246,
        27 => 0xf250,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ceres steam pose."),
    };
}
