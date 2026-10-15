namespace SuperMetroid.Core.Enemies.Atomic.Artwork;

/// <summary>
/// The presentation boundary of Atomic's programs (#641): the bank-$A8 spritemap each
/// <see cref="AtomicFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class AtomicFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 12;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(AtomicFrame frame) => frame.Pose switch
    {
        0 => 0xe489,
        1 => 0xe49f,
        2 => 0xe4b5,
        3 => 0xe4cb,
        4 => 0xe4e1,
        5 => 0xe4f2,
        6 => 0xe508,
        7 => 0xe51e,
        8 => 0xe534,
        9 => 0xe54a,
        10 => 0xe560,
        11 => 0xe571,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Atomic pose."),
    };
}
