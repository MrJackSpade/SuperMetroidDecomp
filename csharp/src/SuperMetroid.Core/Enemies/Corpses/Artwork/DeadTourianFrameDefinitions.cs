using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses.Artwork;

/// <summary>
/// The presentation boundary of the dead Sidehopper's and Tourian corpses' programs (#641): the bank-$A9 spritemap each
/// <see cref="DeadTourianFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class DeadTourianFrameDefinitions
{

    /// <summary>The installed frame each <see cref="DeadTourianFrame"/> shows.</summary>
    internal static EnemyFrameTable<DeadTourianFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.DeadSidehopperCorpse },
        { new(Pose: 1), EnemyFrameId.DeadSidehopperInitiallyDead },
        { new(Pose: 10), EnemyFrameId.DeadSidehopperHopBody },
        { new(Pose: 11), EnemyFrameId.DeadSidehopperHopAlt },
        { new(Pose: 12), EnemyFrameId.DeadSidehopperIdle },
        { new(Pose: 2), EnemyFrameId.DeadZoomerCorpse0 },
        { new(Pose: 3), EnemyFrameId.DeadZoomerCorpse2 },
        { new(Pose: 4), EnemyFrameId.DeadZoomerCorpse4 },
        { new(Pose: 5), EnemyFrameId.DeadRipperCorpse0 },
        { new(Pose: 6), EnemyFrameId.DeadRipperCorpse2 },
        { new(Pose: 7), EnemyFrameId.DeadSkreeCorpse0 },
        { new(Pose: 8), EnemyFrameId.DeadSkreeCorpse2 },
        { new(Pose: 9), EnemyFrameId.DeadSkreeCorpse4 },
    };
}
