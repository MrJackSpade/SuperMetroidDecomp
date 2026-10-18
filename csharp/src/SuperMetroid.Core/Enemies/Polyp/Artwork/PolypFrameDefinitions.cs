using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Polyp.Artwork;

/// <summary>
/// The presentation boundary of Polyp's programs (#641): the bank-$A2 spritemap each
/// <see cref="PolypFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class PolypFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(PolypFrame frame) => frame.Pose switch
    {
        0 => 0xb5fb,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Polyp pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(PolypFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.Polyp00,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the PolypFrame programs show."),
    };
}
