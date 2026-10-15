namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>
/// The presentation boundary of the escaping Dachora's programs (#641): the bank-$B3 spritemap each
/// <see cref="EscapeDachoraFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class EscapeDachoraFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xb3;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 12;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EscapeDachoraFrame frame) => frame.Pose switch
    {
        0 => 0xeb1b,
        1 => 0xeb4a,
        2 => 0xeb79,
        3 => 0xebb2,
        4 => 0xebe1,
        5 => 0xec10,
        6 => 0xec49,
        7 => 0xec78,
        8 => 0xeca7,
        9 => 0xece0,
        10 => 0xed0f,
        11 => 0xed3e,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined escape Dachora pose."),
    };
}
