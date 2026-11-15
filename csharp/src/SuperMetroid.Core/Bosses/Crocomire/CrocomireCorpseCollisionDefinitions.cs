using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Physical components and hitboxes of the 33 Crocomire corpse frames. Each frame lists only its body's component (#1937):
/// the bones' components had no hitboxes, so the frames that show only bones have none.
/// </summary>
internal static class CrocomireCorpseCollisionDefinitions
{
    /// <summary>The corpse's two body rectangles.</summary>
    private static readonly EnemyCollisionHitbox[] CorpseBody = [new(-38, -16, 0, 31, EnemyReaction.Ignore, CrocomireReactions.DustShot), new(0, -29, 26, 28, EnemyReaction.Ignore, CrocomireReactions.DustShot)];

    /// <summary>Physical component offsets and hitbox lists for each of the 33 corpse frames.</summary>
    /// <remarks>The hitbox lists precede the frames that reference them: static fields initialise in declaration
    /// order.</remarks>
    internal static readonly Dictionary<EnemyFrameId, EnemyCollisionComponent[]> Frames = new()
    {
        [EnemyFrameId.CrocomireBodySkeletonFallingFirst] = [new(new(-4, -47), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallingSecond] = [new(new(-3, -49), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallingThird] = [new(new(-1, -50), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartFirst] = [new(new(0, -51), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartSecond] = [new(new(2, -48), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartThird] = [new(new(2, -44), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartFourth] = [new(new(3, -47), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartFifth] = [new(new(2, -48), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartSixth] = [new(new(9, -47), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartSeventh] = [new(new(16, -39), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartEighth] = [new(new(21, -35), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartNinth] = [new(new(31, -20), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartTenth] = [new(new(36, -14), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartEleventh] = [new(new(46, -4), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartTwelfth] = [new(new(55, 7), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartThirteenth] = [new(new(60, 12), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartFourteenth] = [new(new(62, 17), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartFifteenth] = [new(new(64, 22), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartSixteenth] = [new(new(64, 20), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartSeventeenth] = [new(new(64, 19), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartEighteenth] = [new(new(64, 20), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonStable] = [new(new(64, 21), CorpseBody)],
        [EnemyFrameId.CrocomireBodyDead] = [new(new(0, 0), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverFirst] = [],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverSecond] = [],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverThird] = [],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverFourth] = [],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverFifth] = [],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverSixth] = [],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverSeventh] = [],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverEighth] = [],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverNinth] = [],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverTenth] = [],
    };
}
