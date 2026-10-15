namespace SuperMetroid.Core.Enemies.Alcoon.Artwork;

/// <summary>
/// The presentation boundary of the Alcoon's programs (#641): the bank-$A8 spritemap each
/// <see cref="AlcoonFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class AlcoonFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(AlcoonFrame frame) => frame.Pose switch
    {
        0 => 0xdfa2,
        1 => 0xdfc2,
        2 => 0xdfe2,
        3 => 0xe007,
        4 => 0xe027,
        5 => 0xe047,
        6 => 0xe06c,
        7 => 0xe09b,
        8 => 0xe0bb,
        9 => 0xe0db,
        10 => 0xe0fb,
        11 => 0xe11b,
        12 => 0xe140,
        13 => 0xe160,
        14 => 0xe180,
        15 => 0xe1a5,
        16 => 0xe1d4,
        17 => 0xe1f4,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Alcoon pose."),
    };
}
