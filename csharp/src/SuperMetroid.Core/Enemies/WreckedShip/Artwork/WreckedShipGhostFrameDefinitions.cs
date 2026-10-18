using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.WreckedShip.Artwork;

/// <summary>
/// The presentation boundary of the Wrecked Ship ghost's programs (#641): the bank-$A8 spritemap each
/// <see cref="WreckedShipGhostFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class WreckedShipGhostFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(WreckedShipGhostFrame frame) => frame.Pose switch
    {
        0 => 0x9e46,
        1 => 0x9e5c,
        2 => 0x9e72,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Wrecked Ship ghost pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(WreckedShipGhostFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.WreckedShipGhost00,
        { Pose: 1 } => EnemyFrameId.WreckedShipGhost01,
        { Pose: 2 } => EnemyFrameId.WreckedShipGhost02,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the WreckedShipGhostFrame programs show."),
    };
}
