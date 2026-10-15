namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of the Rinkas' programs (#641): the bank-$A2 spritemap each
/// <see cref="RinkaFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class RinkaFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(RinkaFrame frame) => frame.Pose switch
    {
        0 => 0xba38,
        1 => 0xba3f,
        2 => 0xba46,
        3 => 0xba4d,
        4 => 0xba54,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Rinka pose."),
    };
}
