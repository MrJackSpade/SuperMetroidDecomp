using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Bull.Artwork;

/// <summary>
/// The presentation boundary of Bull's programs (#641): the bank-$A8 spritemap each
/// <see cref="BullFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class BullFrameDefinitions
{

    /// <summary>The installed frame each <see cref="BullFrame"/> shows.</summary>
    internal static EnemyFrameTable<BullFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.BullIdle0 },
        { new(Pose: 1), EnemyFrameId.BullIdle1 },
        { new(Pose: 2), EnemyFrameId.BullIdle2 },
    };
}
