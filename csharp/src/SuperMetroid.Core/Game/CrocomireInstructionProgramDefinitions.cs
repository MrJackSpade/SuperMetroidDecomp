namespace SuperMetroid.Core.Game;

/// <summary>One compiled Crocomire-body mechanics word at its bank-$A4 address.</summary>
internal readonly record struct CrocomireInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Crocomire's body, melting and skeleton instruction programs.
/// Interleaved extended-spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class CrocomireInstructionProgramDefinitions
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
    /// <summary><c>InstList_CrocomireTongue_NearSpikeWallCharge_0</c> at $A4:BE7E.</summary>
    internal const ushort NearSpikeWallCharge = 0xbe7e;
    /// <summary><c>InstList_CrocomireTongue_NearSpikeWallCharge_1</c> at $A4:BEEC.</summary>
    internal const ushort NearSpikeWallChargeLoop = 0xbeec;
    /// <summary><c>InstList_Crocomire_BackOffFromSpikeWall</c> at $A4:BF3C.</summary>
    internal const ushort BackOffFromSpikeWall = 0xbf3c;
    /// <summary><c>InstList_Crocomire_Melting1_TopRow</c> at $A4:BF64.</summary>
    internal const ushort MeltingOneTopRow = 0xbf64;
    /// <summary><c>InstList_Crocomire_Melting1_Top2Rows</c> at $A4:BF6C.</summary>
    internal const ushort MeltingOneTopTwoRows = 0xbf6c;
    /// <summary><c>InstList_Crocomire_Melting1_Top3Rows</c> at $A4:BF72.</summary>
    internal const ushort MeltingOneTopThreeRows = 0xbf72;
    /// <summary><c>InstList_Crocomire_Melting1_Top4Rows</c> at $A4:BF78.</summary>
    internal const ushort MeltingOneTopFourRows = 0xbf78;
    /// <summary><c>InstList_Crocomire_Melting2_TopRow</c> at $A4:BF7E.</summary>
    internal const ushort MeltingTwoTopRow = 0xbf7e;
    /// <summary><c>InstList_Crocomire_Melting2_Top2Rows</c> at $A4:BF86.</summary>
    internal const ushort MeltingTwoTopTwoRows = 0xbf86;
    /// <summary><c>InstList_Crocomire_Melting2_Top3Rows</c> at $A4:BF8C.</summary>
    internal const ushort MeltingTwoTopThreeRows = 0xbf8c;
    /// <summary><c>InstList_Crocomire_Melting2_Top4Rows</c> at $A4:BF92.</summary>
    internal const ushort MeltingTwoTopFourRows = 0xbf92;
    /// <summary><c>InstList_CrocomireTongue_BridgeCollapsed</c> at $A4:BFB0.</summary>
    internal const ushort BridgeCollapsed = 0xbfb0;
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_Falling</c> at $A4:E14A.</summary>
    internal const ushort SkeletonFalling = 0xe14a;
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_FallsApart_0</c> at $A4:E158.</summary>
    internal const ushort SkeletonFallsApart = 0xe158;
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_1</c> at $A4:E1C6.</summary>
    internal const ushort SkeletonStable = 0xe1c6;
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_Dead</c> at $A4:E1CC.</summary>
    internal const ushort Dead = 0xe1cc;
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_FlowingDownTheRiver</c> at $A4:E1D2.</summary>
    internal const ushort SkeletonFlowingDownRiver = 0xe1d2;

    /// <summary>First deliberately excluded unreferenced body program at $A4:BBAE.</summary>
    internal const ushort FirstExcludedUnreferencedProgram = 0xbbae;
    /// <summary>First independently owned Crocomire-tongue program at $A4:BE56.</summary>
    internal const ushort FirstTongueProgram = 0xbe56;

    private static readonly CrocomireInstructionMechanicsWord[] Words =
    [
        new(0xbade, 0x0001),
        new(0xbae2, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbae4, CommonEnemyInstructionCodes.Goto),
        new(0xbae6, CrocomireInstructionProgramDefinitions.Initial),
        new(0xbae8, CommonEnemyInstructionCodes.Sleep),
        new(0xbaea, 0x0008),
        new(0xbaee, CrocomireCodePointers.Instruction_Crocomire_ShakeScreen),
        new(0xbaf0, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud),
        new(0xbaf2, 0x0008),
        new(0xbaf6, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbaf8, 0x0008),
        new(0xbafc, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbafe, 0x0008),
        new(0xbb02, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud),
        new(0xbb04, 0x0008),
        new(0xbb08, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbb0a, 0x0008),
        new(0xbb0e, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbb10, 0x0008),
        new(0xbb14, CrocomireCodePointers.Instruction_Crocomire_ShakeScreen),
        new(0xbb16, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud),
        new(0xbb18, 0x0008),
        new(0xbb1c, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbb1e, 0x0008),
        new(0xbb22, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbb24, 0x0008),
        new(0xbb28, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud),
        new(0xbb2a, 0x0008),
        new(0xbb2e, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbb30, 0x0008),
        new(0xbb34, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbb36, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_10),
        new(0xbb38, 0x0005),
        new(0xbb3c, 0x0005),
        new(0xbb40, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_0),
        new(0xbb42, 0x0005),
        new(0xbb46, 0x0005),
        new(0xbb4a, 0x0005),
        new(0xbb4e, CrocomireCodePointers.Instruction_Crocomire_QueueBigExplosionSFX),
        new(0xbb50, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative20),
        new(0xbb52, 0x0005),
        new(0xbb56, 0x0005),
        new(0xbb5a, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative10),
        new(0xbb5c, 0x0005),
        new(0xbb60, 0x0005),
        new(0xbb64, 0x0005),
        new(0xbb68, CrocomireCodePointers.Instruction_Crocomire_QueueBigExplosionSFX),
        new(0xbb6a, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_0),
        new(0xbb6c, 0x0005),
        new(0xbb70, 0x0005),
        new(0xbb74, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_28),
        new(0xbb76, 0x0005),
        new(0xbb7a, 0x0005),
        new(0xbb7e, 0x0005),
        new(0xbb82, CrocomireCodePointers.Instruction_Crocomire_QueueBigExplosionSFX),
        new(0xbb84, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative10),
        new(0xbb86, 0x0005),
        new(0xbb8a, CrocomireCodePointers.Instruction_Crocomire_QueueCrySFX),
        new(0xbb8c, 0x0005),
        new(0xbb90, 0x0005),
        new(0xbb94, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbb96, 0x0008),
        new(0xbb9a, 0x0007),
        new(0xbb9e, 0x0007),
        new(0xbba2, 0x0007),
        new(0xbba6, 0x0007),
        new(0xbbaa, CommonEnemyInstructionCodes.Goto),
        new(0xbbac, CrocomireInstructionProgramDefinitions.ProjectileAttackLoop),
        new(0xbbca, 0x00b4),
        new(0xbbce, CrocomireCodePointers.Instruction_Crocomire_MaybeStartProjectileAttack),
        new(0xbbd0, 0x0005),
        new(0xbbd4, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbbd6, 0x0005),
        new(0xbbda, CrocomireCodePointers.Instruction_Crocomire_QueueCrySFX),
        new(0xbbdc, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbbde, 0x0005),
        new(0xbbe2, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbbe4, 0x0010),
        new(0xbbe8, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud),
        new(0xbbea, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbbec, 0x0004),
        new(0xbbf0, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbbf2, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbbf4, 0x0004),
        new(0xbbf8, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbbfa, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbbfc, 0x0004),
        new(0xbc00, CrocomireCodePointers.Instruction_Crocomire_ShakeScreen),
        new(0xbc02, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud),
        new(0xbc04, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbc06, 0x0004),
        new(0xbc0a, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbc0c, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbc0e, 0x0004),
        new(0xbc12, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbc14, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbc16, 0x0004),
        new(0xbc1a, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud),
        new(0xbc1c, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbc1e, 0x0004),
        new(0xbc22, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbc24, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbc26, 0x0004),
        new(0xbc2a, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbc2c, CommonEnemyInstructionCodes.Goto),
        new(0xbc2e, CrocomireInstructionProgramDefinitions.StepForward),
        new(0xbc30, 0x0002),
        new(0xbc34, 0x0008),
        new(0xbc38, CrocomireCodePointers.Instruction_Crocomire_MoveRight4PixelsIfOnScreen_SpawnCloud),
        new(0xbc3a, 0x0008),
        new(0xbc3e, CrocomireCodePointers.Instruction_Crocomire_MoveRight4PixelsIfOnScreen),
        new(0xbc40, 0x0008),
        new(0xbc44, CrocomireCodePointers.Instruction_Crocomire_MoveRight4PixelsIfOnScreen),
        new(0xbc46, 0x0008),
        new(0xbc4a, CrocomireCodePointers.Instruction_Crocomire_MoveRight4PixelsIfOnScreen_SpawnCloud),
        new(0xbc4c, 0x0008),
        new(0xbc50, CrocomireCodePointers.Instruction_Crocomire_ShakeScreen),
        new(0xbc52, CrocomireCodePointers.Instruction_Crocomire_MoveRight4PixelsIfOnScreen),
        new(0xbc54, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbc56, 0x0022),
        new(0xbc5a, 0x0002),
        new(0xbc5e, 0x0002),
        new(0xbc62, 0x0002),
        new(0xbc66, 0x0002),
        new(0xbc6a, 0x0002),
        new(0xbc6e, 0x0002),
        new(0xbc72, 0x0010),
        new(0xbc76, 0x0001),
        new(0xbc7a, 0x0001),
        new(0xbc7e, 0x0001),
        new(0xbc82, 0x0001),
        new(0xbc86, 0x0001),
        new(0xbc8a, 0x0010),
        new(0xbc8e, 0x0001),
        new(0xbc92, 0x0001),
        new(0xbc96, 0x0001),
        new(0xbc9a, 0x0001),
        new(0xbc9e, 0x0001),
        new(0xbca2, 0x0001),
        new(0xbca6, 0x0001),
        new(0xbcaa, 0x0001),
        new(0xbcae, 0x0001),
        new(0xbcb2, 0x0001),
        new(0xbcb6, 0x0001),
        new(0xbcba, 0x0001),
        new(0xbcbe, 0x0001),
        new(0xbcc2, 0x0001),
        new(0xbcc6, 0x0001),
        new(0xbcca, 0x0001),
        new(0xbcce, 0x0001),
        new(0xbcd2, 0x0001),
        new(0xbcd6, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbcd8, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_10),
        new(0xbcda, 0x0005),
        new(0xbcde, 0x0005),
        new(0xbce2, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_0),
        new(0xbce4, 0x0005),
        new(0xbce8, 0x0005),
        new(0xbcec, 0x0005),
        new(0xbcf0, CrocomireCodePointers.Instruction_Crocomire_QueueBigExplosionSFX),
        new(0xbcf2, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative20),
        new(0xbcf4, 0x0005),
        new(0xbcf8, 0x0005),
        new(0xbcfc, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative10),
        new(0xbcfe, 0x0005),
        new(0xbd02, 0x0005),
        new(0xbd06, 0x0005),
        new(0xbd0a, CrocomireCodePointers.Instruction_Crocomire_QueueBigExplosionSFX),
        new(0xbd0c, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_0),
        new(0xbd0e, 0x0005),
        new(0xbd12, 0x0005),
        new(0xbd16, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_28),
        new(0xbd18, 0x0005),
        new(0xbd1c, 0x0005),
        new(0xbd20, 0x0005),
        new(0xbd24, CrocomireCodePointers.Instruction_Crocomire_QueueBigExplosionSFX),
        new(0xbd26, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative10),
        new(0xbd28, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd2a, 0x0030),
        new(0xbd2e, CrocomireCodePointers.Instruction_Crocomire_QueueCrySFX),
        new(0xbd30, 0x0005),
        new(0xbd34, 0x0002),
        new(0xbd38, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd3a, 0x0002),
        new(0xbd3e, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd40, 0x0002),
        new(0xbd44, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd46, 0x0002),
        new(0xbd4a, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd4c, 0x0002),
        new(0xbd50, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd52, 0x0002),
        new(0xbd56, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd58, 0x0002),
        new(0xbd5c, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd5e, 0x0002),
        new(0xbd62, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd64, 0x0002),
        new(0xbd68, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd6a, 0x0002),
        new(0xbd6e, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd70, 0x0002),
        new(0xbd74, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd76, 0x0002),
        new(0xbd7a, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd7c, 0x0002),
        new(0xbd80, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd82, 0x0020),
        new(0xbd86, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd88, 0x0001),
        new(0xbd8c, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd8e, 0x0020),
        new(0xbd92, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd94, 0x0005),
        new(0xbd98, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbd9a, 0x0008),
        new(0xbd9e, 0x0002),
        new(0xbda2, 0x0001),
        new(0xbda6, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbda8, 0x0001),
        new(0xbdac, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbdae, 0x0002),
        new(0xbdb2, 0x0002),
        new(0xbdb6, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_10),
        new(0xbdb8, 0x0005),
        new(0xbdbc, 0x0005),
        new(0xbdc0, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_0),
        new(0xbdc2, 0x0005),
        new(0xbdc6, 0x0005),
        new(0xbdca, 0x0005),
        new(0xbdce, CrocomireCodePointers.Instruction_Crocomire_QueueBigExplosionSFX),
        new(0xbdd0, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative20),
        new(0xbdd2, 0x0005),
        new(0xbdd6, 0x0005),
        new(0xbdda, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative10),
        new(0xbddc, 0x0005),
        new(0xbde0, 0x0005),
        new(0xbde4, 0x0005),
        new(0xbde8, CrocomireCodePointers.Instruction_Crocomire_QueueBigExplosionSFX),
        new(0xbdea, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_0),
        new(0xbdec, 0x0005),
        new(0xbdf0, 0x0005),
        new(0xbdf4, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_28),
        new(0xbdf6, 0x0005),
        new(0xbdfa, 0x0005),
        new(0xbdfe, 0x0005),
        new(0xbe02, CrocomireCodePointers.Instruction_Crocomire_QueueBigExplosionSFX),
        new(0xbe04, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative10),
        new(0xbe06, 0x0004),
        new(0xbe0a, CrocomireCodePointers.Instruction_Crocomire_ShakeScreen),
        new(0xbe0c, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud),
        new(0xbe0e, 0x0004),
        new(0xbe12, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbe14, 0x0004),
        new(0xbe18, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbe1a, 0x0004),
        new(0xbe1e, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud),
        new(0xbe20, 0x0004),
        new(0xbe24, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbe26, 0x0004),
        new(0xbe2a, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbe2c, 0x0004),
        new(0xbe30, CrocomireCodePointers.Instruction_Crocomire_ShakeScreen),
        new(0xbe32, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud),
        new(0xbe34, 0x0004),
        new(0xbe38, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbe3a, 0x0004),
        new(0xbe3e, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbe40, 0x0004),
        new(0xbe44, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud),
        new(0xbe46, 0x0004),
        new(0xbe4a, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels),
        new(0xbe4c, 0x0004),
        new(0xbe50, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbe52, CommonEnemyInstructionCodes.Goto),
        new(0xbe54, CrocomireInstructionProgramDefinitions.PowerBombReactionMouthNotOpenLoop),
        new(0xbe7e, 0x0005),
        new(0xbe82, CrocomireCodePointers.Instruction_Crocomire_QueueCrySFX),
        new(0xbe84, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbe86, 0x0005),
        new(0xbe8a, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbe8c, 0x0002),
        new(0xbe90, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbe92, 0x0002),
        new(0xbe96, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbe98, 0x0002),
        new(0xbe9c, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbe9e, 0x0002),
        new(0xbea2, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbea4, 0x0002),
        new(0xbea8, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbeaa, 0x0002),
        new(0xbeae, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbeb0, 0x0002),
        new(0xbeb4, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbeb6, 0x0002),
        new(0xbeba, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbebc, 0x0002),
        new(0xbec0, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbec2, 0x0002),
        new(0xbec6, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbec8, 0x0002),
        new(0xbecc, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbece, 0x0002),
        new(0xbed2, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbed4, 0x0002),
        new(0xbed8, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbeda, 0x0005),
        new(0xbede, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbee0, 0x0008),
        new(0xbee4, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbee6, 0x0002),
        new(0xbeea, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbeec, 0x0003),
        new(0xbef0, CrocomireCodePointers.Instruction_Crocomire_ShakeScreen),
        new(0xbef2, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud_dup),
        new(0xbef4, 0x0003),
        new(0xbef8, CrocomireCodePointers.Instruction_Crocomire_MoveLeft_SpawnCloud_HandleSpikeWall),
        new(0xbefa, 0x0003),
        new(0xbefe, CrocomireCodePointers.Instruction_Crocomire_MoveLeft_SpawnCloud_HandleSpikeWall),
        new(0xbf00, 0x0003),
        new(0xbf04, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud_dup),
        new(0xbf06, 0x0003),
        new(0xbf0a, CrocomireCodePointers.Instruction_Crocomire_MoveLeft_SpawnCloud_HandleSpikeWall),
        new(0xbf0c, 0x0003),
        new(0xbf10, CrocomireCodePointers.Instruction_Crocomire_MoveLeft_SpawnCloud_HandleSpikeWall),
        new(0xbf12, 0x0003),
        new(0xbf16, CrocomireCodePointers.Instruction_Crocomire_ShakeScreen),
        new(0xbf18, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud_dup),
        new(0xbf1a, 0x0003),
        new(0xbf1e, CrocomireCodePointers.Instruction_Crocomire_MoveLeft_SpawnCloud_HandleSpikeWall),
        new(0xbf20, 0x0003),
        new(0xbf24, CrocomireCodePointers.Instruction_Crocomire_MoveLeft_SpawnCloud_HandleSpikeWall),
        new(0xbf26, 0x0003),
        new(0xbf2a, CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud_dup),
        new(0xbf2c, 0x0003),
        new(0xbf30, CrocomireCodePointers.Instruction_Crocomire_MoveLeft_SpawnCloud_HandleSpikeWall),
        new(0xbf32, 0x0003),
        new(0xbf36, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbf38, CommonEnemyInstructionCodes.Goto),
        new(0xbf3a, CrocomireInstructionProgramDefinitions.NearSpikeWallChargeLoop),
        new(0xbf3c, 0x0008),
        new(0xbf40, CrocomireCodePointers.Instruction_Crocomire_MoveRight4Pixels_SpawnBigDustCloud),
        new(0xbf42, 0x0008),
        new(0xbf46, CrocomireCodePointers.Instruction_Crocomire_MoveRight4Pixels),
        new(0xbf48, 0x0008),
        new(0xbf4c, CrocomireCodePointers.Instruction_Crocomire_MoveRight4Pixels),
        new(0xbf4e, 0x0008),
        new(0xbf52, CrocomireCodePointers.Instruction_Crocomire_MoveRight4Pixels_SpawnBigDustCloud),
        new(0xbf54, 0x0008),
        new(0xbf58, CrocomireCodePointers.Instruction_Crocomire_ShakeScreen),
        new(0xbf5a, CrocomireCodePointers.Instruction_Crocomire_MoveRight4Pixels),
        new(0xbf5c, CrocomireCodePointers.Instruction_Crocomire_FightAI),
        new(0xbf5e, CommonEnemyInstructionCodes.Goto),
        new(0xbf60, CrocomireInstructionProgramDefinitions.BackOffFromSpikeWall),
        new(0xbf64, 0x7fff),
        new(0xbf68, CommonEnemyInstructionCodes.Goto),
        new(0xbf6a, CrocomireInstructionProgramDefinitions.MeltingOneTopRow),
        new(0xbf6c, 0x7fff),
        new(0xbf70, CommonEnemyInstructionCodes.Sleep),
        new(0xbf72, 0x7fff),
        new(0xbf76, CommonEnemyInstructionCodes.Sleep),
        new(0xbf78, 0x7fff),
        new(0xbf7c, CommonEnemyInstructionCodes.Sleep),
        new(0xbf7e, 0x7fff),
        new(0xbf82, CommonEnemyInstructionCodes.Goto),
        new(0xbf84, CrocomireInstructionProgramDefinitions.MeltingTwoTopRow),
        new(0xbf86, 0x7fff),
        new(0xbf8a, CommonEnemyInstructionCodes.Sleep),
        new(0xbf8c, 0x7fff),
        new(0xbf90, CommonEnemyInstructionCodes.Sleep),
        new(0xbf92, 0x7fff),
        new(0xbf96, CommonEnemyInstructionCodes.Sleep),
        new(0xbfb0, 0x0005),
        new(0xbfb4, 0x0005),
        new(0xbfb8, CrocomireCodePointers.Instruction_Crocomire_QueueCrySFX),
        new(0xbfba, 0x0005),
        new(0xbfbe, 0x0005),
        new(0xbfc2, CommonEnemyInstructionCodes.Sleep),
        new(0xe14a, 0x000a),
        new(0xe14e, 0x000a),
        new(0xe152, 0x000a),
        new(0xe156, CommonEnemyInstructionCodes.Sleep),
        new(0xe158, 0x000a),
        new(0xe15c, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative20),
        new(0xe15e, 0x0005),
        new(0xe162, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_0),
        new(0xe164, 0x0005),
        new(0xe168, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative10),
        new(0xe16a, 0x0005),
        new(0xe16e, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_10),
        new(0xe170, 0x0005),
        new(0xe174, 0x000a),
        new(0xe178, 0x0020),
        new(0xe17c, 0x0010),
        new(0xe180, CrocomireCodePointers.Instruction_Crocomire_QueueSkeletonCollapseSFX),
        new(0xe182, 0x000a),
        new(0xe186, 0x0009),
        new(0xe18a, 0x0009),
        new(0xe18e, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_0_dup),
        new(0xe190, 0x0008),
        new(0xe194, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_8),
        new(0xe196, 0x0008),
        new(0xe19a, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_10_dup),
        new(0xe19c, 0x0007),
        new(0xe1a0, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_18),
        new(0xe1a2, 0x0007),
        new(0xe1a6, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_20),
        new(0xe1a8, 0x0006),
        new(0xe1ac, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_28),
        new(0xe1ae, 0x0006),
        new(0xe1b2, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_30),
        new(0xe1b4, 0x0006),
        new(0xe1b8, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_38),
        new(0xe1ba, 0x0005),
        new(0xe1be, CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_40),
        new(0xe1c0, CrocomireCodePointers.Instruction_Crocomire_QueueBigExplosionSFX),
        new(0xe1c2, 0x0005),
        new(0xe1c6, 0x7fff),
        new(0xe1ca, CommonEnemyInstructionCodes.Sleep),
        new(0xe1cc, 0x7fff),
        new(0xe1d0, CommonEnemyInstructionCodes.Sleep),
        new(0xe1d2, 0x0004),
        new(0xe1d6, 0x0004),
        new(0xe1da, 0x0004),
        new(0xe1de, 0x0004),
        new(0xe1e2, 0x0004),
        new(0xe1e6, 0x0004),
        new(0xe1ea, 0x0004),
        new(0xe1ee, 0x0004),
        new(0xe1f2, 0x0004),
        new(0xe1f6, 0x0014),
        new(0xe1fa, CommonEnemyInstructionCodes.Goto),
        new(0xe1fc, CrocomireInstructionProgramDefinitions.SkeletonFlowingDownRiver),
    ];

    /// <summary>
    /// Entry 0 identifies the initial body's live spritemap operand at
    /// $A4:BAE0. Its pinned NTSC J/U v1.0 stock pointer is $C2EC, between
    /// duration $0001 at BADE and Fight AI $86A6 at BAE2. Retain this one
    /// authored presentation identity as a live cartridge read; it is not
    /// a second mechanics word or an indexed numeric progression.
    /// The following twelve entries are the unused charge-forward list's
    /// live spritemap operands at $A4:BAEC, BAF4, BAFA, BB00, BB06,
    /// BB0C, BB12, BB1A, BB20, BB26, BB2C, and BB32. Their pinned ROM
    /// values obey $BFC4 + $32*i exactly for indices 0 through 11,
    /// ending at $C1EA. This bounded stock progression describes the
    /// presentation data; the interpreter still reads each operand live.
    /// The next 23 entries are projectile-attack spritemap operands at
    /// $A4:BB3A-$BBA8. For zero-based indices 0..14, the pinned ROM
    /// value is $C47A + $32*(i modulo 5), repeating one five-pointer
    /// cycle three times. Indices 15..17 use $C574 + $3A*(i-15);
    /// loop indices 18..22 use $C95C + $3A*(i-18), ending at $CA44.
    /// These bounded stock identities do not replace live cartridge reads.
    /// The later step-forward-after-delay entry at $A4:BBCC is one live
    /// spritemap operand with pinned stock value $C6A4, between the
    /// $00B4 duration at BBCA and StepForward opcode $8752 at BBCE.
    /// Retain that one authored presentation identity.
    /// The next twelve StepForward operands run from $A4:BBD2 to BC28.
    /// For zero-based indices 0..2, stock pointer = $C574 + $3A*i;
    /// for 3..11, stock pointer = $C752 + $3A*(i-3), ending at $C922.
    /// The jump at index 3 is a different spritemap family. All twelve
    /// operand addresses remain live cartridge presentation reads.
    /// The single StepBack operand at $A4:BC32 has pinned stock value
    /// $C5AE, between its two-frame duration and the SteppingBack list.
    /// Retain this authored identity as a live presentation read.
    /// The next five SteppingBack operands at $A4:BC36, BC3C, BC42,
    /// BC48, and BC4E have pinned stock values $C1EA - $32*i for
    /// zero-based indices 0..4, ending at $C122. This bounded descent
    /// describes the stock spritemaps; the operands remain live reads.
    /// The 32 wait-for-damage operands at $A4:BC58-$BCD4 have stride
    /// four in instruction address. Their pinned stock pointer is
    /// $C2EC + $3A*T(i) for indices 0..31: T(i)=6-i for 0..6;
    /// for 7..31, r=(i-7) modulo 12 and T(i)=min(r,12-r).
    /// This includes an extra $C2EC hold at index 7 before the
    /// triangular cycle. The interpreter still reads all operands live.
    /// The fifteen MovingClaws operands at $A4:BCDC-$BD22 repeat
    /// $C47A + $32*(i modulo 5) for zero-based indices 0..14.
    /// They equal ProjectileAttack's first fifteen stock operands
    /// under instruction-address translation +$01A2. This three-cycle
    /// identity leaves all presentation words as live cartridge reads.
    /// The seventeen Roar operands at $A4:BD2C-$BD8A have pinned stock
    /// pointer $C574 + $3A*min(i,2) for indices 0..16: two advances,
    /// then $C5E8 through the rest of the list. This bounded saturation
    /// describes stock content; each operand remains a live ROM read.
    /// The six RoarCloseMouth operands at $A4:BD90, BD96, BD9C,
    /// BDA0, BDA4, and BDAA use pinned stock pointer
    /// $C5E8 - $3A*min(floor((i+1)/2),2) for indices 0..5.
    /// This yields one $C5E8, two $C5AE, and three $C574 values;
    /// each address remains a live presentation operand.
    /// The power-bomb fully-open and partially-open operands at
    /// $A4:BDB0 and BDB4 have pinned stock pointers $C5AE and
    /// $C574: $C5AE - $3A*i for indices 0..1. A fully-open entry
    /// traverses both; a partially-open entry begins at the second.
    /// Both remain live cartridge presentation reads.
    /// The 27 closed-mouth power-bomb operands have two bounded stock
    /// rules. Prefix indices 0..14 use $C47A + $32*(i modulo 5),
    /// the same three cycles as ProjectileAttack's first fifteen
    /// operands under +$0280 instruction-address translation.
    /// Loop indices 15..26 use $BFC4 + $32*(i-15), ending at $C1EA;
    /// this matches the unused charge-forward pointer progression.
    /// All 27 addresses remain live cartridge presentation reads.
    /// </summary>
    private static readonly ushort[] PresentationWords =
    [
        0xbae0, 0xbaec, 0xbaf4, 0xbafa, 0xbb00, 0xbb06, 0xbb0c, 0xbb12, 0xbb1a, 0xbb20,
        0xbb26, 0xbb2c, 0xbb32, 0xbb3a, 0xbb3e, 0xbb44, 0xbb48, 0xbb4c, 0xbb54, 0xbb58,
        0xbb5e, 0xbb62, 0xbb66, 0xbb6e, 0xbb72, 0xbb78, 0xbb7c, 0xbb80, 0xbb88, 0xbb8e,
        0xbb92, 0xbb98, 0xbb9c, 0xbba0, 0xbba4, 0xbba8, 0xbbcc, 0xbbd2, 0xbbd8, 0xbbe0,
        0xbbe6, 0xbbee, 0xbbf6, 0xbbfe, 0xbc08, 0xbc10, 0xbc18, 0xbc20, 0xbc28, 0xbc32,
        0xbc36, 0xbc3c, 0xbc42, 0xbc48, 0xbc4e, 0xbc58, 0xbc5c, 0xbc60, 0xbc64, 0xbc68,
        0xbc6c, 0xbc70, 0xbc74, 0xbc78, 0xbc7c, 0xbc80, 0xbc84, 0xbc88, 0xbc8c, 0xbc90,
        0xbc94, 0xbc98, 0xbc9c, 0xbca0, 0xbca4, 0xbca8, 0xbcac, 0xbcb0, 0xbcb4, 0xbcb8,
        0xbcbc, 0xbcc0, 0xbcc4, 0xbcc8, 0xbccc, 0xbcd0, 0xbcd4, 0xbcdc, 0xbce0, 0xbce6,
        0xbcea, 0xbcee, 0xbcf6, 0xbcfa, 0xbd00, 0xbd04, 0xbd08, 0xbd10, 0xbd14, 0xbd1a,
        0xbd1e, 0xbd22, 0xbd2c, 0xbd32, 0xbd36, 0xbd3c, 0xbd42, 0xbd48, 0xbd4e, 0xbd54,
        0xbd5a, 0xbd60, 0xbd66, 0xbd6c, 0xbd72, 0xbd78, 0xbd7e, 0xbd84, 0xbd8a, 0xbd90,
        0xbd96, 0xbd9c, 0xbda0, 0xbda4, 0xbdaa, 0xbdb0, 0xbdb4, 0xbdba, 0xbdbe, 0xbdc4,
        0xbdc8, 0xbdcc, 0xbdd4, 0xbdd8, 0xbdde, 0xbde2, 0xbde6, 0xbdee, 0xbdf2, 0xbdf8,
        0xbdfc, 0xbe00, 0xbe08, 0xbe10, 0xbe16, 0xbe1c, 0xbe22, 0xbe28, 0xbe2e, 0xbe36,
        0xbe3c, 0xbe42, 0xbe48, 0xbe4e, 0xbe80, 0xbe88, 0xbe8e, 0xbe94, 0xbe9a, 0xbea0,
        0xbea6, 0xbeac, 0xbeb2, 0xbeb8, 0xbebe, 0xbec4, 0xbeca, 0xbed0, 0xbed6, 0xbedc,
        0xbee2, 0xbee8, 0xbeee, 0xbef6, 0xbefc, 0xbf02, 0xbf08, 0xbf0e, 0xbf14, 0xbf1c,
        0xbf22, 0xbf28, 0xbf2e, 0xbf34, 0xbf3e, 0xbf44, 0xbf4a, 0xbf50, 0xbf56, 0xbf66,
        0xbf6e, 0xbf74, 0xbf7a, 0xbf80, 0xbf88, 0xbf8e, 0xbf94, 0xbfb2, 0xbfb6, 0xbfbc,
        0xbfc0, 0xe14c, 0xe150, 0xe154, 0xe15a, 0xe160, 0xe166, 0xe16c, 0xe172, 0xe176,
        0xe17a, 0xe17e, 0xe184, 0xe188, 0xe18c, 0xe192, 0xe198, 0xe19e, 0xe1a4, 0xe1aa,
        0xe1b0, 0xe1b6, 0xe1bc, 0xe1c4, 0xe1c8, 0xe1ce, 0xe1d4, 0xe1d8, 0xe1dc, 0xe1e0,
        0xe1e4, 0xe1e8, 0xe1ec, 0xe1f0, 0xe1f4, 0xe1f8,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static CrocomireInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CrocomireInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Crocomire instruction mechanics pointer $A4:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa40000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
