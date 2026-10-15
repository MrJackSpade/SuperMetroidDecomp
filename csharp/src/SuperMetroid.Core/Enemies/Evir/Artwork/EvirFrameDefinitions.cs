namespace SuperMetroid.Core.Enemies.Evir.Artwork;

/// <summary>
/// The presentation boundary of the Evir's programs (#641): the bank-$A8 spritemap each
/// <see cref="EvirFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class EvirFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EvirFrame frame) => frame.Pose switch
    {
        0 => 0x8b59,
        1 => 0x8b88,
        2 => 0x8bb7,
        3 => 0x8be6,
        4 => 0x8c15,
        5 => 0x8c44,
        6 => 0x8ca2,
        7 => 0x8cbd,
        8 => 0x8cd8,
        9 => 0x8cf3,
        10 => 0x8d13,
        11 => 0x8d64,
        12 => 0x8d7a,
        13 => 0x8d81,
        14 => 0x8db0,
        15 => 0x8ddf,
        16 => 0x8e0e,
        17 => 0x8e3d,
        18 => 0x8e6c,
        19 => 0x8eca,
        20 => 0x8ee5,
        21 => 0x8f00,
        22 => 0x8f1b,
        23 => 0x8f3b,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Evir pose."),
    };
}
