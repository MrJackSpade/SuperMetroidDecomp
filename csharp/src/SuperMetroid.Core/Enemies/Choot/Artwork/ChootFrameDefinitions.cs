using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Choot.Artwork;

/// <summary>
/// The presentation boundary of Choot's programs (#641): the bank-$A2 spritemap each
/// <see cref="ChootFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ChootFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ChootFrame frame) => frame.Pose switch
    {
        0 => 0xe146,
        1 => 0xe15c,
        2 => 0xe168,
        3 => 0xe16f,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Choot pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(ChootFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.ChootIdle,
        { Pose: 1 } => EnemyFrameId.ChootJump,
        { Pose: 2 } => EnemyFrameId.ChootJumpApex,
        { Pose: 3 } => EnemyFrameId.ChootFallEnd,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the ChootFrame programs show."),
    };
}
