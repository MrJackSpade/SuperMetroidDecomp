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
        [EnemyFrameId.CrocomireBodySkeletonToppling] = [new(new(-4, -47), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonLeaning] = [new(new(-3, -49), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonTipping] = [new(new(-1, -50), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage1] = [new(new(0, -51), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage2] = [new(new(2, -48), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage3] = [new(new(2, -44), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage4] = [new(new(3, -47), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage5] = [new(new(2, -48), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage6] = [new(new(9, -47), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage7] = [new(new(16, -39), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage8] = [new(new(21, -35), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage9] = [new(new(31, -20), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage10] = [new(new(36, -14), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage11] = [new(new(46, -4), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage12] = [new(new(55, 7), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage13] = [new(new(60, 12), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage14] = [new(new(62, 17), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage15] = [new(new(64, 22), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage16] = [new(new(64, 20), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage17] = [new(new(64, 19), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapseStage18] = [new(new(64, 20), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonStable] = [new(new(64, 21), CorpseBody)],
        [EnemyFrameId.CrocomireBodyDead] = [new(new(0, 0), CorpseBody)],
        [EnemyFrameId.CrocomireBubblesRisingStage1] = [],
        [EnemyFrameId.CrocomireBubblesRisingStage2] = [],
        [EnemyFrameId.CrocomireBubblesRisingStage3] = [],
        [EnemyFrameId.CrocomireBubblesRisingStage4] = [],
        [EnemyFrameId.CrocomireBubblesRisingStage5] = [],
        [EnemyFrameId.CrocomireBubblesRisingStage6] = [],
        [EnemyFrameId.CrocomireBubblesRisingStage7] = [],
        [EnemyFrameId.CrocomireBubblesRisingStage8] = [],
        [EnemyFrameId.CrocomireBubblesRisingStage9] = [],
        [EnemyFrameId.CrocomireBubblesAdrift] = [],
    };
}
