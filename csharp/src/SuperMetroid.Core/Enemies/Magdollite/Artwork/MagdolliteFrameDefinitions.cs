namespace SuperMetroid.Core.Enemies.Magdollite.Artwork;

/// <summary>
/// The presentation boundary of the Magdollite's programs (#641): the bank-$A8 spritemap each
/// <see cref="MagdolliteFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MagdolliteFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MagdolliteFrame frame) => frame.Pose switch
    {
        0 => 0xb448,
        1 => 0xb459,
        2 => 0xb46a,
        3 => 0xb47b,
        4 => 0xb48c,
        5 => 0xb49d,
        6 => 0xb4a9,
        7 => 0xb4b5,
        8 => 0xb4c1,
        9 => 0xb4cf,
        10 => 0xb4e0,
        11 => 0xb4f1,
        12 => 0xb502,
        13 => 0xb513,
        14 => 0xb524,
        15 => 0xb535,
        16 => 0xb546,
        17 => 0xb552,
        18 => 0xb56a,
        19 => 0xb578,
        20 => 0xb589,
        21 => 0xb59a,
        22 => 0xb5a1,
        23 => 0xb5ad,
        24 => 0xb5be,
        25 => 0xb5d4,
        26 => 0xb5ef,
        27 => 0xb60f,
        28 => 0xb634,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Magdollite pose."),
    };
}
