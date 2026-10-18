using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Bull.Artwork;

/// <summary>
/// The presentation boundary of Bull's programs (#641): the bank-$A8 spritemap each
/// <see cref="BullFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class BullFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(BullFrame frame) => frame.Pose switch
    {
        0 => 0xdb76,
        1 => 0xdb8c,
        2 => 0xdba2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Bull pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(BullFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.BullIdle0,
        { Pose: 1 } => EnemyFrameId.BullIdle1,
        { Pose: 2 } => EnemyFrameId.BullIdle2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the BullFrame programs show."),
    };
}
