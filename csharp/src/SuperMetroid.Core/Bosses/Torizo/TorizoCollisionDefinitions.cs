using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Ordered physical components and touch/shot rectangles for all 106 Torizo frames selected
/// by the compiled bank-$AA instruction lists. OAM composition and pixels are not stored here.
/// The native blank frame has an empty hitbox list, rather than invented collision.
/// </summary>
/// <remarks>
/// Reviewed for #1165; the single owner of the bank-$AA Torizo collision space. Frame records
/// are contiguous within four runs ($87D0 blank, $A4F0 turning plus 42 left-facing frames,
/// $AA12 seven awakening frames, $AA98 right-facing frames), so every pointer derives from its
/// run start and the preceding record sizes (a count word plus eight bytes per component).
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
        new([new(-15, -27, 13, 27, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]),
        new([new(-13, -27, 13, 27, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]),
        new([new(-14, -32, 13, 33, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]),
        new([new(-14, -35, 11, 39, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]),
        new([new(-6, -42, 13, 47, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]),
        new([new(-7, -41, 11, 47, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]),
        new([new(-8, -41, 22, 47, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]),
    ];
    /// <summary>Right-facing active body.</summary>
    private static readonly TorizoHitboxList ActiveBodyRight = new([new(-10, -38, 13, 23, TorizoHitboxTouch.Body, TorizoHitboxShot.Body)]);
    /// <summary>Right-facing standing body, extending farther down and right than the right-facing jump-back body.</summary>
    private static readonly TorizoHitboxList StandingBodyRight = new([new(-9, -40, 16, 25, TorizoHitboxTouch.Body, TorizoHitboxShot.Body)]);
    /// <summary>Compact right-facing jump-back body, fitted separately from its left counterpart; the right-facing jump-back frames use it directly.</summary>
    private static readonly TorizoHitboxList JumpBackBodyRight = new([new(-8, -37, 15, 14, TorizoHitboxTouch.Body, TorizoHitboxShot.Body)]);

    /// <summary>
    /// The seven awakening bodies with the stand-up/sit-down shot reaction, indexed by awakening step: a 32-by-54 body,
    /// narrowed on the left, taller, centered and extending farther vertically, its upper reach shifted left, shortened
    /// below the center, and finally narrowing below under the tall upper reach.
    /// </summary>
    private static readonly TorizoHitboxList[] AwakeningSteps =
    [
        new([new(-16, -27, 16, 27, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStepsRight[0]),
        new([new(-14, -27, 13, 27, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStepsRight[1]),
        new([new(-13, -34, 9, 33, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStepsRight[2]),
        new([new(-11, -38, 11, 39, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStepsRight[3]),
        new([new(-15, -44, 8, 47, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStepsRight[4]),
        new([new(-18, -43, 3, 24, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStepsRight[5]),
        new([new(-17, -42, 5, 15, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStepsRight[6]),
    ];
    /// <summary>Left-facing active body, with the ordinary body shot reaction.</summary>
    private static readonly TorizoHitboxList ActiveBody = new([new(-15, -39, 7, 21, TorizoHitboxTouch.Body, TorizoHitboxShot.Body)], ActiveBodyRight);
    /// <summary>Compact left-facing jump-back body with its lower edge at Y 9.</summary>
    private static readonly TorizoHitboxList JumpBackBody = new([new(-18, -38, 7, 9, TorizoHitboxTouch.Body, TorizoHitboxShot.Body)], JumpBackBodyRight);
    /// <summary>Left-facing standing body, extending to Y 18.</summary>
    private static readonly TorizoHitboxList StandingBody = new([new(-18, -37, 7, 18, TorizoHitboxTouch.Body, TorizoHitboxShot.Body)], StandingBodyRight);

    /// <summary>Single origin component used by the native blank and shared turning frames.</summary>
    private static readonly GoldenTorizoCollisionComponent[] SingleEmpty = [new(new(0, 0), Empty)];

    /// <summary>The 42 left-facing frames from $AA:A4FA, in native order.</summary>
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

    /// <summary>The seven awakening frames from $AA:AA12, in native order.</summary>
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
        [EnemyFrameId.GoldenTorizoRightward0] = SingleEmpty,
        [EnemyFrameId.GoldenTorizoWalkLeft0] = LeftFrames[0],
        [EnemyFrameId.GoldenTorizoWalkLeft1] = LeftFrames[1],
        [EnemyFrameId.GoldenTorizoWalkLeft2] = LeftFrames[2],
        [EnemyFrameId.GoldenTorizoWalkLeft3] = LeftFrames[3],
        [EnemyFrameId.GoldenTorizoWalkLeft4] = LeftFrames[4],
        [EnemyFrameId.GoldenTorizoWalkLeft5] = LeftFrames[5],
        [EnemyFrameId.GoldenTorizoWalkLeft6] = LeftFrames[6],
        [EnemyFrameId.GoldenTorizoWalkLeft7] = LeftFrames[7],
        [EnemyFrameId.GoldenTorizoWalkLeft8] = LeftFrames[8],
        [EnemyFrameId.GoldenTorizoWalkLeft9] = LeftFrames[9],
        [EnemyFrameId.GoldenTorizoLeftOrb0] = LeftFrames[10],
        [EnemyFrameId.GoldenTorizoLeftOrb1] = LeftFrames[11],
        [EnemyFrameId.GoldenTorizoLeftOrb2] = LeftFrames[12],
        [EnemyFrameId.GoldenTorizoLeftOrb3] = LeftFrames[13],
        [EnemyFrameId.GoldenTorizoLeftOrb4] = LeftFrames[14],
        [EnemyFrameId.GoldenTorizoLeftOrb5] = LeftFrames[15],
        [EnemyFrameId.GoldenTorizoLeftOrb6] = LeftFrames[16],
        [EnemyFrameId.GoldenTorizoLeftOrb7] = LeftFrames[17],
        [EnemyFrameId.GoldenTorizoLeftOrb8] = LeftFrames[18],
        [EnemyFrameId.GoldenTorizoLeftOrb9] = LeftFrames[19],
        [EnemyFrameId.GoldenTorizoLeftOrb10] = LeftFrames[20],
        [EnemyFrameId.GoldenTorizoLeftOrb11] = LeftFrames[21],
        [EnemyFrameId.TorizoCombat0] = LeftFrames[22],
        [EnemyFrameId.TorizoCombat1] = LeftFrames[23],
        [EnemyFrameId.TorizoCombat2] = LeftFrames[24],
        [EnemyFrameId.TorizoCombat3] = LeftFrames[25],
        [EnemyFrameId.TorizoCombat4] = LeftFrames[26],
        [EnemyFrameId.TorizoCombat5] = LeftFrames[27],
        [EnemyFrameId.TorizoCombat6] = LeftFrames[28],
        [EnemyFrameId.TorizoCombat7] = LeftFrames[29],
        [EnemyFrameId.TorizoCombat8] = LeftFrames[30],
        [EnemyFrameId.TorizoCombat9] = LeftFrames[31],
        [EnemyFrameId.TorizoCombat10] = LeftFrames[32],
        [EnemyFrameId.TorizoCombat11] = LeftFrames[33],
        [EnemyFrameId.TorizoCombat12] = LeftFrames[34],
        [EnemyFrameId.TorizoCombat13] = LeftFrames[35],
        [EnemyFrameId.TorizoCombat14] = LeftFrames[36],
        [EnemyFrameId.TorizoCombat15] = LeftFrames[37],
        [EnemyFrameId.TorizoCombat16] = LeftFrames[38],
        [EnemyFrameId.TorizoCombat17] = LeftFrames[39],
        [EnemyFrameId.TorizoCombat18] = LeftFrames[40],
        [EnemyFrameId.TorizoCombat19] = LeftFrames[41],
        [EnemyFrameId.GoldenTorizoAwake2] = AwakeningFrames[0],
        [EnemyFrameId.GoldenTorizoAwake1] = AwakeningFrames[1],
        [EnemyFrameId.GoldenTorizoAwake0] = AwakeningFrames[2],
        [EnemyFrameId.GoldenTorizoInitial] = AwakeningFrames[3],
        [EnemyFrameId.GoldenTorizoAwake3] = AwakeningFrames[4],
        [EnemyFrameId.GoldenTorizoAwake4] = AwakeningFrames[5],
        [EnemyFrameId.GoldenTorizoAwake5] = AwakeningFrames[6],
        [EnemyFrameId.GoldenTorizoRightward1] = MirroredFrames[0],
        [EnemyFrameId.GoldenTorizoRightward2] = MirroredFrames[1],
        [EnemyFrameId.GoldenTorizoRightward3] = MirroredFrames[2],
        [EnemyFrameId.GoldenTorizoRightward4] = MirroredFrames[3],
        [EnemyFrameId.GoldenTorizoRightward5] = MirroredFrames[4],
        [EnemyFrameId.GoldenTorizoRightward6] = MirroredFrames[5],
        [EnemyFrameId.GoldenTorizoRightward7] = MirroredFrames[6],
        [EnemyFrameId.GoldenTorizoRightward8] = MirroredFrames[7],
        [EnemyFrameId.GoldenTorizoRightward9] = MirroredFrames[8],
        [EnemyFrameId.GoldenTorizoRightward10] = MirroredFrames[9],
        [EnemyFrameId.GoldenTorizoRightSonic0] = MirroredFrames[10],
        [EnemyFrameId.GoldenTorizoLeftFootOrb0] = MirroredFrames[11],
        [EnemyFrameId.GoldenTorizoLeftFootOrb1] = MirroredFrames[12],
        [EnemyFrameId.GoldenTorizoLeftFootOrb2] = MirroredFrames[13],
        [EnemyFrameId.GoldenTorizoLeftFootOrb3] = MirroredFrames[14],
        [EnemyFrameId.GoldenTorizoLeftFootOrb4] = MirroredFrames[15],
        [EnemyFrameId.GoldenTorizoRightOrb0] = MirroredFrames[16],
        [EnemyFrameId.GoldenTorizoRightOrb1] = MirroredFrames[17],
        [EnemyFrameId.GoldenTorizoRightOrb2] = MirroredFrames[18],
        [EnemyFrameId.GoldenTorizoRightOrb3] = MirroredFrames[19],
        [EnemyFrameId.GoldenTorizoRightOrb4] = MirroredFrames[20],
        [EnemyFrameId.GoldenTorizoRightOrb5] = MirroredFrames[21],
        [EnemyFrameId.GoldenTorizoRightSonic1] = MirroredFrames[22],
        [EnemyFrameId.GoldenTorizoRightSonic2] = MirroredFrames[23],
        [EnemyFrameId.GoldenTorizoRightSonic3] = MirroredFrames[24],
        [EnemyFrameId.GoldenTorizoRightSonic4] = MirroredFrames[25],
        [EnemyFrameId.GoldenTorizoRightSonic5] = MirroredFrames[26],
        [EnemyFrameId.GoldenTorizoRightSonic6] = MirroredFrames[27],
        [EnemyFrameId.GoldenTorizoRightSonic7] = MirroredFrames[28],
        [EnemyFrameId.GoldenTorizoRightSonic8] = MirroredFrames[29],
        [EnemyFrameId.GoldenTorizoRightSonic9] = MirroredFrames[30],
        [EnemyFrameId.GoldenTorizoRightSonic10] = MirroredFrames[31],
        [EnemyFrameId.GoldenTorizoRightSonic11] = MirroredFrames[32],
        [EnemyFrameId.GoldenTorizoRightSonic12] = MirroredFrames[33],
        [EnemyFrameId.GoldenTorizoRightSonic13] = MirroredFrames[34],
        [EnemyFrameId.GoldenTorizoRightSonic14] = MirroredFrames[35],
        [EnemyFrameId.GoldenTorizoRightSonic15] = MirroredFrames[36],
        [EnemyFrameId.GoldenTorizoRightSonic16] = MirroredFrames[37],
        [EnemyFrameId.GoldenTorizoRightSonic17] = MirroredFrames[38],
        [EnemyFrameId.GoldenTorizoRightSonic18] = MirroredFrames[39],
        [EnemyFrameId.GoldenTorizoRightSonic19] = MirroredFrames[40],
        [EnemyFrameId.GoldenTorizoRightSonic20] = MirroredFrames[41],
        [EnemyFrameId.TorizoCombat20] = MirroredFrames[42],
        [EnemyFrameId.TorizoCombat21] = MirroredFrames[43],
        [EnemyFrameId.TorizoCombat22] = MirroredFrames[44],
        [EnemyFrameId.TorizoCombat23] = MirroredFrames[45],
        [EnemyFrameId.TorizoCombat24] = MirroredFrames[46],
        [EnemyFrameId.TorizoCombat25] = MirroredFrames[47],
        [EnemyFrameId.TorizoCombat26] = MirroredFrames[48],
        [EnemyFrameId.TorizoJumpBackLeft0] = JumpBackFrames[0],
        [EnemyFrameId.TorizoFallingLeft] = JumpBackFrames[1],
        [EnemyFrameId.TorizoJumpBackLeft1] = JumpBackFrames[2],
        [EnemyFrameId.TorizoJumpBack0] = JumpBackFrames[3],
        [EnemyFrameId.TorizoJumpBack1] = JumpBackFrames[4],
        [EnemyFrameId.TorizoJumpBack2] = JumpBackFrames[5],
    };
}
