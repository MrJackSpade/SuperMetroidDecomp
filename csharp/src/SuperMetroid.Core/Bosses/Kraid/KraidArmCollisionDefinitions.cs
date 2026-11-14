using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Immutable physical components and ordered touch/shot rectangles of the 22 Kraid-arm frames (#1867). The rising or
/// sinking frames reuse the normal frames' ten poses but have independent editable sprite art. Each of those poses has
/// five components in collision order: an articulated component and its connector, the body at the arm's origin, and a
/// second articulated component and its connector.
/// </summary>
internal static class KraidArmCollisionDefinitions
{
    /// <summary>The initial pose.</summary>
    private static readonly KraidArmComponentDefinition[] FirstPose =
    [
        new(new(-36, -33), KraidArmHitboxList.ArticulatedFirst),
        new(new(-28, -24), KraidArmHitboxList.ConnectorInitial),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyInitial),
        new(new(-36, -40), KraidArmHitboxList.ArticulatedFirst),
        new(new(-28, -31), KraidArmHitboxList.ConnectorInitial),
    ];

    /// <summary>The next pose, before the arm rises.</summary>
    private static readonly KraidArmComponentDefinition[] SecondPose =
    [
        new(new(-38, -33), KraidArmHitboxList.ArticulatedSecond),
        new(new(-30, -26), KraidArmHitboxList.ConnectorInitial),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyInitial),
        new(new(-36, -40), KraidArmHitboxList.ArticulatedFirst),
        new(new(-28, -31), KraidArmHitboxList.ConnectorInitial),
    ];

    /// <summary>The arm raised outward.</summary>
    private static readonly KraidArmComponentDefinition[] ThirdPose =
    [
        new(new(-48, -13), KraidArmHitboxList.ArticulatedThird),
        new(new(-38, -13), KraidArmHitboxList.ConnectorSecond),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyMiddle),
        new(new(-45, -27), KraidArmHitboxList.ArticulatedSecond),
        new(new(-37, -19), KraidArmHitboxList.ConnectorInitial),
    ];

    /// <summary>The raised arm with its first articulated component and connector shifted.</summary>
    private static readonly KraidArmComponentDefinition[] FourthPose =
    [
        new(new(-46, -13), KraidArmHitboxList.ArticulatedFourth),
        new(new(-37, -13), KraidArmHitboxList.ConnectorSecond),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyMiddle),
        new(new(-45, -19), KraidArmHitboxList.ArticulatedThird),
        new(new(-36, -18), KraidArmHitboxList.ConnectorSecond),
    ];

    /// <summary>The arm lowered beside his body.</summary>
    private static readonly KraidArmComponentDefinition[] FifthPose =
    [
        new(new(-45, 8), KraidArmHitboxList.ArticulatedFifth),
        new(new(-38, 2), KraidArmHitboxList.ConnectorThird),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyFinal),
        new(new(-46, 3), KraidArmHitboxList.ArticulatedFourth),
        new(new(-39, -3), KraidArmHitboxList.ConnectorThird),
    ];

    /// <summary>The arm lowered beside his body, its second articulated component a pixel lower.</summary>
    private static readonly KraidArmComponentDefinition[] SixthPose =
    [
        new(new(-44, 8), KraidArmHitboxList.ArticulatedFifth),
        new(new(-37, 2), KraidArmHitboxList.ConnectorThird),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyFinal),
        new(new(-46, 4), KraidArmHitboxList.ArticulatedFifth),
        new(new(-39, -2), KraidArmHitboxList.ConnectorThird),
    ];

    /// <summary>The low pose.</summary>
    private static readonly KraidArmComponentDefinition[] SeventhPose =
    [
        new(new(-39, 10), KraidArmHitboxList.ArticulatedSixth),
        new(new(-38, 0), KraidArmHitboxList.ConnectorFinal),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyFinal),
        new(new(-43, 10), KraidArmHitboxList.ArticulatedSixth),
        new(new(-41, -2), KraidArmHitboxList.ConnectorFinal),
    ];

    /// <summary>The low pose with its second articulated component a pixel higher.</summary>
    private static readonly KraidArmComponentDefinition[] EighthPose =
    [
        new(new(-39, 10), KraidArmHitboxList.ArticulatedSixth),
        new(new(-38, 0), KraidArmHitboxList.ConnectorFinal),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyFinal),
        new(new(-43, 9), KraidArmHitboxList.ArticulatedSixth),
        new(new(-41, -2), KraidArmHitboxList.ConnectorFinal),
    ];

    /// <summary>The eighth pose's placement with its first articulated component in its last shape.</summary>
    private static readonly KraidArmComponentDefinition[] NinthPose =
    [
        new(new(-39, 10), KraidArmHitboxList.ArticulatedSeventh),
        new(new(-38, 0), KraidArmHitboxList.ConnectorFinal),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyFinal),
        new(new(-43, 9), KraidArmHitboxList.ArticulatedSixth),
        new(new(-41, -2), KraidArmHitboxList.ConnectorFinal),
    ];

    /// <summary>The low pose with the second articulated component and its connector drawn toward one another.</summary>
    private static readonly KraidArmComponentDefinition[] TenthPose =
    [
        new(new(-39, 10), KraidArmHitboxList.ArticulatedSeventh),
        new(new(-38, 0), KraidArmHitboxList.ConnectorFinal),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyFinal),
        new(new(-42, 9), KraidArmHitboxList.ArticulatedSeventh),
        new(new(-42, -2), KraidArmHitboxList.ConnectorFinal),
    ];

    /// <summary>
    /// Each Kraid-arm frame's components: the normal and rising or sinking frames share the ten poses, and each dying or
    /// preparing-to-lunge frame is one component at the arm's origin.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, KraidArmComponentDefinition[]> FrameComponents = new()
    {
        [EnemyFrameId.KraidArmNormalFirst] = FirstPose,
        [EnemyFrameId.KraidArmNormalSecond] = SecondPose,
        [EnemyFrameId.KraidArmNormalThird] = ThirdPose,
        [EnemyFrameId.KraidArmNormalFourth] = FourthPose,
        [EnemyFrameId.KraidArmNormalFifth] = FifthPose,
        [EnemyFrameId.KraidArmNormalSixth] = SixthPose,
        [EnemyFrameId.KraidArmNormalSeventh] = SeventhPose,
        [EnemyFrameId.KraidArmNormalEighth] = EighthPose,
        [EnemyFrameId.KraidArmNormalNinth] = NinthPose,
        [EnemyFrameId.KraidArmNormalTenth] = TenthPose,
        [EnemyFrameId.KraidArmRisingOrSinkingFirst] = FirstPose,
        [EnemyFrameId.KraidArmRisingOrSinkingSecond] = SecondPose,
        [EnemyFrameId.KraidArmRisingOrSinkingThird] = ThirdPose,
        [EnemyFrameId.KraidArmRisingOrSinkingFourth] = FourthPose,
        [EnemyFrameId.KraidArmRisingOrSinkingFifth] = FifthPose,
        [EnemyFrameId.KraidArmRisingOrSinkingSixth] = SixthPose,
        [EnemyFrameId.KraidArmRisingOrSinkingSeventh] = SeventhPose,
        [EnemyFrameId.KraidArmRisingOrSinkingEighth] = EighthPose,
        [EnemyFrameId.KraidArmRisingOrSinkingNinth] = NinthPose,
        [EnemyFrameId.KraidArmRisingOrSinkingTenth] = TenthPose,
        [EnemyFrameId.KraidArmDyingOrPreparingToLungeSecond] = [new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.FinalPoseFirst)],
        [EnemyFrameId.KraidArmDyingOrPreparingToLungeThird] = [new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.FinalPoseSecond)],
    };

    /// <summary>
    /// Each hitbox list's touch reaction and its rectangles in collision order. The articulated and connecting lists
    /// touch as the arm; the body lists and both dying/lunge lists touch as the background (independently reviewed for
    /// #1165 against every selected rectangle).
    /// </summary>
    internal static readonly Dictionary<KraidArmHitboxList, KraidHitboxListDefinition> HitboxListShapes = new()
    {
        [KraidArmHitboxList.ArticulatedFirst] = new(KraidReactions.ArmTouch, [new(-13, -11, -3, -5), new(-9, -5, 1, 2)]),
        [KraidArmHitboxList.ArticulatedSecond] = new(KraidReactions.ArmTouch, [new(-16, -5, 1, 2)]),
        [KraidArmHitboxList.ArticulatedThird] = new(KraidReactions.ArmTouch, [new(-9, -2, 1, 7), new(-12, 3, -6, 12)]),
        [KraidArmHitboxList.ArticulatedFourth] = new(KraidReactions.ArmTouch, [new(-6, -1, 1, 14)]),
        [KraidArmHitboxList.ArticulatedFifth] = new(KraidReactions.ArmTouch, [new(-3, -2, 6, 9), new(2, 7, 11, 11)]),
        [KraidArmHitboxList.ArticulatedSixth] = new(KraidReactions.ArmTouch, [new(-1, -4, 14, 4)]),
        [KraidArmHitboxList.ArticulatedSeventh] = new(KraidReactions.ArmTouch, [new(-3, -7, 6, 2), new(4, -12, 10, -1)]),
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
