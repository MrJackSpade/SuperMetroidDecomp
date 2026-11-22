using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Physical components and hitboxes of the 33 Crocomire corpse frames. Each frame lists only its body's component (#1937):
/// the bones' components had no hitboxes, so the frames that show only bones have none.
/// </summary>
internal static partial class CrocomireCorpseCollisionDefinitions
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
        [EnemyFrameId.CrocomireBodySkeletonArmHeldLevel] = [new(new(0, -51), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonArmDropping] = [new(new(2, -48), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonArmLowered] = [new(new(2, -44), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonArmPlanted] = [new(new(3, -47), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonBodySettling] = [new(new(2, -48), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonArmSplaying] = [new(new(9, -47), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonArmFlat] = [new(new(16, -39), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonRibsSagging] = [new(new(21, -35), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonRibsSlumped] = [new(new(31, -20), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonArmFallenOff] = [new(new(36, -14), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonTailScattering] = [new(new(46, -4), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonSpineFallingApart] = [new(new(55, 7), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonRibsSplaying] = [new(new(60, 12), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonRibsFlattened] = [new(new(62, 17), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonBonesGathering] = [new(new(64, 22), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonBonesHuddled] = [new(new(64, 20), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonRibsBesideSkull] = [new(new(64, 19), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonLastBones] = [new(new(64, 20), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonStable] = [new(new(64, 21), CorpseBody)],
        [EnemyFrameId.CrocomireBodyDead] = [new(new(0, 0), CorpseBody)],
        [EnemyFrameId.CrocomireBubblesForming] = [],
        [EnemyFrameId.CrocomireBubblesWelling] = [],
        [EnemyFrameId.CrocomireBubblesSpreading] = [],
        [EnemyFrameId.CrocomireBubblesBursting] = [],
        [EnemyFrameId.CrocomireBubblesSpraying] = [],
        [EnemyFrameId.CrocomireBubblesScattered] = [],
        [EnemyFrameId.CrocomireBubblesThinning] = [],
        [EnemyFrameId.CrocomireBubblesFading] = [],
        [EnemyFrameId.CrocomireBubblesVanishing] = [],
        [EnemyFrameId.CrocomireBubblesAdrift] = [],
    };
}
