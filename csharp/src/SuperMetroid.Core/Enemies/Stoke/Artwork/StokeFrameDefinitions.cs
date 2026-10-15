namespace SuperMetroid.Core.Enemies.Stoke.Artwork;

/// <summary>
/// The presentation boundary of Stoke's programs (#641): the bank-$A2 spritemap each
/// <see cref="StokeFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class StokeFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 10;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(StokeFrame frame) => frame.Pose switch
    {
        0 => 0x8aca,
        1 => 0x8ad6,
        2 => 0x8ae7,
        3 => 0x8af3,
        4 => 0x8aff,
        5 => 0x8b15,
        6 => 0x8b21,
        7 => 0x8b32,
        8 => 0x8b3e,
        9 => 0x8b4a,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Stoke pose."),
    };
}
