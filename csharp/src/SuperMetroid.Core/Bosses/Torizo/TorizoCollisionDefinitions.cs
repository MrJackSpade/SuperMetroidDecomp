using SuperMetroid.Core.Enemies.Artwork;
using System.Diagnostics.CodeAnalysis;
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

    /// <summary>
    /// The right-facing counterparts of the left and awakening frames, in mirror order: each component's X reflected and
    /// each hitbox list replaced by its right-facing counterpart.
    /// </summary>
    private static readonly GoldenTorizoCollisionComponent[][] MirroredFrames =
        [.. Enumerable.Range(0, MirroredCount).Select(index => LeftFacing(index)
            .Select(part => part with { Offset = part.Offset.MirroredX(), Hitboxes = part.Hitboxes.RightFacing }).ToArray())];

    /// <summary>
    /// Each Torizo frame's components: the blank and turning frames carry one empty component; left-facing, awakening
    /// and jump-back frames their own components; right-facing frames the precomputed mirror images.
    /// </summary>
    private static readonly Dictionary<EnemyFrameId, GoldenTorizoCollisionComponent[]> FrameComponents = new()
    {
        [EnemyFrameId.BombTorizoDormant] = SingleEmpty,
        [EnemyFrameId.GoldenTorizoRightwardA4F0] = SingleEmpty,
        [EnemyFrameId.GoldenTorizoWalkLeftA4FA] = LeftFrames[0],
        [EnemyFrameId.GoldenTorizoWalkLeftA51C] = LeftFrames[1],
        [EnemyFrameId.GoldenTorizoWalkLeftA53E] = LeftFrames[2],
        [EnemyFrameId.GoldenTorizoWalkLeftA560] = LeftFrames[3],
        [EnemyFrameId.GoldenTorizoWalkLeftA582] = LeftFrames[4],
        [EnemyFrameId.GoldenTorizoWalkLeftA5A4] = LeftFrames[5],
        [EnemyFrameId.GoldenTorizoWalkLeftA5C6] = LeftFrames[6],
        [EnemyFrameId.GoldenTorizoWalkLeftA5E8] = LeftFrames[7],
        [EnemyFrameId.GoldenTorizoWalkLeftA60A] = LeftFrames[8],
        [EnemyFrameId.GoldenTorizoWalkLeftA62C] = LeftFrames[9],
        [EnemyFrameId.GoldenTorizoLeftOrbA64E] = LeftFrames[10],
        [EnemyFrameId.GoldenTorizoLeftOrbA668] = LeftFrames[11],
        [EnemyFrameId.GoldenTorizoLeftOrbA682] = LeftFrames[12],
        [EnemyFrameId.GoldenTorizoLeftOrbA69C] = LeftFrames[13],
        [EnemyFrameId.GoldenTorizoLeftOrbA6B6] = LeftFrames[14],
        [EnemyFrameId.GoldenTorizoLeftOrbA6D0] = LeftFrames[15],
        [EnemyFrameId.GoldenTorizoLeftOrbA6EA] = LeftFrames[16],
        [EnemyFrameId.GoldenTorizoLeftOrbA704] = LeftFrames[17],
        [EnemyFrameId.GoldenTorizoLeftOrbA71E] = LeftFrames[18],
        [EnemyFrameId.GoldenTorizoLeftOrbA738] = LeftFrames[19],
        [EnemyFrameId.GoldenTorizoLeftOrbA752] = LeftFrames[20],
        [EnemyFrameId.GoldenTorizoLeftOrbA76C] = LeftFrames[21],
        [EnemyFrameId.TorizoCombatA786] = LeftFrames[22],
        [EnemyFrameId.TorizoCombatA7A0] = LeftFrames[23],
        [EnemyFrameId.TorizoCombatA7C2] = LeftFrames[24],
        [EnemyFrameId.TorizoCombatA7E4] = LeftFrames[25],
        [EnemyFrameId.TorizoCombatA806] = LeftFrames[26],
        [EnemyFrameId.TorizoCombatA828] = LeftFrames[27],
        [EnemyFrameId.TorizoCombatA84A] = LeftFrames[28],
        [EnemyFrameId.TorizoCombatA86C] = LeftFrames[29],
        [EnemyFrameId.TorizoCombatA88E] = LeftFrames[30],
        [EnemyFrameId.TorizoCombatA8B0] = LeftFrames[31],
        [EnemyFrameId.TorizoCombatA8D2] = LeftFrames[32],
        [EnemyFrameId.TorizoCombatA8EC] = LeftFrames[33],
        [EnemyFrameId.TorizoCombatA906] = LeftFrames[34],
        [EnemyFrameId.TorizoCombatA920] = LeftFrames[35],
        [EnemyFrameId.TorizoCombatA93A] = LeftFrames[36],
        [EnemyFrameId.TorizoCombatA954] = LeftFrames[37],
        [EnemyFrameId.TorizoCombatA976] = LeftFrames[38],
        [EnemyFrameId.TorizoCombatA998] = LeftFrames[39],
        [EnemyFrameId.TorizoCombatA9BA] = LeftFrames[40],
        [EnemyFrameId.TorizoCombatA9DC] = LeftFrames[41],
        [EnemyFrameId.GoldenTorizoAwakeAA12] = AwakeningFrames[0],
        [EnemyFrameId.GoldenTorizoAwakeAA1C] = AwakeningFrames[1],
        [EnemyFrameId.GoldenTorizoAwakeAA26] = AwakeningFrames[2],
        [EnemyFrameId.GoldenTorizoInitial] = AwakeningFrames[3],
        [EnemyFrameId.GoldenTorizoAwakeAA3A] = AwakeningFrames[4],
        [EnemyFrameId.GoldenTorizoAwakeAA4C] = AwakeningFrames[5],
        [EnemyFrameId.GoldenTorizoAwakeAA5E] = AwakeningFrames[6],
        [EnemyFrameId.GoldenTorizoRightwardAA98] = MirroredFrames[0],
        [EnemyFrameId.GoldenTorizoRightwardAABA] = MirroredFrames[1],
        [EnemyFrameId.GoldenTorizoRightwardAADC] = MirroredFrames[2],
        [EnemyFrameId.GoldenTorizoRightwardAAFE] = MirroredFrames[3],
        [EnemyFrameId.GoldenTorizoRightwardAB20] = MirroredFrames[4],
        [EnemyFrameId.GoldenTorizoRightwardAB42] = MirroredFrames[5],
        [EnemyFrameId.GoldenTorizoRightwardAB64] = MirroredFrames[6],
        [EnemyFrameId.GoldenTorizoRightwardAB86] = MirroredFrames[7],
        [EnemyFrameId.GoldenTorizoRightwardABA8] = MirroredFrames[8],
        [EnemyFrameId.GoldenTorizoRightwardABCA] = MirroredFrames[9],
        [EnemyFrameId.GoldenTorizoRightSonicABEC] = MirroredFrames[10],
        [EnemyFrameId.GoldenTorizoLeftFootOrbAC06] = MirroredFrames[11],
        [EnemyFrameId.GoldenTorizoLeftFootOrbAC20] = MirroredFrames[12],
        [EnemyFrameId.GoldenTorizoLeftFootOrbAC3A] = MirroredFrames[13],
        [EnemyFrameId.GoldenTorizoLeftFootOrbAC54] = MirroredFrames[14],
        [EnemyFrameId.GoldenTorizoLeftFootOrbAC6E] = MirroredFrames[15],
        [EnemyFrameId.GoldenTorizoRightOrbAC88] = MirroredFrames[16],
        [EnemyFrameId.GoldenTorizoRightOrbACA2] = MirroredFrames[17],
        [EnemyFrameId.GoldenTorizoRightOrbACBC] = MirroredFrames[18],
        [EnemyFrameId.GoldenTorizoRightOrbACD6] = MirroredFrames[19],
        [EnemyFrameId.GoldenTorizoRightOrbACF0] = MirroredFrames[20],
        [EnemyFrameId.GoldenTorizoRightOrbAD0A] = MirroredFrames[21],
        [EnemyFrameId.GoldenTorizoRightSonicAD24] = MirroredFrames[22],
        [EnemyFrameId.GoldenTorizoRightSonicAD3E] = MirroredFrames[23],
        [EnemyFrameId.GoldenTorizoRightSonicAD60] = MirroredFrames[24],
        [EnemyFrameId.GoldenTorizoRightSonicAD82] = MirroredFrames[25],
        [EnemyFrameId.GoldenTorizoRightSonicADA4] = MirroredFrames[26],
        [EnemyFrameId.GoldenTorizoRightSonicADC6] = MirroredFrames[27],
        [EnemyFrameId.GoldenTorizoRightSonicADE8] = MirroredFrames[28],
        [EnemyFrameId.GoldenTorizoRightSonicAE0A] = MirroredFrames[29],
        [EnemyFrameId.GoldenTorizoRightSonicAE2C] = MirroredFrames[30],
        [EnemyFrameId.GoldenTorizoRightSonicAE4E] = MirroredFrames[31],
        [EnemyFrameId.GoldenTorizoRightSonicAE70] = MirroredFrames[32],
        [EnemyFrameId.GoldenTorizoRightSonicAE8A] = MirroredFrames[33],
        [EnemyFrameId.GoldenTorizoRightSonicAEA4] = MirroredFrames[34],
        [EnemyFrameId.GoldenTorizoRightSonicAEBE] = MirroredFrames[35],
        [EnemyFrameId.GoldenTorizoRightSonicAED8] = MirroredFrames[36],
        [EnemyFrameId.GoldenTorizoRightSonicAEF2] = MirroredFrames[37],
        [EnemyFrameId.GoldenTorizoRightSonicAF14] = MirroredFrames[38],
        [EnemyFrameId.GoldenTorizoRightSonicAF36] = MirroredFrames[39],
        [EnemyFrameId.GoldenTorizoRightSonicAF58] = MirroredFrames[40],
        [EnemyFrameId.GoldenTorizoRightSonicAF7A] = MirroredFrames[41],
        [EnemyFrameId.TorizoCombatAF9C] = MirroredFrames[42],
        [EnemyFrameId.TorizoCombatAFA6] = MirroredFrames[43],
        [EnemyFrameId.TorizoCombatAFB0] = MirroredFrames[44],
        [EnemyFrameId.TorizoCombatAFBA] = MirroredFrames[45],
        [EnemyFrameId.TorizoCombatAFC4] = MirroredFrames[46],
        [EnemyFrameId.TorizoCombatAFD6] = MirroredFrames[47],
        [EnemyFrameId.TorizoCombatAFE8] = MirroredFrames[48],
        [EnemyFrameId.TorizoJumpBackLeftAFFA] = JumpBackFrames[0],
        [EnemyFrameId.TorizoFallingLeftB014] = JumpBackFrames[1],
        [EnemyFrameId.TorizoJumpBackLeftB02E] = JumpBackFrames[2],
        [EnemyFrameId.TorizoJumpBackB048] = JumpBackFrames[3],
        [EnemyFrameId.TorizoJumpBackB062] = JumpBackFrames[4],
        [EnemyFrameId.TorizoJumpBackB07C] = JumpBackFrames[5],
    };

    /// <summary>The collision components of Torizo frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Torizo's frames.</exception>
    internal static ReadOnlySpan<GoldenTorizoCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && FrameComponents.TryGetValue(shown, out GoldenTorizoCollisionComponent[]? components)
            ? components
            : throw new InvalidDataException($"Torizo frame {frame} is not compiled.");

    /// <summary>The <paramref name="index"/>th left-facing frame in mirror order.</summary>
    internal static GoldenTorizoCollisionComponent[] LeftFacing(int index) =>
        index < LeftFrames.Length ? LeftFrames[index] : AwakeningFrames[index - LeftFrames.Length];

}
