namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>
/// The presentation boundary of the Dachora's programs (#641): the bank-$A7 spritemap each
/// <see cref="DachoraFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class DachoraFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa7;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(DachoraFrame frame) => frame.Pose switch
    {
        0 => 0xf9c4,
        1 => 0xf9f3,
        2 => 0xfa22,
        3 => 0xfa5b,
        4 => 0xfa8a,
        5 => 0xfab9,
        6 => 0xfb1c,
        7 => 0xfb50,
        8 => 0xfb7f,
        9 => 0xfbb3,
        10 => 0xfbe7,
        11 => 0xfc16,
        12 => 0xfc45,
        13 => 0xfc74,
        14 => 0xfca3,
        15 => 0xfcd2,
        16 => 0xfd01,
        17 => 0xfd3a,
        18 => 0xfd69,
        19 => 0xfd98,
        20 => 0xfdd1,
        21 => 0xfdfb,
        22 => 0xfe2f,
        23 => 0xfe5e,
        24 => 0xfe92,
        25 => 0xfec6,
        26 => 0xfef5,
        27 => 0xff24,
        28 => 0xff53,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Dachora pose."),
    };
}
