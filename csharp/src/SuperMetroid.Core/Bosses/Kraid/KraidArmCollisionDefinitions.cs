using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Immutable physical components and ordered touch/shot rectangles of the 22 Kraid-arm frames (#1867). The rising or
/// sinking frames reuse the normal frames' ten poses but have independent editable sprite art. Each of those poses has
/// five components in collision order: a limb and its connector, his body at the arm's origin, and a second limb and its
/// connector; each pose is named for the frame it is (#2211).
/// </summary>
internal static class KraidArmCollisionDefinitions
{
    /// <summary>The arm raised, its claw closed.</summary>
    private static readonly KraidArmComponentDefinition[] RaisedClawClosed =
    [
        new(new(-36, -33), KraidArmHitboxList.LimbRaised),
        new(new(-28, -24), KraidArmHitboxList.ConnectorRaised),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyBehindRaisedArm),
        new(new(-36, -40), KraidArmHitboxList.LimbRaised),
        new(new(-28, -31), KraidArmHitboxList.ConnectorRaised),
    ];

    /// <summary>The arm raised, its claw open.</summary>
    private static readonly KraidArmComponentDefinition[] RaisedClawOpen =
    [
        new(new(-38, -33), KraidArmHitboxList.LimbRaisedOpen),
        new(new(-30, -26), KraidArmHitboxList.ConnectorRaised),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyBehindRaisedArm),
        new(new(-36, -40), KraidArmHitboxList.LimbRaised),
        new(new(-28, -31), KraidArmHitboxList.ConnectorRaised),
    ];

    /// <summary>The arm lowering, its claw hooked.</summary>
    private static readonly KraidArmComponentDefinition[] LoweringClawHooked =
    [
        new(new(-48, -13), KraidArmHitboxList.LimbLowering),
        new(new(-38, -13), KraidArmHitboxList.ConnectorLowered),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyBehindLoweringArm),
        new(new(-45, -27), KraidArmHitboxList.LimbRaisedOpen),
        new(new(-37, -19), KraidArmHitboxList.ConnectorRaised),
    ];

    /// <summary>The arm level, its claw hooked.</summary>
    private static readonly KraidArmComponentDefinition[] LevelClawHooked =
    [
        new(new(-46, -13), KraidArmHitboxList.LimbLevel),
        new(new(-37, -13), KraidArmHitboxList.ConnectorLowered),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyBehindLoweringArm),
        new(new(-45, -19), KraidArmHitboxList.LimbLowering),
        new(new(-36, -18), KraidArmHitboxList.ConnectorLowered),
    ];

    /// <summary>The arm level, its claw hanging down.</summary>
    private static readonly KraidArmComponentDefinition[] LevelClawHangingDown =
    [
        new(new(-45, 8), KraidArmHitboxList.LimbHanging),
        new(new(-38, 2), KraidArmHitboxList.ConnectorHanging),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyBehindLoweredArm),
        new(new(-46, 3), KraidArmHitboxList.LimbLevel),
        new(new(-39, -3), KraidArmHitboxList.ConnectorHanging),
    ];

    /// <summary>The arm level, its claw curling under.</summary>
    private static readonly KraidArmComponentDefinition[] LevelClawCurlingUnder =
    [
        new(new(-44, 8), KraidArmHitboxList.LimbHanging),
        new(new(-37, 2), KraidArmHitboxList.ConnectorHanging),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyBehindLoweredArm),
        new(new(-46, 4), KraidArmHitboxList.LimbHanging),
        new(new(-39, -2), KraidArmHitboxList.ConnectorHanging),
    ];

    /// <summary>The arm level, its claw tucked.</summary>
    private static readonly KraidArmComponentDefinition[] LevelClawTucked =
    [
        new(new(-39, 10), KraidArmHitboxList.LimbTucked),
        new(new(-38, 0), KraidArmHitboxList.ConnectorTucked),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyBehindLoweredArm),
        new(new(-43, 10), KraidArmHitboxList.LimbTucked),
        new(new(-41, -2), KraidArmHitboxList.ConnectorTucked),
    ];

    /// <summary>The arm level, its claw tucked tight.</summary>
    private static readonly KraidArmComponentDefinition[] LevelClawTuckedTight =
    [
        new(new(-39, 10), KraidArmHitboxList.LimbTucked),
        new(new(-38, 0), KraidArmHitboxList.ConnectorTucked),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyBehindLoweredArm),
        new(new(-43, 9), KraidArmHitboxList.LimbTucked),
        new(new(-41, -2), KraidArmHitboxList.ConnectorTucked),
    ];

    /// <summary>The arm level, its claw folded.</summary>
    private static readonly KraidArmComponentDefinition[] LevelClawFolded =
    [
        new(new(-39, 10), KraidArmHitboxList.LimbFolded),
        new(new(-38, 0), KraidArmHitboxList.ConnectorTucked),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyBehindLoweredArm),
        new(new(-43, 9), KraidArmHitboxList.LimbTucked),
        new(new(-41, -2), KraidArmHitboxList.ConnectorTucked),
    ];

    /// <summary>The arm level, its claw folded flat.</summary>
    private static readonly KraidArmComponentDefinition[] LevelClawFoldedFlat =
    [
        new(new(-39, 10), KraidArmHitboxList.LimbFolded),
        new(new(-38, 0), KraidArmHitboxList.ConnectorTucked),
        new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.BodyBehindLoweredArm),
        new(new(-42, 9), KraidArmHitboxList.LimbFolded),
        new(new(-42, -2), KraidArmHitboxList.ConnectorTucked),
    ];

    /// <summary>
    /// Each Kraid-arm frame's components: the normal and rising or sinking frames share the ten poses, and each dying or
    /// preparing-to-lunge frame is one component at the arm's origin.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, KraidArmComponentDefinition[]> FrameComponents = new()
    {
        [EnemyFrameId.KraidArmRaisedClawClosed] = RaisedClawClosed,
        [EnemyFrameId.KraidArmRaisedClawOpen] = RaisedClawOpen,
        [EnemyFrameId.KraidArmLoweringClawHooked] = LoweringClawHooked,
        [EnemyFrameId.KraidArmLevelClawHooked] = LevelClawHooked,
        [EnemyFrameId.KraidArmLevelClawHangingDown] = LevelClawHangingDown,
        [EnemyFrameId.KraidArmLevelClawCurlingUnder] = LevelClawCurlingUnder,
        [EnemyFrameId.KraidArmLevelClawTucked] = LevelClawTucked,
        [EnemyFrameId.KraidArmLevelClawTuckedTight] = LevelClawTuckedTight,
        [EnemyFrameId.KraidArmLevelClawFolded] = LevelClawFolded,
        [EnemyFrameId.KraidArmLevelClawFoldedFlat] = LevelClawFoldedFlat,
        [EnemyFrameId.KraidArmRaisedClawClosedTwin] = RaisedClawClosed,
        [EnemyFrameId.KraidArmRaisedClawOpenTwin] = RaisedClawOpen,
        [EnemyFrameId.KraidArmLoweringClawHookedTwin] = LoweringClawHooked,
        [EnemyFrameId.KraidArmLevelClawHookedTwin] = LevelClawHooked,
        [EnemyFrameId.KraidArmLevelClawHangingDownTwin] = LevelClawHangingDown,
        [EnemyFrameId.KraidArmLevelClawCurlingUnderTwin] = LevelClawCurlingUnder,
        [EnemyFrameId.KraidArmLevelClawTuckedTwin] = LevelClawTucked,
        [EnemyFrameId.KraidArmLevelClawTuckedTightTwin] = LevelClawTuckedTight,
        [EnemyFrameId.KraidArmLevelClawFoldedTwin] = LevelClawFolded,
        [EnemyFrameId.KraidArmLevelClawFoldedFlatTwin] = LevelClawFoldedFlat,
        [EnemyFrameId.KraidArmDroopingDiagonal] = [new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.DroopingDiagonal)],
        [EnemyFrameId.KraidArmOutstretched] = [new(Offset16<RoomPixels>.Zero, KraidArmHitboxList.Outstretched)],
    };

    /// <summary>
    /// Each hitbox list's touch reaction and its rectangles in collision order. The articulated and connecting lists
    /// touch as the arm; the body lists and both dying/lunge lists touch as his body.
    /// </summary>
    internal static readonly Dictionary<KraidArmHitboxList, KraidHitboxListDefinition> HitboxListShapes = new()
    {
        [KraidArmHitboxList.LimbRaised] = new(KraidReactions.ArmTouch, [new(-13, -11, -3, -5), new(-9, -5, 1, 2)]),
        [KraidArmHitboxList.LimbRaisedOpen] = new(KraidReactions.ArmTouch, [new(-16, -5, 1, 2)]),
        [KraidArmHitboxList.LimbLowering] = new(KraidReactions.ArmTouch, [new(-9, -2, 1, 7), new(-12, 3, -6, 12)]),
        [KraidArmHitboxList.LimbLevel] = new(KraidReactions.ArmTouch, [new(-6, -1, 1, 14)]),
        [KraidArmHitboxList.LimbHanging] = new(KraidReactions.ArmTouch, [new(-3, -2, 6, 9), new(2, 7, 11, 11)]),
        [KraidArmHitboxList.LimbTucked] = new(KraidReactions.ArmTouch, [new(-1, -4, 14, 4)]),
        [KraidArmHitboxList.LimbFolded] = new(KraidReactions.ArmTouch, [new(-3, -7, 6, 2), new(4, -12, 10, -1)]),
        [KraidArmHitboxList.ConnectorLowered] = new(KraidReactions.ArmTouch, [new(-15, -5, 2, 4)]),
        [KraidArmHitboxList.ConnectorHanging] = new(KraidReactions.ArmTouch, [new(-11, 2, -4, 10), new(-6, -3, 3, 5)]),
        [KraidArmHitboxList.ConnectorTucked] = new(KraidReactions.ArmTouch, [new(-4, -2, 3, 13)]),
        [KraidArmHitboxList.ConnectorRaised] = new(KraidReactions.ArmTouch, [new(-12, -12, -3, -3), new(-6, -6, 3, 2)]),
        [KraidArmHitboxList.BodyBehindLoweredArm] = new(KraidReactions.BodyTouch, [new(-45, -9, 4, 8)]),
        [KraidArmHitboxList.BodyBehindLoweringArm] = new(KraidReactions.BodyTouch, [new(-28, -17, -12, 0), new(-42, -23, -28, -6)]),
        [KraidArmHitboxList.BodyBehindRaisedArm] = new(KraidReactions.BodyTouch, [new(-22, -25, -8, -5), new(-35, -35, -19, -17)]),
        [KraidArmHitboxList.DroopingDiagonal] = new(KraidReactions.BodyTouch, [new(-64, -48, -32, -16)]),
        [KraidArmHitboxList.Outstretched] = new(KraidReactions.BodyTouch, [new(-64, -4, 0, 4)]),
    };
}
