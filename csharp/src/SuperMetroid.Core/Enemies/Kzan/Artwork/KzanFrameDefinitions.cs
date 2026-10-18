using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Kzan.Artwork;

/// <summary>
/// The presentation boundary of Kzan's programs (#641): the bank-$A6 spritemap each
/// <see cref="KzanFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class KzanFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(KzanFrame frame) => frame.Pose switch
    {
        0 => 0x8ce5,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Kzan pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(KzanFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.Kzan00,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the KzanFrame programs show."),
    };
}
