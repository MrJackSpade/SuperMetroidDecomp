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
        [EnemyFrameId.CrocomireBodySkeletonCollapsing158By134] = [new(new(0, -51), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing141By132] = [new(new(2, -48), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing142By127] = [new(new(2, -44), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing143By130] = [new(new(3, -47), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing143By131] = [new(new(2, -48), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing160By130] = [new(new(9, -47), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing159By122] = [new(new(16, -39), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing158By118] = [new(new(21, -35), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing158By108] = [new(new(31, -20), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing158By109] = [new(new(36, -14), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing177By87] = [new(new(46, -4), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing174By82] = [new(new(55, 7), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing185By79] = [new(new(60, 12), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing189By77] = [new(new(62, 17), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing173By77] = [new(new(64, 22), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing153By84] = [new(new(64, 20), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing120By71] = [new(new(64, 19), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonCollapsing120By68] = [new(new(64, 20), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonStable] = [new(new(64, 21), CorpseBody)],
        [EnemyFrameId.CrocomireBodyDead] = [new(new(0, 0), CorpseBody)],
        [EnemyFrameId.CrocomireBubbles6By7Risen0] = [],
        [EnemyFrameId.CrocomireBubbles12By12Risen6] = [],
        [EnemyFrameId.CrocomireBubbles14By13Risen8] = [],
        [EnemyFrameId.CrocomireBubbles12By14Risen11] = [],
        [EnemyFrameId.CrocomireBubbles12By14Risen14] = [],
        [EnemyFrameId.CrocomireBubbles10By13Risen15] = [],
        [EnemyFrameId.CrocomireBubbles10By12Risen16] = [],
        [EnemyFrameId.CrocomireBubbles8By10Risen16] = [],
        [EnemyFrameId.CrocomireBubbles4By6Risen14] = [],
        [EnemyFrameId.CrocomireBubbles4By6Adrift] = [],
    };
}
