using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Ordered physical components and touch/shot rectangles for all 106 Torizo frames selected
/// by the compiled animations. Sprite composition and pixels are not stored here.
/// The blank frame has an empty hitbox list, rather than invented collision.
/// </summary>
/// <remarks>
/// Reviewed for #1165; the single owner of the Torizo collision space. Frame records
/// are contiguous within four runs (the blank frame, turning plus 42 left-facing frames,
/// seven awakening frames, right-facing frames), so every frame derives from its
/// run start and the preceding record sizes (a count plus one entry per component).
/// The first 49 right-facing frames are the exact X mirrors, in order, of the 49 left-facing
/// frames: X negates and each hitbox list is replaced by its separately fitted right-facing
/// counterpart (<see cref="TorizoHitboxList.RightFacing"/>); every list without collision is
/// <see cref="Empty"/>. Retained as authored: the left-facing placements (torso/leg choreography of each
/// drawing), the six jump-back frames (their two facings are drawn differently), and the list
/// rectangles, which are fitted per facing to the drawings and are not mirrors of each other.
/// </remarks>
internal static class TorizoCollisionDefinitions
{
    /// <summary>The list of every component without collision; it is its own right-facing counterpart.</summary>
    internal static readonly TorizoHitboxList Empty = new([]);

    /// <summary>
    /// The seven right-facing awakening bodies, indexed by awakening step, each fitted separately from its left-facing
    /// counterpart: narrower on the left at first, growing upward to Y -32 and then -35 through 39, shifting the upper reach
    /// toward positive X, and finally expanding to X 22.
    /// </summary>
    private static readonly TorizoHitboxList[] AwakeningStepsRight =
    [
        new([new(-15, -27, 13, 27, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)]),
        new([new(-13, -27, 13, 27, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)]),
        new([new(-14, -32, 13, 33, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)]),
        new([new(-14, -35, 11, 39, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)]),
        new([new(-6, -42, 13, 47, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)]),
        new([new(-7, -41, 11, 47, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)]),
        new([new(-8, -41, 22, 47, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)]),
    ];
    /// <summary>Right-facing active body.</summary>
    private static readonly TorizoHitboxList ActiveBodyRight = new([new(-10, -38, 13, 23, EnemyReaction.BombTorizoTouch, EnemyReaction.BombTorizoShot)]);
    /// <summary>Right-facing standing body, extending farther down and right than the right-facing jump-back body.</summary>
    private static readonly TorizoHitboxList StandingBodyRight = new([new(-9, -40, 16, 25, EnemyReaction.BombTorizoTouch, EnemyReaction.BombTorizoShot)]);
    /// <summary>Compact right-facing jump-back body, fitted separately from its left counterpart; the right-facing jump-back frames use it directly.</summary>
    private static readonly TorizoHitboxList JumpBackBodyRight = new([new(-8, -37, 15, 14, EnemyReaction.BombTorizoTouch, EnemyReaction.BombTorizoShot)]);

    /// <summary>
    /// The seven awakening bodies with the stand-up/sit-down shot reaction, indexed by awakening step: a 32-by-54 body,
    /// narrowed on the left, taller, centered and extending farther vertically, its upper reach shifted left, shortened
    /// below the center, and finally narrowing below under the tall upper reach.
    /// </summary>
    private static readonly TorizoHitboxList[] AwakeningSteps =
    [
        new([new(-16, -27, 16, 27, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)], AwakeningStepsRight[0]),
        new([new(-14, -27, 13, 27, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)], AwakeningStepsRight[1]),
        new([new(-13, -34, 9, 33, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)], AwakeningStepsRight[2]),
        new([new(-11, -38, 11, 39, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)], AwakeningStepsRight[3]),
        new([new(-15, -44, 8, 47, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)], AwakeningStepsRight[4]),
        new([new(-18, -43, 3, 24, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)], AwakeningStepsRight[5]),
        new([new(-17, -42, 5, 15, EnemyReaction.BombTorizoTouch, EnemyReaction.TorizoStandUpSitDownShot)], AwakeningStepsRight[6]),
    ];
    /// <summary>Left-facing active body, with the ordinary body shot reaction.</summary>
    private static readonly TorizoHitboxList ActiveBody = new([new(-15, -39, 7, 21, EnemyReaction.BombTorizoTouch, EnemyReaction.BombTorizoShot)], ActiveBodyRight);
    /// <summary>Compact left-facing jump-back body with its lower edge at Y 9.</summary>
    private static readonly TorizoHitboxList JumpBackBody = new([new(-18, -38, 7, 9, EnemyReaction.BombTorizoTouch, EnemyReaction.BombTorizoShot)], JumpBackBodyRight);
    /// <summary>Left-facing standing body, extending to Y 18.</summary>
    private static readonly TorizoHitboxList StandingBody = new([new(-18, -37, 7, 18, EnemyReaction.BombTorizoTouch, EnemyReaction.BombTorizoShot)], StandingBodyRight);

    /// <summary>Single origin component used by the blank and shared turning frames.</summary>
    private static readonly GoldenTorizoCollisionComponent[] SingleEmpty = [new(new(0, 0), Empty)];

    /// <summary>The 42 left-facing frames, in authored order.</summary>
    internal static readonly GoldenTorizoCollisionComponent[][] LeftFrames =
    [
        [new(new(-15, -30), Empty), new(new(-5, -24), Empty), new(new(0, 0), StandingBody), new(new(-5, -24), Empty)],
        [new(new(-15, -30), Empty), new(new(-5, -25), Empty), new(new(0, 0), ActiveBody), new(new(-5, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-5, -26), Empty), new(new(0, 0), ActiveBody), new(new(-5, -26), Empty)],
        [new(new(-15, -32), Empty), new(new(-5, -25), Empty), new(new(0, 0), ActiveBody), new(new(-5, -25), Empty)],
        [new(new(-15, -32), Empty), new(new(-5, -24), Empty), new(new(0, 0), ActiveBody), new(new(-5, -24), Empty)],
        [new(new(-15, -30), Empty), new(new(-5, -24), Empty), new(new(0, 0), StandingBody), new(new(-5, -24), Empty)],
        [new(new(-15, -31), Empty), new(new(-5, -25), Empty), new(new(0, 0), ActiveBody), new(new(-5, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-5, -26), Empty), new(new(0, 0), ActiveBody), new(new(-5, -26), Empty)],
        [new(new(-15, -32), Empty), new(new(-5, -25), Empty), new(new(0, 0), ActiveBody), new(new(-5, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-5, -24), Empty), new(new(0, 0), ActiveBody), new(new(-5, -24), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-9, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-9, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-9, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-9, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-9, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-9, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-9, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-9, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-9, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-9, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
    ];

    /// <summary>The seven awakening frames, in authored order.</summary>
    internal static readonly GoldenTorizoCollisionComponent[][] AwakeningFrames =
    [
        [new(new(0, 0), AwakeningSteps[0])],
        [new(new(0, 0), AwakeningSteps[1])],
        [new(new(0, 0), AwakeningSteps[2])],
        [new(new(0, 0), AwakeningSteps[3])],
        [new(new(-5, -24), Empty), new(new(0, 0), AwakeningSteps[4])],
        [new(new(-5, -24), Empty), new(new(0, 0), AwakeningSteps[5])],
        [new(new(-5, -24), Empty), new(new(0, 0), AwakeningSteps[6])],
    ];

    /// <summary>The six jump-back frames ending the right-facing run (three per facing).</summary>
    internal static readonly GoldenTorizoCollisionComponent[][] JumpBackFrames =
    [
        [new(new(-16, -29), Empty), new(new(-4, -22), Empty), new(new(0, 0), JumpBackBody)],
        [new(new(-16, -30), Empty), new(new(-4, -24), Empty), new(new(0, 0), JumpBackBody)],
        [new(new(-16, -30), Empty), new(new(-3, -24), Empty), new(new(0, 0), JumpBackBody)],
        [new(new(15, -28), Empty), new(new(4, -22), Empty), new(new(0, 0), JumpBackBodyRight)],
        [new(new(15, -29), Empty), new(new(4, -24), Empty), new(new(0, 0), JumpBackBodyRight)],
        [new(new(15, -29), Empty), new(new(3, -24), Empty), new(new(0, 0), JumpBackBodyRight)],
    ];

    /// <summary>Number of left and awakening frames whose right-facing counterparts derive by reflection.</summary>
    internal static int MirroredCount => LeftFrames.Length + AwakeningFrames.Length;

    /// <summary>The left-facing frames in mirror order: the left frames, then the awakening frames.</summary>
    internal static readonly GoldenTorizoCollisionComponent[][] LeftFacingFrames = [.. LeftFrames, .. AwakeningFrames];

    /// <summary>
    /// The right-facing counterparts of the left and awakening frames, in mirror order: each component's X reflected and
    /// each hitbox list replaced by its right-facing counterpart.
    /// </summary>
    private static readonly GoldenTorizoCollisionComponent[][] MirroredFrames =
        [.. Enumerable.Range(0, MirroredCount).Select(index => LeftFacingFrames[index]
            .Select(part => part with { Offset = part.Offset.MirroredX(), Hitboxes = part.Hitboxes.RightFacing }).ToArray())];

    /// <summary>
    /// Each Torizo frame's components: the blank and turning frames carry one empty component; left-facing, awakening
    /// and jump-back frames their own components; right-facing frames the precomputed mirror images.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, GoldenTorizoCollisionComponent[]> FrameComponents = new()
    {
        [EnemyFrameId.BombTorizoDormant] = SingleEmpty,
        [EnemyFrameId.TorizoFacingLeftTurningLeft] = SingleEmpty,
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFirst] = LeftFrames[0],
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingSecond] = LeftFrames[1],
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingThird] = LeftFrames[2],
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFourth] = LeftFrames[3],
        [EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFifth] = LeftFrames[4],
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFirst] = LeftFrames[5],
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingSecond] = LeftFrames[6],
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingThird] = LeftFrames[7],
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFourth] = LeftFrames[8],
        [EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFifth] = LeftFrames[9],
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFirst] = LeftFrames[10],
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardSecond] = LeftFrames[11],
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardThird] = LeftFrames[12],
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFourth] = LeftFrames[13],
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFifth] = LeftFrames[14],
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardSixth] = LeftFrames[15],
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFirst] = LeftFrames[16],
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardSecond] = LeftFrames[17],
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardThird] = LeftFrames[18],
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFourth] = LeftFrames[19],
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFifth] = LeftFrames[20],
        [EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardSixth] = LeftFrames[21],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSecond] = LeftFrames[22],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardThird] = LeftFrames[23],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardFourth] = LeftFrames[24],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardFifth] = LeftFrames[25],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSixth] = LeftFrames[26],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSeventh] = LeftFrames[27],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardEighth] = LeftFrames[28],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardNinth] = LeftFrames[29],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardTenth] = LeftFrames[30],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardEleventh] = LeftFrames[31],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSecond] = LeftFrames[32],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardThird] = LeftFrames[33],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardFourth] = LeftFrames[34],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardFifth] = LeftFrames[35],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSixth] = LeftFrames[36],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSeventh] = LeftFrames[37],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardEighth] = LeftFrames[38],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardNinth] = LeftFrames[39],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardTenth] = LeftFrames[40],
        [EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardEleventh] = LeftFrames[41],
        [EnemyFrameId.TorizoFacingLeftSeated] = AwakeningFrames[0],
        [EnemyFrameId.TorizoFacingLeftStandingUpFirst] = AwakeningFrames[1],
        [EnemyFrameId.TorizoFacingLeftStandingUpSecond] = AwakeningFrames[2],
        [EnemyFrameId.GoldenTorizoInitial] = AwakeningFrames[3],
        [EnemyFrameId.TorizoFacingLeftStandingUpFourth] = AwakeningFrames[4],
        [EnemyFrameId.TorizoFacingLeftStandingUpFifth] = AwakeningFrames[5],
        [EnemyFrameId.TorizoFacingLeftStandingUpSixth] = AwakeningFrames[6],
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFirst] = MirroredFrames[0],
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingSecond] = MirroredFrames[1],
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingThird] = MirroredFrames[2],
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFourth] = MirroredFrames[3],
        [EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFifth] = MirroredFrames[4],
        [EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFirst] = MirroredFrames[5],
        [EnemyFrameId.TorizoFacingRightWalkingRightLegMovingSecond] = MirroredFrames[6],
        [EnemyFrameId.TorizoFacingRightWalkingRightLegMovingThird] = MirroredFrames[7],
        [EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFourth] = MirroredFrames[8],
        [EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFifth] = MirroredFrames[9],
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFirst] = MirroredFrames[10],
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardSecond] = MirroredFrames[11],
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardThird] = MirroredFrames[12],
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFourth] = MirroredFrames[13],
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFifth] = MirroredFrames[14],
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardSixth] = MirroredFrames[15],
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFirst] = MirroredFrames[16],
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardSecond] = MirroredFrames[17],
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardThird] = MirroredFrames[18],
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFourth] = MirroredFrames[19],
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFifth] = MirroredFrames[20],
        [EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardSixth] = MirroredFrames[21],
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSecond] = MirroredFrames[22],
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardThird] = MirroredFrames[23],
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardFourth] = MirroredFrames[24],
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardFifth] = MirroredFrames[25],
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSixth] = MirroredFrames[26],
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSeventh] = MirroredFrames[27],
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardEighth] = MirroredFrames[28],
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardNinth] = MirroredFrames[29],
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardTenth] = MirroredFrames[30],
        [EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardEleventh] = MirroredFrames[31],
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSecond] = MirroredFrames[32],
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardThird] = MirroredFrames[33],
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardFourth] = MirroredFrames[34],
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardFifth] = MirroredFrames[35],
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSixth] = MirroredFrames[36],
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSeventh] = MirroredFrames[37],
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardEighth] = MirroredFrames[38],
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardNinth] = MirroredFrames[39],
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardTenth] = MirroredFrames[40],
        [EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardEleventh] = MirroredFrames[41],
        [EnemyFrameId.TorizoFacingRightSeated] = MirroredFrames[42],
        [EnemyFrameId.TorizoFacingRightStandingUpFirst] = MirroredFrames[43],
        [EnemyFrameId.TorizoFacingRightStandingUpSecond] = MirroredFrames[44],
        [EnemyFrameId.TorizoFacingRightStandingUpThird] = MirroredFrames[45],
        [EnemyFrameId.TorizoFacingRightStandingUpFourth] = MirroredFrames[46],
        [EnemyFrameId.TorizoFacingRightStandingUpFifth] = MirroredFrames[47],
        [EnemyFrameId.TorizoFacingRightStandingUpSixth] = MirroredFrames[48],
        [EnemyFrameId.TorizoFacingLeftJumpingForwardsFirst] = JumpBackFrames[0],
        [EnemyFrameId.TorizoFallingLeft] = JumpBackFrames[1],
        [EnemyFrameId.TorizoFacingLeftJumpingForwardsThird] = JumpBackFrames[2],
        [EnemyFrameId.TorizoFacingRightJumpingForwardsFirst] = JumpBackFrames[3],
        [EnemyFrameId.TorizoFacingRightJumpingForwardsSecond] = JumpBackFrames[4],
        [EnemyFrameId.TorizoFacingRightJumpingForwardsThird] = JumpBackFrames[5],
    };
}
