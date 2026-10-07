using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Crocomire's body, melting and skeleton instruction programs.
/// Interleaved selections resolve compiled physical identities to installed presentation data.
/// </summary>
internal abstract class CrocomireInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>
    /// <c>InstList_Crocomire_Initial</c> at $A4:BADE. The pinned NTSC J/U
    /// v1.0 ROM contains one-frame duration $0001 at BADE, a live
    /// spritemap operand at BAE0, Fight AI $86A6 at BAE2, goto $80ED at
    /// BAE4, target BADE at BAE6, and trailing Sleep $812F at BAE8.
    /// Normal goto control skips Sleep; Fight AI may redirect to another
    /// list. Retain this authored callback/control order rather than an
    /// address classifier. The next program starts at BAEA.
    /// </summary>
    internal const ushort Initial = 0xbade;
    /// <summary>
    /// <c>UNUSED_InstList_Crocomire_ChargeForwardOneStep_A4BAEA</c> at
    /// $A4:BAEA-$BB35. The pinned NTSC J/U v1.0 ROM has twelve $0008
    /// duration words interleaved with twelve live spritemap operands.
    /// Its fourteen mechanics callbacks, in execution order, are $8FC7,
    /// $8FFA, $8FDF, $8FDF, $8FFA, $8FDF, $8FDF, $8FC7, $8FFA, $8FDF,
    /// $8FDF, $8FFA, $8FDF, then Fight AI $86A6. The constant duration
    /// subrule does not determine this authored move, dust, and shake
    /// schedule. Retain its control order; the unused ChooseAttack fight
    /// branch can select this list. $BB36 starts a different program.
    /// </summary>
    internal const ushort UnusedChargeForwardOneStep = 0xbaea;
    /// <summary>
    /// <c>InstList_Crocomire_ProjectileAttack_0</c> at $A4:BB36-$BB93.
    /// The pinned NTSC J/U v1.0 ROM interleaves eighteen $0005 durations
    /// and live spritemap operands with callbacks for dust projectiles,
    /// explosion sound, and a final cry. Projectile offsets occur in
    /// authored order: +10, 0, -20, -10, 0, +28, -10; explosion sound
    /// occurs before -20, before the second 0, and before the final -10.
    /// Retain this event schedule rather than deriving it from the
    /// constant duration run. $BB94 starts the loop below.
    /// </summary>
    internal const ushort ProjectileAttack = 0xbb36;
    /// <summary>
    /// <c>InstList_Crocomire_ProjectileAttack_1</c> at $A4:BB94-$BBAD.
    /// Fight AI $86A6 precedes durations $0008 and four $0007 words,
    /// each with a live spritemap operand, then goto $80ED targets $BB94.
    /// These exact control and timing words are retained; $BBAE begins a
    /// different, excluded unreferenced list.
    /// </summary>
    internal const ushort ProjectileAttackLoop = 0xbb94;
    /// <summary>
    /// <c>InstList_Crocomire_StepForwardAfterDelay</c> at $A4:BBCA.
    /// Its only mechanics word is the pinned ROM's $00B4 (180-frame)
    /// duration. A live spritemap operand follows at BBCC; execution
    /// then falls into StepForward at BBCE. Retain this authored delay.
    /// </summary>
    internal const ushort StepForwardAfterDelay = 0xbbca;
    /// <summary>
    /// <c>InstList_Crocomire_StepForward</c> at $A4:BBCE-$BC2F.
    /// The pinned ROM's twelve durations are three $0005 words, one
    /// $0010, then eight $0004 words, each followed by a live spritemap
    /// operand. Its other 25 mechanics words form an authored sequence:
    /// optional projectile attack, Fight AI callbacks, cry, leftward
    /// movement and dust, shake, then goto $80ED targeting BBCE at
    /// BC2C-BC2E. Retain the exact callback order; the duration subrule
    /// alone cannot reproduce its effects. StepBack begins at BC30.
    /// </summary>
    internal const ushort StepForward = 0xbbce;
    /// <summary>
    /// <c>InstList_Crocomire_StepBack</c> at $A4:BC30. The pinned ROM
    /// has one $0002 duration, a live spritemap operand at BC32, then
    /// falls into SteppingBack at BC34. Retain this authored two-frame
    /// entry; there is no indexed mechanics progression to derive.
    /// </summary>
    internal const ushort StepBack = 0xbc30;
    /// <summary>
    /// <c>InstList_Crocomire_SteppingBack</c> at $A4:BC34-$BC55.
    /// Five $0008 durations each precede a live spritemap operand.
    /// Pinned-ROM callbacks then order rightward moves as cloud, plain,
    /// plain, cloud, shake plus plain, and final Fight AI $86A6.
    /// Retain this authored movement/effect schedule; the constant
    /// duration rule does not determine callback placement. The
    /// wait-for-damage program begins at BC56.
    /// </summary>
    internal const ushort SteppingBack = 0xbc34;
    /// <summary>
    /// <c>InstList_Crocomire_WaitForFirstSecondDamage</c> at
    /// $A4:BC56-$BCD7. For the 32 pinned-ROM duration words, zero-based
    /// D(0)=$0022, D(1..6)=$0002, D(7)=$0010,
    /// D(8..12)=$0001, D(13)=$0010, and D(14..31)=$0001.
    /// Each precedes a live spritemap operand. Fight AI $86A6 at BCD6
    /// terminates this authored instruction schedule; MovingClaws starts
    /// at BCD8. The run-length rule is exact only for indices 0..31.
    /// </summary>
    internal const ushort WaitForFirstSecondDamage = 0xbc56;
    /// <summary>
    /// <c>InstList_Crocomire_WaitForFirstSecondDamage_MovingClaws</c> at
    /// $A4:BCD8-$BD29. Its first $50 bytes are identical to the pinned
    /// ROM's ProjectileAttack prefix at $A4:BB36-$BB85, translated by
    /// +$01A2. That covers fifteen $0005 durations, their live
    /// spritemap operands, and the authored dust/SFX callbacks.
    /// MovingClaws instead ends with Fight AI $86A6 at BD28.
    /// The prefix identity does not extend into either list's ending;
    /// retain this distinct control word. Roar starts at BD2A.
    /// </summary>
    internal const ushort MovingClaws = 0xbcd8;
    /// <summary>
    /// <c>InstList_Crocomire_WaitForFirstSecondDamage_Roar</c> at
    /// $A4:BD2A-$BD8D. The pinned ROM begins with duration $0030,
    /// cry SFX $8CFB, then duration $0005. From BD34, thirteen
    /// repetitions each hold for $0002 and call Fight AI $86A6.
    /// Duration $0020 plus Fight AI and then $0001 plus Fight AI
    /// finish the list. Each of the 17 durations precedes a live
    /// spritemap operand. This exact repetition ends before the
    /// RoarCloseMouth program at BD8E.
    /// </summary>
    internal const ushort Roar = 0xbd2a;
    /// <summary>
    /// <c>InstList_Crocomire_WaitForFirstSecondDamage_RoarCloseMouth_0</c>
    /// at $A4:BD8E-$BDA1. Pinned-ROM mechanics begin with $0020,
    /// Fight AI $86A6, $0005, Fight AI, $0008, then $0002.
    /// Each duration has a live spritemap operand. This authored
    /// timing/control order continues into the loop entry at BDA2.
    /// </summary>
    internal const ushort RoarCloseMouth = 0xbd8e;
    /// <summary>
    /// <c>InstList_Crocomire_WaitForFirstSecondDamage_RoarCloseMouth_1</c>
    /// at $A4:BDA2-$BDAD. Two $0001 durations each have a live
    /// spritemap operand and Fight AI $86A6 callback. Retain this
    /// short control sequence; the next program begins at BDAE.
    /// </summary>
    internal const ushort RoarCloseMouthLoop = 0xbda2;
    /// <summary>
    /// <c>InstList_Crocomire_PowerBombReaction_MouthFullyOpen</c> at
    /// $A4:BDAE. Its pinned-ROM mechanics word is duration $0002,
    /// followed by a live spritemap operand at BDB0. A fully-open
    /// reaction falls through the partially-open two-frame entry
    /// at BDB2 before the closed-mouth program at BDB6.
    /// </summary>
    internal const ushort PowerBombReactionMouthFullyOpen = 0xbdae;
    /// <summary>
    /// <c>InstList_Crocomire_PowerBombReaction_MouthPartiallyOpen</c>
    /// at $A4:BDB2. Its pinned-ROM mechanics word is also duration
    /// $0002, followed by a live spritemap operand at BDB4. A
    /// partially-open reaction starts here and falls into the
    /// closed-mouth program at BDB6. These two entry durations
    /// form an exact bounded constant rule, with distinct entry paths.
    /// </summary>
    internal const ushort PowerBombReactionMouthPartiallyOpen = 0xbdb2;
    /// <summary>
    /// <c>InstList_Crocomire_PowerBombReaction_MouthNotOpen_0</c> at
    /// $A4:BDB6-$BE05. This pinned-ROM $50-byte prefix equals
    /// ProjectileAttack's $A4:BB36-$BB85 prefix under +$0280 address
    /// translation. It contains fifteen $0005 durations, their live
    /// spritemap operands, and the same authored dust-projectile and
    /// explosion-SFX callbacks. The identity ends before the loop at BE06.
    /// </summary>
    internal const ushort PowerBombReactionMouthNotOpen = 0xbdb6;
    /// <summary>
    /// <c>InstList_Crocomire_PowerBombReaction_MouthNotOpen_1</c> at
    /// $A4:BE06-$BE55. Twelve $0004 durations each have a live
    /// spritemap operand. Authored callbacks shake, move left with
    /// dust or plainly, then call Fight AI $86A6; goto $80ED at
    /// BE52 targets BE06 at BE54. Retain their exact order rather
    /// than deriving side effects from the constant duration rule.
    /// </summary>
    internal const ushort PowerBombReactionMouthNotOpenLoop = 0xbe06;
    /// <summary>
    /// <c>InstList_CrocomireTongue_NearSpikeWallCharge_0</c> at
    /// $A4:BE7E-$BEEB. Its 18 pinned-ROM durations are two $0005
    /// words, thirteen $0002 words, then $0005, $0008, $0002.
    /// The first hold queues cry $8CFB, and each hold calls Fight AI
    /// $86A6. Every duration has a live spritemap operand. This
    /// bounded timing schedule precedes the charge loop at BEEC;
    /// the tongue's separate BE56-BE7D program has another owner.
    /// </summary>
    internal const ushort NearSpikeWallCharge = 0xbe7e;
    /// <summary>
    /// <c>InstList_CrocomireTongue_NearSpikeWallCharge_1</c> at
    /// $A4:BEEC-$BF3B. Twelve pinned-ROM durations are all $0003,
    /// each followed by a live spritemap operand. Authored callbacks
    /// shake, move left with dust, and handle the spike wall before
    /// Fight AI $86A6; goto $80ED at BF38 targets BEEC at BF3A.
    /// Retain callback order and possible wall redirect; the
    /// constant duration rule alone does not determine those effects.
    /// </summary>
    internal const ushort NearSpikeWallChargeLoop = 0xbeec;
    /// <summary>
    /// <c>InstList_Crocomire_BackOffFromSpikeWall</c> at
    /// $A4:BF3C-$BF61. Five pinned-ROM $0008 durations each precede
    /// a live spritemap operand. Authored callbacks move right with
    /// big dust, move plainly twice, move with dust, shake and move
    /// plainly, then call Fight AI $86A6. Goto $80ED at BF5E targets
    /// BF3C at BF60. Retain these native callback identities and
    /// order; they differ from SteppingBack despite the shared
    /// spritemap progression. Melting programs begin at BF64.
    /// </summary>
    internal const ushort BackOffFromSpikeWall = 0xbf3c;
    /// <summary>
    /// <c>InstList_Crocomire_Melting1_TopRow</c> at $A4:BF64.
    /// First melt pass, row index 0 of 0..3: duration $7FFF,
    /// live spritemap operand at BF66, then goto $80ED at BF68
    /// targets BF64 at BF6A. Across both passes, every row uses
    /// $7FFF; only row index 0 loops, while indices 1..3 Sleep.
    /// </summary>
    internal const ushort MeltingOneTopRow = 0xbf64;
    /// <summary><c>InstList_Crocomire_Melting1_Top2Rows</c> at $A4:BF6C:
    /// $7FFF, live operand BF6E, then Sleep $812F at BF70.</summary>
    internal const ushort MeltingOneTopTwoRows = 0xbf6c;
    /// <summary><c>InstList_Crocomire_Melting1_Top3Rows</c> at $A4:BF72:
    /// $7FFF, live operand BF74, then Sleep $812F at BF76.</summary>
    internal const ushort MeltingOneTopThreeRows = 0xbf72;
    /// <summary><c>InstList_Crocomire_Melting1_Top4Rows</c> at $A4:BF78:
    /// $7FFF, live operand BF7A, then Sleep $812F at BF7C.</summary>
    internal const ushort MeltingOneTopFourRows = 0xbf78;
    /// <summary>
    /// <c>InstList_Crocomire_Melting2_TopRow</c> at $A4:BF7E.
    /// Second melt pass, row index 0: duration $7FFF, live operand
    /// BF80, then goto $80ED at BF82 targets BF7E at BF84.
    /// Its row indices 1..3 use the same duration and Sleep instead.
    /// </summary>
    internal const ushort MeltingTwoTopRow = 0xbf7e;
    /// <summary><c>InstList_Crocomire_Melting2_Top2Rows</c> at $A4:BF86:
    /// $7FFF, live operand BF88, then Sleep $812F at BF8A.</summary>
    internal const ushort MeltingTwoTopTwoRows = 0xbf86;
    /// <summary><c>InstList_Crocomire_Melting2_Top3Rows</c> at $A4:BF8C:
    /// $7FFF, live operand BF8E, then Sleep $812F at BF90.</summary>
    internal const ushort MeltingTwoTopThreeRows = 0xbf8c;
    /// <summary><c>InstList_Crocomire_Melting2_Top4Rows</c> at $A4:BF92:
    /// $7FFF, live operand BF94, then Sleep $812F at BF96.</summary>
    internal const ushort MeltingTwoTopFourRows = 0xbf92;
    /// <summary>
    /// <c>InstList_CrocomireTongue_BridgeCollapsed</c> at
    /// $A4:BFB0-$BFC3. Four pinned-ROM durations are all $0005,
    /// each preceding a live spritemap operand. Cry SFX $8CFB
    /// at BFB8 lies between the second and third holds; Sleep
    /// $812F at BFC2 ends the authored sequence. BFC4 begins
    /// spritemap data rather than another mechanics instruction.
    /// </summary>
    internal const ushort BridgeCollapsed = 0xbfb0;
    /// <summary>
    /// <c>InstList_CrocomireCorpse_Skeleton_Falling</c> at
    /// $A4:E14A-$E157. Three pinned-ROM $000A durations each
    /// precede a live spritemap operand; Sleep $812F at E156
    /// ends this three-pose fall. The falls-apart program starts
    /// at E158. The constant timing rule is bounded to three entries.
    /// </summary>
    internal const ushort SkeletonFalling = 0xe14a;
    /// <summary>
    /// <c>InstList_CrocomireCorpse_Skeleton_FallsApart_0</c> at
    /// $A4:E158-$E1C5. Its twenty pinned-ROM durations, in order,
    /// are $000A, four $0005, $000A/$0020/$0010/$000A,
    /// two $0009, two $0008, two $0007, three $0006, and
    /// two $0005. Each precedes a live spritemap operand.
    /// The first four dust callbacks have authored offsets
    /// -32, 0, -16, +16; skeleton-collapse SFX $8D13 follows.
    /// The later nine dust callbacks at E18E..E1BE use native opcode
    /// $9AAF+5*j and X offset 8*j for j=0..8. Their durations are
    /// 8,8,7,7,6,6,6,5,5. Explosion SFX $8D07 precedes the final
    /// five-frame hold. Retain the authored first callbacks and
    /// sound order; SkeletonStable starts at E1C6.
    /// </summary>
    internal const ushort SkeletonFallsApart = 0xe158;
    /// <summary>
    /// <c>InstList_CrocomireCorpse_Skeleton_1</c> at $A4:E1C6.
    /// The stable skeleton holds for pinned-ROM duration $7FFF,
    /// reads a live spritemap operand at E1C8, then Sleep $812F
    /// at E1CA. This is the first of two identical hold/Sleep
    /// mechanics entries with distinct presentation identities.
    /// </summary>
    internal const ushort SkeletonStable = 0xe1c6;
    /// <summary>
    /// <c>InstList_CrocomireCorpse_Skeleton_Dead</c> at $A4:E1CC.
    /// The dead skeleton also holds for $7FFF, reads a live
    /// spritemap operand at E1CE, then Sleep $812F at E1D0.
    /// The bounded two-entry mechanics rule ends before the
    /// flowing-down-river program at E1D2.
    /// </summary>
    internal const ushort Dead = 0xe1cc;
    /// <summary>
    /// <c>InstList_CrocomireCorpse_Skeleton_FlowingDownTheRiver</c>
    /// at $A4:E1D2-$E1FD. Nine pinned-ROM durations are $0004,
    /// followed by one $0014 duration; each has a live spritemap
    /// operand. Goto $80ED at E1FA targets E1D2 at E1FC.
    /// The exact timing rule is D(i)=4 for i=0..8 and D(9)=20;
    /// retain the authored loop boundary for the river animation.
    /// </summary>
    internal const ushort SkeletonFlowingDownRiver = 0xe1d2;

    /// <summary>First deliberately excluded unreferenced body program at $A4:BBAE.</summary>
    internal const ushort FirstExcludedUnreferencedProgram = 0xbbae;
    /// <summary>First independently owned Crocomire-tongue program at $A4:BE56.</summary>
    internal const ushort FirstTongueProgram = 0xbe56;

    /// <summary><c>Instruction_Crocomire_FightAI</c> at $A4:86A6.</summary>
    private const ushort FightAI = 0x86a6;
    /// <summary><c>Instruction_Crocomire_MaybeStartProjectileAttack</c> at $A4:8752.</summary>
    private const ushort MaybeStartProjectileAttack = 0x8752;
    /// <summary><c>Instruction_Crocomire_QueueCrySFX</c> at $A4:8CFB.</summary>
    private const ushort QueueCrySFX = 0x8cfb;
    /// <summary><c>Instruction_Crocomire_QueueBigExplosionSFX</c> at $A4:8D07.</summary>
    private const ushort QueueBigExplosionSFX = 0x8d07;
    /// <summary><c>Instruction_Crocomire_QueueSkeletonCollapseSFX</c> at $A4:8D13.</summary>
    private const ushort QueueSkeletonCollapseSFX = 0x8d13;
    /// <summary><c>Instruction_Crocomire_ShakeScreen</c> at $A4:8FC7.</summary>
    private const ushort ShakeScreen = 0x8fc7;
    /// <summary><c>Instruction_Crocomire_MoveLeft4Pixels</c> at $A4:8FDF.</summary>
    private const ushort MoveLeft4Pixels = 0x8fdf;
    /// <summary><c>Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud</c> at $A4:8FFA.</summary>
    private const ushort MoveLeft4PixelsSpawnBigDustCloud = 0x8ffa;
    /// <summary><c>Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud_dup</c> at $A4:8FFF.</summary>
    private const ushort MoveLeft4PixelsSpawnBigDustCloudDup = 0x8fff;
    /// <summary><c>Instruction_Crocomire_MoveLeft_SpawnCloud_HandleSpikeWall</c> at $A4:901D.</summary>
    private const ushort MoveLeftSpawnCloudHandleSpikeWall = 0x901d;
    /// <summary><c>Instruction_Crocomire_MoveRight4PixelsIfOnScreen</c> at $A4:905B.</summary>
    private const ushort MoveRight4PixelsIfOnScreen = 0x905b;
    /// <summary><c>Instruction_Crocomire_MoveRight4Pixels</c> at $A4:907F.</summary>
    private const ushort MoveRight4Pixels = 0x907f;
    /// <summary><c>Instruction_Crocomire_MoveRight4PixelsIfOnScreen_SpawnCloud</c> at $A4:908F.</summary>
    private const ushort MoveRight4PixelsIfOnScreenSpawnCloud = 0x908f;
    /// <summary><c>Instruction_Crocomire_MoveRight4Pixels_SpawnBigDustCloud</c> at $A4:9094.</summary>
    private const ushort MoveRight4PixelsSpawnBigDustCloud = 0x9094;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative20</c> at $A4:9A9B.</summary>
    private const ushort SpawnBigDustCloudProjectileNegative20 = 0x9a9b;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_0</c> at $A4:9AA0.</summary>
    private const ushort SpawnBigDustCloudProjectile0 = 0x9aa0;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative10</c> at $A4:9AA5.</summary>
    private const ushort SpawnBigDustCloudProjectileNegative10 = 0x9aa5;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_10</c> at $A4:9AAA.</summary>
    private const ushort SpawnBigDustCloudProjectile10 = 0x9aaa;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_0_dup</c> at $A4:9AAF.</summary>
    private const ushort SpawnBigDustCloudProjectile0Dup = 0x9aaf;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_8</c> at $A4:9AB4.</summary>
    private const ushort SpawnBigDustCloudProjectile8 = 0x9ab4;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_10_dup</c> at $A4:9AB9.</summary>
    private const ushort SpawnBigDustCloudProjectile10Dup = 0x9ab9;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_18</c> at $A4:9ABE.</summary>
    private const ushort SpawnBigDustCloudProjectile18 = 0x9abe;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_20</c> at $A4:9AC3.</summary>
    private const ushort SpawnBigDustCloudProjectile20 = 0x9ac3;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_28</c> at $A4:9AC8.</summary>
    private const ushort SpawnBigDustCloudProjectile28 = 0x9ac8;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_30</c> at $A4:9ACD.</summary>
    private const ushort SpawnBigDustCloudProjectile30 = 0x9acd;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_38</c> at $A4:9AD2.</summary>
    private const ushort SpawnBigDustCloudProjectile38 = 0x9ad2;
    /// <summary><c>Instruction_Crocomire_SpawnBigDustCloudProjectile_40</c> at $A4:9AD7.</summary>
    private const ushort SpawnBigDustCloudProjectile40 = 0x9ad7;

    /// <summary>Native program bank $A4.</summary>
    internal const byte Bank = 0xa4;
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xbade),
        Entry(Initial),
        Frame(1),
        Op(FightAI),
        Op(CommonEnemyInstructionCodes.Goto, Initial),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(UnusedChargeForwardOneStep),
        Frame(8),
        Op(ShakeScreen),
        Op(MoveLeft4PixelsSpawnBigDustCloud),
        Frame(8),
        Op(MoveLeft4Pixels),
        Frame(8),
        Op(MoveLeft4Pixels),
        Frame(8),
        Op(MoveLeft4PixelsSpawnBigDustCloud),
        Frame(8),
        Op(MoveLeft4Pixels),
        Frame(8),
        Op(MoveLeft4Pixels),
        Frame(8),
        Op(ShakeScreen),
        Op(MoveLeft4PixelsSpawnBigDustCloud),
        Frame(8),
        Op(MoveLeft4Pixels),
        Frame(8),
        Op(MoveLeft4Pixels),
        Frame(8),
        Op(MoveLeft4PixelsSpawnBigDustCloud),
        Frame(8),
        Op(MoveLeft4Pixels),
        Frame(8),
        Op(FightAI),
        Entry(ProjectileAttack),
        Op(SpawnBigDustCloudProjectile10),
        Frame(5),
        Frame(5),
        Op(SpawnBigDustCloudProjectile0),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(QueueBigExplosionSFX),
        Op(SpawnBigDustCloudProjectileNegative20),
        Frame(5),
        Frame(5),
        Op(SpawnBigDustCloudProjectileNegative10),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(QueueBigExplosionSFX),
        Op(SpawnBigDustCloudProjectile0),
        Frame(5),
        Frame(5),
        Op(SpawnBigDustCloudProjectile28),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(QueueBigExplosionSFX),
        Op(SpawnBigDustCloudProjectileNegative10),
        Frame(5),
        Op(QueueCrySFX),
        Frame(5),
        Frame(5),
        Entry(ProjectileAttackLoop),
        Op(FightAI),
        Frame(8),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Op(CommonEnemyInstructionCodes.Goto, ProjectileAttackLoop),
        Origin(0xbbca),
        Entry(StepForwardAfterDelay),
        Frame(180),
        Entry(StepForward),
        Op(MaybeStartProjectileAttack),
        Frame(5),
        Op(FightAI),
        Frame(5),
        Op(QueueCrySFX),
        Op(FightAI),
        Frame(5),
        Op(FightAI),
        Frame(16),
        Op(MoveLeft4PixelsSpawnBigDustCloud),
        Op(FightAI),
        Frame(4),
        Op(MoveLeft4Pixels),
        Op(FightAI),
        Frame(4),
        Op(MoveLeft4Pixels),
        Op(FightAI),
        Frame(4),
        Op(ShakeScreen),
        Op(MoveLeft4PixelsSpawnBigDustCloud),
        Op(FightAI),
        Frame(4),
        Op(MoveLeft4Pixels),
        Op(FightAI),
        Frame(4),
        Op(MoveLeft4Pixels),
        Op(FightAI),
        Frame(4),
        Op(MoveLeft4PixelsSpawnBigDustCloud),
        Op(FightAI),
        Frame(4),
        Op(MoveLeft4Pixels),
        Op(FightAI),
        Frame(4),
        Op(FightAI),
        Op(CommonEnemyInstructionCodes.Goto, StepForward),
        Entry(StepBack),
        Frame(2),
        Entry(SteppingBack),
        Frame(8),
        Op(MoveRight4PixelsIfOnScreenSpawnCloud),
        Frame(8),
        Op(MoveRight4PixelsIfOnScreen),
        Frame(8),
        Op(MoveRight4PixelsIfOnScreen),
        Frame(8),
        Op(MoveRight4PixelsIfOnScreenSpawnCloud),
        Frame(8),
        Op(ShakeScreen),
        Op(MoveRight4PixelsIfOnScreen),
        Op(FightAI),
        Entry(WaitForFirstSecondDamage),
        Frame(34),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(16),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(16),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Op(FightAI),
        Entry(MovingClaws),
        Op(SpawnBigDustCloudProjectile10),
        Frame(5),
        Frame(5),
        Op(SpawnBigDustCloudProjectile0),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(QueueBigExplosionSFX),
        Op(SpawnBigDustCloudProjectileNegative20),
        Frame(5),
        Frame(5),
        Op(SpawnBigDustCloudProjectileNegative10),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(QueueBigExplosionSFX),
        Op(SpawnBigDustCloudProjectile0),
        Frame(5),
        Frame(5),
        Op(SpawnBigDustCloudProjectile28),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(QueueBigExplosionSFX),
        Op(SpawnBigDustCloudProjectileNegative10),
        Op(FightAI),
        Entry(Roar),
        Frame(48),
        Op(QueueCrySFX),
        Frame(5),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(32),
        Op(FightAI),
        Frame(1),
        Op(FightAI),
        Entry(RoarCloseMouth),
        Frame(32),
        Op(FightAI),
        Frame(5),
        Op(FightAI),
        Frame(8),
        Frame(2),
        Entry(RoarCloseMouthLoop),
        Frame(1),
        Op(FightAI),
        Frame(1),
        Op(FightAI),
        Entry(PowerBombReactionMouthFullyOpen),
        Frame(2),
        Entry(PowerBombReactionMouthPartiallyOpen),
        Frame(2),
        Entry(PowerBombReactionMouthNotOpen),
        Op(SpawnBigDustCloudProjectile10),
        Frame(5),
        Frame(5),
        Op(SpawnBigDustCloudProjectile0),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(QueueBigExplosionSFX),
        Op(SpawnBigDustCloudProjectileNegative20),
        Frame(5),
        Frame(5),
        Op(SpawnBigDustCloudProjectileNegative10),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(QueueBigExplosionSFX),
        Op(SpawnBigDustCloudProjectile0),
        Frame(5),
        Frame(5),
        Op(SpawnBigDustCloudProjectile28),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(QueueBigExplosionSFX),
        Op(SpawnBigDustCloudProjectileNegative10),
        Entry(PowerBombReactionMouthNotOpenLoop),
        Frame(4),
        Op(ShakeScreen),
        Op(MoveLeft4PixelsSpawnBigDustCloud),
        Frame(4),
        Op(MoveLeft4Pixels),
        Frame(4),
        Op(MoveLeft4Pixels),
        Frame(4),
        Op(MoveLeft4PixelsSpawnBigDustCloud),
        Frame(4),
        Op(MoveLeft4Pixels),
        Frame(4),
        Op(MoveLeft4Pixels),
        Frame(4),
        Op(ShakeScreen),
        Op(MoveLeft4PixelsSpawnBigDustCloud),
        Frame(4),
        Op(MoveLeft4Pixels),
        Frame(4),
        Op(MoveLeft4Pixels),
        Frame(4),
        Op(MoveLeft4PixelsSpawnBigDustCloud),
        Frame(4),
        Op(MoveLeft4Pixels),
        Frame(4),
        Op(FightAI),
        Op(CommonEnemyInstructionCodes.Goto, PowerBombReactionMouthNotOpenLoop),
        Origin(0xbe7e),
        Entry(NearSpikeWallCharge),
        Frame(5),
        Op(QueueCrySFX),
        Op(FightAI),
        Frame(5),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Frame(5),
        Op(FightAI),
        Frame(8),
        Op(FightAI),
        Frame(2),
        Op(FightAI),
        Entry(NearSpikeWallChargeLoop),
        Frame(3),
        Op(ShakeScreen),
        Op(MoveLeft4PixelsSpawnBigDustCloudDup),
        Frame(3),
        Op(MoveLeftSpawnCloudHandleSpikeWall),
        Frame(3),
        Op(MoveLeftSpawnCloudHandleSpikeWall),
        Frame(3),
        Op(MoveLeft4PixelsSpawnBigDustCloudDup),
        Frame(3),
        Op(MoveLeftSpawnCloudHandleSpikeWall),
        Frame(3),
        Op(MoveLeftSpawnCloudHandleSpikeWall),
        Frame(3),
        Op(ShakeScreen),
        Op(MoveLeft4PixelsSpawnBigDustCloudDup),
        Frame(3),
        Op(MoveLeftSpawnCloudHandleSpikeWall),
        Frame(3),
        Op(MoveLeftSpawnCloudHandleSpikeWall),
        Frame(3),
        Op(MoveLeft4PixelsSpawnBigDustCloudDup),
        Frame(3),
        Op(MoveLeftSpawnCloudHandleSpikeWall),
        Frame(3),
        Op(FightAI),
        Op(CommonEnemyInstructionCodes.Goto, NearSpikeWallChargeLoop),
        Entry(BackOffFromSpikeWall),
        Frame(8),
        Op(MoveRight4PixelsSpawnBigDustCloud),
        Frame(8),
        Op(MoveRight4Pixels),
        Frame(8),
        Op(MoveRight4Pixels),
        Frame(8),
        Op(MoveRight4PixelsSpawnBigDustCloud),
        Frame(8),
        Op(ShakeScreen),
        Op(MoveRight4Pixels),
        Op(FightAI),
        Op(CommonEnemyInstructionCodes.Goto, BackOffFromSpikeWall),
        Skip(2),
        Entry(MeltingOneTopRow),
        Frame(IndefiniteDuration),
        Op(CommonEnemyInstructionCodes.Goto, MeltingOneTopRow),
        Entry(MeltingOneTopTwoRows),
        Frame(IndefiniteDuration),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(MeltingOneTopThreeRows),
        Frame(IndefiniteDuration),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(MeltingOneTopFourRows),
        Frame(IndefiniteDuration),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(MeltingTwoTopRow),
        Frame(IndefiniteDuration),
        Op(CommonEnemyInstructionCodes.Goto, MeltingTwoTopRow),
        Entry(MeltingTwoTopTwoRows),
        Frame(IndefiniteDuration),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(MeltingTwoTopThreeRows),
        Frame(IndefiniteDuration),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(MeltingTwoTopFourRows),
        Frame(IndefiniteDuration),
        Op(CommonEnemyInstructionCodes.Sleep),
        Origin(0xbfb0),
        Entry(BridgeCollapsed),
        Frame(5),
        Frame(5),
        Op(QueueCrySFX),
        Frame(5),
        Frame(5),
        Op(CommonEnemyInstructionCodes.Sleep),
        Origin(0xe14a),
        Entry(SkeletonFalling),
        Frame(10),
        Frame(10),
        Frame(10),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(SkeletonFallsApart),
        Frame(10),
        Op(SpawnBigDustCloudProjectileNegative20),
        Frame(5),
        Op(SpawnBigDustCloudProjectile0),
        Frame(5),
        Op(SpawnBigDustCloudProjectileNegative10),
        Frame(5),
        Op(SpawnBigDustCloudProjectile10),
        Frame(5),
        Frame(10),
        Frame(32),
        Frame(16),
        Op(QueueSkeletonCollapseSFX),
        Frame(10),
        Frame(9),
        Frame(9),
        Op(SpawnBigDustCloudProjectile0Dup),
        Frame(8),
        Op(SpawnBigDustCloudProjectile8),
        Frame(8),
        Op(SpawnBigDustCloudProjectile10Dup),
        Frame(7),
        Op(SpawnBigDustCloudProjectile18),
        Frame(7),
        Op(SpawnBigDustCloudProjectile20),
        Frame(6),
        Op(SpawnBigDustCloudProjectile28),
        Frame(6),
        Op(SpawnBigDustCloudProjectile30),
        Frame(6),
        Op(SpawnBigDustCloudProjectile38),
        Frame(5),
        Op(SpawnBigDustCloudProjectile40),
        Op(QueueBigExplosionSFX),
        Frame(5),
        Entry(SkeletonStable),
        Frame(IndefiniteDuration),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(Dead),
        Frame(IndefiniteDuration),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(SkeletonFlowingDownRiver),
        Frame(4),
        Frame(4),
        Frame(4),
        Frame(4),
        Frame(4),
        Frame(4),
        Frame(4),
        Frame(4),
        Frame(4),
        Frame(20),
        Op(CommonEnemyInstructionCodes.Goto, SkeletonFlowingDownRiver));

    public static int MechanicsWordCount => Layout.MechanicsWordCount;
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Crocomire instruction mechanics pointer $A4:{address:X4} is not compiled.");

    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
