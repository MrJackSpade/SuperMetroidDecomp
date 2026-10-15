namespace SuperMetroid.Core.Enemies.Hoppers.Artwork;

/// <summary>
/// The presentation boundary of the Sidehoppers' and Dessgeegas' programs (#641): the bank-$A3 spritemap each
/// <see cref="HopperFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class HopperFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(HopperFrame frame) => frame.Pose switch
    {
        0 => 0xaee3,
        1 => 0xaefe,
        2 => 0xaf19,
        3 => 0xaf34,
        4 => 0xaf4f,
        5 => 0xaf6a,
        6 => 0xafe3,
        7 => 0xaffe,
        8 => 0xb019,
        9 => 0xb034,
        10 => 0xb04f,
        11 => 0xb06a,
        12 => 0xb111,
        13 => 0xb136,
        14 => 0xb15b,
        15 => 0xb194,
        16 => 0xb1b9,
        17 => 0xb1de,
        18 => 0xb273,
        19 => 0xb2a2,
        20 => 0xb2d1,
        21 => 0xb30a,
        22 => 0xb339,
        23 => 0xb368,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Hopper pose."),
    };
}
