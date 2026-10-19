using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Choot.Artwork;

/// <summary>
/// The presentation boundary of Choot's programs (#641): the bank-$A2 spritemap each
/// <see cref="ChootFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ChootFrameDefinitions
{

    /// <summary>The installed frame each <see cref="ChootFrame"/> shows.</summary>
    internal static EnemyFrameTable<ChootFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.ChootIdle },
        { new(Pose: 1), EnemyFrameId.ChootJump },
        { new(Pose: 2), EnemyFrameId.ChootJumpApex },
        { new(Pose: 3), EnemyFrameId.ChootFallEnd },
    };
}
