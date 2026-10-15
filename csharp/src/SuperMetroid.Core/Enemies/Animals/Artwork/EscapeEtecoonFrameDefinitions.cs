namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>
/// The presentation boundary of the escaping Etecoons' programs (#641): the bank-$B3 spritemap each
/// <see cref="EscapeEtecoonFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class EscapeEtecoonFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EscapeEtecoonFrame frame) => frame.Pose switch
    {
        0 => 0xe736,
        1 => 0xe747,
        2 => 0xe753,
        3 => 0xe75f,
        4 => 0xe770,
        5 => 0xe77c,
        6 => 0xe8b0,
        7 => 0xe8d5,
        8 => 0xe8fa,
        9 => 0xe91f,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined escape Etecoon pose."),
    };
}
