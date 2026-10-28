using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Immutable physical components and ordered touch/shot rectangles selected by
/// the 22 Kraid-arm extended frames at $A7:8F59-92B4. The second ten frames
/// reuse the first ten physical layouts but have independent editable OAM.
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

    /// <summary>Ordered physical rectangles; callbacks are calculated separately from the index.</summary>
    internal static readonly EnemyHitboxRectangle[] Geometry =
    [
        new(-13, -11, -3, -5),
        new(-9, -5, 1, 2),
        new(-16, -5, 1, 2),
        new(-9, -2, 1, 7),
        new(-12, 3, -6, 12),
        new(-6, -1, 1, 14),
        new(-3, -2, 6, 9),
        new(2, 7, 11, 11),
        new(-1, -4, 14, 4),
        new(-3, -7, 6, 2),
        new(4, -12, 10, -1),
        new(-15, -5, 2, 4),
        new(-11, 2, -4, 10),
        new(-6, -3, 3, 5),
        new(-4, -2, 3, 13),
        new(-12, -12, -3, -3),
        new(-6, -6, 3, 2),
        new(-45, -9, 4, 8),
        new(-28, -17, -12, 0),
        new(-42, -23, -28, -6),
        new(-22, -25, -8, -5),
        new(-35, -35, -19, -17),
        new(-64, -48, -32, -16),
        new(-64, -4, 0, 4),
    ];

    /// <summary>Twenty-four rectangles selected by the compiled arm frame families.</summary>
    internal const int RectangleCount = 24;
    /// <summary>First compiled rectangle from $A7:9411, Hitbox_KraidArm_10, using normal background touch.</summary>
    internal const int FirstBackgroundRectangle = 17;

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
        [EnemyFrameId.KraidArm0] = 0,
        [EnemyFrameId.KraidArm1] = 1,
        [EnemyFrameId.KraidArm2] = 2,
        [EnemyFrameId.KraidArm3] = 3,
        [EnemyFrameId.KraidArm4] = 4,
        [EnemyFrameId.KraidArm5] = 5,
        [EnemyFrameId.KraidArm6] = 6,
        [EnemyFrameId.KraidArm7] = 7,
        [EnemyFrameId.KraidArm8] = 8,
        [EnemyFrameId.KraidArm9] = 9,
        [EnemyFrameId.KraidArm10] = 0,
        [EnemyFrameId.KraidArm11] = 1,
        [EnemyFrameId.KraidArm12] = 2,
        [EnemyFrameId.KraidArm13] = 3,
        [EnemyFrameId.KraidArm14] = 4,
        [EnemyFrameId.KraidArm15] = 5,
        [EnemyFrameId.KraidArm16] = 6,
        [EnemyFrameId.KraidArm17] = 7,
        [EnemyFrameId.KraidArm18] = 8,
        [EnemyFrameId.KraidArm19] = 9,
        [EnemyFrameId.KraidArm20] = 10,
        [EnemyFrameId.KraidArm21] = 11,
    };

    /// <summary>Each compiled hitbox list and its slice of the rectangle geometry: first rectangle and count.</summary>
    internal static readonly KraidHitboxListDefinition[] HitboxListSlices =
    [
        new(KraidArmHitboxList.Articulated0, 0, 2),
        new(KraidArmHitboxList.Articulated1, 2, 1),
        new(KraidArmHitboxList.Articulated2, 3, 2),
        new(KraidArmHitboxList.Articulated3, 5, 1),
        new(KraidArmHitboxList.Articulated4, 6, 2),
        new(KraidArmHitboxList.Articulated5, 8, 1),
        new(KraidArmHitboxList.Articulated6, 9, 2),
        new(KraidArmHitboxList.ConnectorSecond, 11, 1),
        new(KraidArmHitboxList.ConnectorThird, 12, 2),
        new(KraidArmHitboxList.ConnectorFinal, 14, 1),
        new(KraidArmHitboxList.ConnectorInitial, 15, 2),
        new(KraidArmHitboxList.BodyFinal, 17, 1),
        new(KraidArmHitboxList.BodyMiddle, 18, 2),
        new(KraidArmHitboxList.BodyInitial, 20, 2),
        new(KraidArmHitboxList.FinalPoseFirst, 22, 1),
        new(KraidArmHitboxList.FinalPoseSecond, 23, 1),
    ];
}
