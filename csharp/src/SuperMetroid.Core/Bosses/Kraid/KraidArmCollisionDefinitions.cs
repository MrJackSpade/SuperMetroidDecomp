using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Immutable physical components and ordered touch/shot rectangles selected by
/// the 22 Kraid-arm frames. The second ten frames
/// reuse the first ten physical layouts but have independent editable sprite art.
/// </summary>
internal static class KraidArmCollisionDefinitions
{

    /// <summary>
    /// The four articulated component offsets of each of the ten shared five-component poses, indexed by pose; the body
    /// component stays at the enemy origin. Pose 0 is the initial pose and pose 1 the next before the arm rises; poses 2
    /// and 3 raise it outward, the second shifting hand and connector; poses 4 and 5 lower it beside Kraid's body;
    /// poses 6 through 9 hold the low pose, 7 lifting the outer connector a pixel above 6, 8 repeating 7's positions under
    /// distinct artwork, and 9 shifting the two outer components toward one another.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[][] PosePositions =
    [
        [new(-36, -33), new(-28, -24), new(-36, -40), new(-28, -31)],
        [new(-38, -33), new(-30, -26), new(-36, -40), new(-28, -31)],
        [new(-48, -13), new(-38, -13), new(-45, -27), new(-37, -19)],
        [new(-46, -13), new(-37, -13), new(-45, -19), new(-36, -18)],
        [new(-45, 8), new(-38, 2), new(-46, 3), new(-39, -3)],
        [new(-44, 8), new(-37, 2), new(-46, 4), new(-39, -2)],
        [new(-39, 10), new(-38, 0), new(-43, 10), new(-41, -2)],
        [new(-39, 10), new(-38, 0), new(-43, 9), new(-41, -2)],
        [new(-39, 10), new(-38, 0), new(-43, 9), new(-41, -2)],
        [new(-39, 10), new(-38, 0), new(-42, 9), new(-42, -2)],
    ];


    /// <summary>The seven articulated shapes components 0 and 3 advance through.</summary>
    internal static readonly KraidArmHitboxList[] ArticulatedShapes =
    [
        KraidArmHitboxList.Articulated0, KraidArmHitboxList.Articulated1, KraidArmHitboxList.Articulated2,
        KraidArmHitboxList.Articulated3, KraidArmHitboxList.Articulated4, KraidArmHitboxList.Articulated5,
        KraidArmHitboxList.Articulated6,
    ];

    /// <summary>
    /// The physical pose of each Kraid-arm frame: the two ten-frame artwork groups share the ten five-component poses,
    /// and the two dying frames take the single-component poses 10 and 11.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, int> FramePoses = new()
    {
        [EnemyFrameId.KraidArmNormalFirst] = 0,
        [EnemyFrameId.KraidArmNormalSecond] = 1,
        [EnemyFrameId.KraidArmNormalThird] = 2,
        [EnemyFrameId.KraidArmNormalFourth] = 3,
        [EnemyFrameId.KraidArmNormalFifth] = 4,
        [EnemyFrameId.KraidArmNormalSixth] = 5,
        [EnemyFrameId.KraidArmNormalSeventh] = 6,
        [EnemyFrameId.KraidArmNormalEighth] = 7,
        [EnemyFrameId.KraidArmNormalNinth] = 8,
        [EnemyFrameId.KraidArmNormalTenth] = 9,
        [EnemyFrameId.KraidArmRisingOrSinkingFirst] = 0,
        [EnemyFrameId.KraidArmRisingOrSinkingSecond] = 1,
        [EnemyFrameId.KraidArmRisingOrSinkingThird] = 2,
        [EnemyFrameId.KraidArmRisingOrSinkingFourth] = 3,
        [EnemyFrameId.KraidArmRisingOrSinkingFifth] = 4,
        [EnemyFrameId.KraidArmRisingOrSinkingSixth] = 5,
        [EnemyFrameId.KraidArmRisingOrSinkingSeventh] = 6,
        [EnemyFrameId.KraidArmRisingOrSinkingEighth] = 7,
        [EnemyFrameId.KraidArmRisingOrSinkingNinth] = 8,
        [EnemyFrameId.KraidArmRisingOrSinkingTenth] = 9,
        [EnemyFrameId.KraidArmDyingOrPreparingToLungeSecond] = 10,
        [EnemyFrameId.KraidArmDyingOrPreparingToLungeThird] = 11,
    };

    /// <summary>
    /// Each hitbox list's touch reaction and its rectangles in collision order. The articulated and connecting lists
    /// touch as the arm; the body lists and both dying/lunge lists touch as the background (independently reviewed for
    /// #1165 against every selected rectangle).
    /// </summary>
    internal static readonly Dictionary<KraidArmHitboxList, KraidHitboxListDefinition> HitboxListShapes = new()
    {
        [KraidArmHitboxList.Articulated0] = new(KraidReactions.ArmTouch, [new(-13, -11, -3, -5), new(-9, -5, 1, 2)]),
        [KraidArmHitboxList.Articulated1] = new(KraidReactions.ArmTouch, [new(-16, -5, 1, 2)]),
        [KraidArmHitboxList.Articulated2] = new(KraidReactions.ArmTouch, [new(-9, -2, 1, 7), new(-12, 3, -6, 12)]),
        [KraidArmHitboxList.Articulated3] = new(KraidReactions.ArmTouch, [new(-6, -1, 1, 14)]),
        [KraidArmHitboxList.Articulated4] = new(KraidReactions.ArmTouch, [new(-3, -2, 6, 9), new(2, 7, 11, 11)]),
        [KraidArmHitboxList.Articulated5] = new(KraidReactions.ArmTouch, [new(-1, -4, 14, 4)]),
        [KraidArmHitboxList.Articulated6] = new(KraidReactions.ArmTouch, [new(-3, -7, 6, 2), new(4, -12, 10, -1)]),
        [KraidArmHitboxList.ConnectorSecond] = new(KraidReactions.ArmTouch, [new(-15, -5, 2, 4)]),
        [KraidArmHitboxList.ConnectorThird] = new(KraidReactions.ArmTouch, [new(-11, 2, -4, 10), new(-6, -3, 3, 5)]),
        [KraidArmHitboxList.ConnectorFinal] = new(KraidReactions.ArmTouch, [new(-4, -2, 3, 13)]),
        [KraidArmHitboxList.ConnectorInitial] = new(KraidReactions.ArmTouch, [new(-12, -12, -3, -3), new(-6, -6, 3, 2)]),
        [KraidArmHitboxList.BodyFinal] = new(KraidReactions.BackgroundTouch, [new(-45, -9, 4, 8)]),
        [KraidArmHitboxList.BodyMiddle] = new(KraidReactions.BackgroundTouch, [new(-28, -17, -12, 0), new(-42, -23, -28, -6)]),
        [KraidArmHitboxList.BodyInitial] = new(KraidReactions.BackgroundTouch, [new(-22, -25, -8, -5), new(-35, -35, -19, -17)]),
        [KraidArmHitboxList.FinalPoseFirst] = new(KraidReactions.BackgroundTouch, [new(-64, -48, -32, -16)]),
        [KraidArmHitboxList.FinalPoseSecond] = new(KraidReactions.BackgroundTouch, [new(-64, -4, 0, 4)]),
    };
}
