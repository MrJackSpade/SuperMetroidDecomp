using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Physical components and hitboxes of the 33 Crocomire corpse frames.</summary>
internal static class CrocomireCorpseCollisionDefinitions
{
    /// <summary>The corpse's two body rectangles.</summary>
    private static readonly EnemyCollisionHitbox[] CorpseBody = [new(-38, -16, 0, 31, EnemyReaction.Ignore, CrocomireReactions.DustHitboxShot), new(0, -29, 26, 28, EnemyReaction.Ignore, CrocomireReactions.DustHitboxShot)];
    /// <summary>The corpse's empty list, for components without collision.</summary>
    private static readonly EnemyCollisionHitbox[] CorpseEmpty = [];

    /// <summary>Physical component offsets and hitbox lists for each of the 33 corpse frames.</summary>
    /// <remarks>The hitbox lists precede the frames that reference them: static fields initialise in declaration
    /// order.</remarks>
    internal static readonly Dictionary<EnemyFrameId, EnemyCollisionComponent[]> Frames = new()
    {
        [EnemyFrameId.CrocomireBodySkeletonFallingFirst] = [new(new(-16, 7), CorpseEmpty), new(new(-4, -47), CorpseBody), new(new(-25, -8), CorpseEmpty), new(new(-1, 5), CorpseEmpty), new(new(-28, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallingSecond] = [new(new(-16, 4), CorpseEmpty), new(new(-3, -49), CorpseBody), new(new(-25, -8), CorpseEmpty), new(new(-1, 3), CorpseEmpty), new(new(-30, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallingThird] = [new(new(-11, -1), CorpseEmpty), new(new(-1, -50), CorpseBody), new(new(-25, -10), CorpseEmpty), new(new(-1, 2), CorpseEmpty), new(new(-31, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartFirst] = [new(new(-16, 3), CorpseEmpty), new(new(0, -51), CorpseBody), new(new(-25, -8), CorpseEmpty), new(new(-1, 1), CorpseEmpty), new(new(-32, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartSecond] = [new(new(-12, 7), CorpseEmpty), new(new(2, -48), CorpseBody), new(new(-25, -5), CorpseEmpty), new(new(-1, 4), CorpseEmpty), new(new(-33, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartThird] = [new(new(-12, 6), CorpseEmpty), new(new(2, -44), CorpseBody), new(new(-25, -5), CorpseEmpty), new(new(-1, 6), CorpseEmpty), new(new(-34, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartFourth] = [new(new(-11, 4), CorpseEmpty), new(new(3, -47), CorpseBody), new(new(-25, -5), CorpseEmpty), new(new(-1, 4), CorpseEmpty), new(new(-34, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartFifth] = [new(new(-12, 5), CorpseEmpty), new(new(2, -48), CorpseBody), new(new(-25, -5), CorpseEmpty), new(new(-1, 3), CorpseEmpty), new(new(-35, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartSixth] = [new(new(-16, 14), CorpseEmpty), new(new(9, -47), CorpseBody), new(new(-25, 0), CorpseEmpty), new(new(1, 5), CorpseEmpty), new(new(-38, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartSeventh] = [new(new(-21, 26), CorpseEmpty), new(new(16, -39), CorpseBody), new(new(-25, 7), CorpseEmpty), new(new(5, 11), CorpseEmpty), new(new(-38, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartEighth] = [new(new(-22, 35), CorpseEmpty), new(new(21, -35), CorpseBody), new(new(-26, 19), CorpseEmpty), new(new(7, 15), CorpseEmpty), new(new(-38, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartNinth] = [new(new(-22, 47), CorpseEmpty), new(new(31, -20), CorpseBody), new(new(-25, 30), CorpseEmpty), new(new(10, 20), CorpseEmpty), new(new(-38, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartTenth] = [new(new(-22, 54), CorpseEmpty), new(new(36, -14), CorpseBody), new(new(-25, 35), CorpseEmpty), new(new(10, 25), CorpseEmpty), new(new(-38, 20), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartEleventh] = [new(new(46, -4), CorpseBody), new(new(-25, 43), CorpseEmpty), new(new(12, 30), CorpseEmpty), new(new(1, 36), CorpseEmpty), new(new(-21, 32), CorpseEmpty), new(new(-40, 34), CorpseEmpty), new(new(-58, 30), CorpseEmpty), new(new(-74, 41), CorpseEmpty), new(new(-93, 41), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartTwelfth] = [new(new(55, 7), CorpseBody), new(new(41, 39), CorpseEmpty), new(new(-7, 20), CorpseEmpty), new(new(25, 44), CorpseEmpty), new(new(25, 28), CorpseEmpty), new(new(-8, 37), CorpseEmpty), new(new(8, 25), CorpseEmpty), new(new(9, 42), CorpseEmpty), new(new(-9, 49), CorpseEmpty), new(new(-24, 36), CorpseEmpty), new(new(-45, 40), CorpseEmpty), new(new(-61, 28), CorpseEmpty), new(new(-76, 43), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartThirteenth] = [new(new(60, 12), CorpseBody), new(new(41, 44), CorpseEmpty), new(new(-7, 25), CorpseEmpty), new(new(28, 48), CorpseEmpty), new(new(26, 32), CorpseEmpty), new(new(-3, 37), CorpseEmpty), new(new(9, 30), CorpseEmpty), new(new(13, 45), CorpseEmpty), new(new(-8, 51), CorpseEmpty), new(new(-23, 42), CorpseEmpty), new(new(-48, 42), CorpseEmpty), new(new(-62, 33), CorpseEmpty), new(new(-82, 47), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartFourteenth] = [new(new(62, 17), CorpseBody), new(new(41, 49), CorpseEmpty), new(new(-8, 32), CorpseEmpty), new(new(28, 48), CorpseEmpty), new(new(24, 37), CorpseEmpty), new(new(-2, 41), CorpseEmpty), new(new(9, 35), CorpseEmpty), new(new(15, 48), CorpseEmpty), new(new(-10, 54), CorpseEmpty), new(new(-24, 47), CorpseEmpty), new(new(-50, 45), CorpseEmpty), new(new(-64, 38), CorpseEmpty), new(new(-84, 51), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartFifteenth] = [new(new(64, 22), CorpseBody), new(new(41, 49), CorpseEmpty), new(new(-9, 38), CorpseEmpty), new(new(28, 44), CorpseEmpty), new(new(22, 40), CorpseEmpty), new(new(2, 44), CorpseEmpty), new(new(12, 39), CorpseEmpty), new(new(15, 48), CorpseEmpty), new(new(-10, 54), CorpseEmpty), new(new(-23, 52), CorpseEmpty), new(new(-46, 50), CorpseEmpty), new(new(-66, 44), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartSixteenth] = [new(new(64, 20), CorpseBody), new(new(41, 54), CorpseEmpty), new(new(-10, 43), CorpseEmpty), new(new(28, 49), CorpseEmpty), new(new(22, 45), CorpseEmpty), new(new(2, 44), CorpseEmpty), new(new(12, 44), CorpseEmpty), new(new(15, 53), CorpseEmpty), new(new(-22, 57), CorpseEmpty), new(new(-46, 55), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartSeventeenth] = [new(new(64, 19), CorpseBody), new(new(-13, 46), CorpseEmpty), new(new(29, 51), CorpseEmpty), new(new(20, 47), CorpseEmpty), new(new(2, 49), CorpseEmpty), new(new(12, 44), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFallsApartEighteenth] = [new(new(64, 20), CorpseBody), new(new(-13, 49), CorpseEmpty), new(new(14, 48), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonStable] = [new(new(64, 21), CorpseBody)],
        [EnemyFrameId.CrocomireBodyDead] = [new(new(0, 0), CorpseBody)],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverFirst] = [new(new(0, 0), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverSecond] = [new(new(0, 0), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverThird] = [new(new(0, 0), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverFourth] = [new(new(0, 0), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverFifth] = [new(new(0, 0), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverSixth] = [new(new(0, 0), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverSeventh] = [new(new(0, 0), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverEighth] = [new(new(0, 0), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverNinth] = [new(new(0, 0), CorpseEmpty)],
        [EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverTenth] = [new(new(32, 32), CorpseEmpty)],
    };
}
