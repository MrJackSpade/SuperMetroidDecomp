using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Fixed bank-$A9 words of Mother Brain's head instruction lists. Only the six
/// contiguous data regions identified by the native labels are stored here;
/// the CPU routines between them are dispatched as opcodes, not copied as data.
/// Frame durations, visual selectors, commands and operands retain native order.
/// </summary>
public static class MotherBrainHeadInstructionProgramDefinitions
{

    /// <summary>Native program bank $A9.</summary>
    internal const byte Bank = 0xa9;

    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase2_0</c> at $A9:9B7F.</summary>
    private const ushort InstList_MotherBrainHead_Stretching_Phase2_0 = 0x9b7f;
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase2_1</c> at $A9:9BAB.</summary>
    private const ushort InstList_MotherBrainHead_Stretching_Phase2_1 = 0x9bab;
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase3_0</c> at $A9:9BB3.</summary>
    private const ushort InstList_MotherBrainHead_Stretching_Phase3_0 = 0x9bb3;
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase3_1</c> at $A9:9BDF.</summary>
    private const ushort InstList_MotherBrainHead_Stretching_Phase3_1 = 0x9bdf;
    /// <summary><c>InstList_MotherBrainHead_HyperBeamRecoil_0</c> at $A9:9BE7.</summary>
    private const ushort InstList_MotherBrainHead_HyperBeamRecoil_0 = 0x9be7;
    /// <summary><c>InstList_MotherBrainHead_HyperBeamRecoil_1</c> at $A9:9C0B.</summary>
    private const ushort InstList_MotherBrainHead_HyperBeamRecoil_1 = 0x9c0b;
    /// <summary><c>InstList_MotherBrainHead_InitialDummy</c> at $A9:9C13.</summary>
    private const ushort InstList_MotherBrainHead_InitialDummy = 0x9c13;
    /// <summary><c>InstList_MotherBrainHead_Initial</c> at $A9:9C21.</summary>
    private const ushort InstList_MotherBrainHead_Initial = 0x9c21;
    /// <summary><c>InstList_MotherBrainHead_Decapitated_0</c> at $A9:9C29.</summary>
    private const ushort InstList_MotherBrainHead_Decapitated_0 = 0x9c29;
    /// <summary><c>InstList_MotherBrainHead_Decapitated_1</c> at $A9:9C31.</summary>
    private const ushort InstList_MotherBrainHead_Decapitated_1 = 0x9c31;
    /// <summary><c>InstList_MotherBrainHead_DyingDrool_0</c> at $A9:9C39.</summary>
    private const ushort InstList_MotherBrainHead_DyingDrool_0 = 0x9c39;
    /// <summary><c>InstList_MotherBrainHead_DyingDrool_1</c> at $A9:9C47.</summary>
    private const ushort InstList_MotherBrainHead_DyingDrool_1 = 0x9c47;
    /// <summary><c>InstList_MotherBrainHead_DyingDrool_2</c> at $A9:9C5F.</summary>
    private const ushort InstList_MotherBrainHead_DyingDrool_2 = 0x9c5f;
    /// <summary><c>InstList_MotherBrainHead_FiringRainbowBeam</c> at $A9:9C77.</summary>
    private const ushort InstList_MotherBrainHead_FiringRainbowBeam = 0x9c77;
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase2_0</c> at $A9:9C87.</summary>
    private const ushort InstList_MotherBrainHead_Neutral_Phase2_0 = 0x9c87;
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase2_1</c> at $A9:9C9F.</summary>
    private const ushort InstList_MotherBrainHead_Neutral_Phase2_1 = 0x9c9f;
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase3_0</c> at $A9:9CB9.</summary>
    private const ushort InstList_MotherBrainHead_Neutral_Phase3_0 = 0x9cb9;
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase3_1</c> at $A9:9CD1.</summary>
    private const ushort InstList_MotherBrainHead_Neutral_Phase3_1 = 0x9cd1;
    /// <summary><c>InstList_MotherBrainHead_Corpse_0</c> at $A9:9D25.</summary>
    private const ushort InstList_MotherBrainHead_Corpse_0 = 0x9d25;
    /// <summary><c>InstList_MotherBrainHead_Corpse_1</c> at $A9:9D35.</summary>
    private const ushort InstList_MotherBrainHead_Corpse_1 = 0x9d35;
    /// <summary><c>InstList_MotherBrainHead_Attacking_4OnionRings_Phase2</c> at $A9:9D3D.</summary>
    private const ushort InstList_MotherBrainHead_Attacking_4OnionRings_Phase2 = 0x9d3d;
    /// <summary><c>InstList_MotherBrainHead_Attacking_2OnionRings_Phase2</c> at $A9:9D7F.</summary>
    private const ushort InstList_MotherBrainHead_Attacking_2OnionRings_Phase2 = 0x9d7f;
    /// <summary><c>InstList_MotherBrainHead_Attacking_BabyMetroid</c> at $A9:9DB1.</summary>
    private const ushort InstList_MotherBrainHead_Attacking_BabyMetroid = 0x9db1;
    /// <summary><c>InstList_MotherBrainHead_AttackingSamus_4OnionRings_Phase3</c> at $A9:9DBB.</summary>
    private const ushort InstList_MotherBrainHead_AttackingSamus_4OnionRings_Phase3 = 0x9dbb;
    /// <summary><c>InstList_MotherBrainHead_Attacking_4OnionRings_Phase3</c> at $A9:9DC1.</summary>
    private const ushort InstList_MotherBrainHead_Attacking_4OnionRings_Phase3 = 0x9dc1;
    /// <summary><c>InstList_MotherBrainHead_Attacking_Bomb_Phase2</c> at $A9:9ECC.</summary>
    private const ushort InstList_MotherBrainHead_Attacking_Bomb_Phase2 = 0x9ecc;
    /// <summary><c>InstList_MotherBrainHead_Attacking_Bomb_Phase3</c> at $A9:9F00.</summary>
    private const ushort InstList_MotherBrainHead_Attacking_Bomb_Phase3 = 0x9f00;
    /// <summary><c>InstList_MotherBrainHead_Attacking_Laser</c> at $A9:9F34.</summary>
    private const ushort InstList_MotherBrainHead_Attacking_Laser = 0x9f34;
    /// <summary><c>InstList_MotherBrainHead_ChargingRainbowBeam_0</c> at $A9:9F6C.</summary>
    private const ushort InstList_MotherBrainHead_ChargingRainbowBeam_0 = 0x9f6c;
    /// <summary><c>InstList_MotherBrainHead_ChargingRainbowBeam_1</c> at $A9:9F7A.</summary>
    private const ushort InstList_MotherBrainHead_ChargingRainbowBeam_1 = 0x9f7a;
    /// <summary><c>UNUSED_InstList_MotherBrainHead_A99C7F</c> at $A9:9C7F.</summary>
    private const ushort UNUSED_InstList_MotherBrainHead_A99C7F = 0x9c7f;
    /// <summary><c>Instruction_MotherBrainHead_GotoNeutralPhase3</c> at $A9:9D21.</summary>
    private const ushort Instruction_MotherBrainHead_GotoNeutralPhase3 = 0x9d21;
    /// <summary><c>InstList_MotherBrainHead_SpawnLaserProjectile</c> at $A9:9F46.</summary>
    private const ushort InstList_MotherBrainHead_SpawnLaserProjectile = 0x9f46;
    /// <summary><c>UNUSED_ExtendedSpritemap_MotherBrainBrain_A9A320</c> at $A9:A320.</summary>
    private const ushort UNUSED_ExtendedSpritemap_MotherBrainBrain_A9A320 = 0xa320;
    /// <summary><c>Spritemaps_MotherBrain_0</c> at $A9:A586.</summary>
    private const ushort Spritemaps_MotherBrain_0 = 0xa586;
    /// <summary><c>Spritemaps_MotherBrain_1</c> at $A9:A5BF.</summary>
    private const ushort Spritemaps_MotherBrain_1 = 0xa5bf;
    /// <summary><c>Spritemaps_MotherBrain_2</c> at $A9:A5F8.</summary>
    private const ushort Spritemaps_MotherBrain_2 = 0xa5f8;
    /// <summary><c>Spritemaps_MotherBrain_3</c> at $A9:A62C.</summary>
    private const ushort Spritemaps_MotherBrain_3 = 0xa62c;
    /// <summary><c>Spritemaps_MotherBrain_4</c> at $A9:A660.</summary>
    private const ushort Spritemaps_MotherBrain_4 = 0xa660;
    /// <summary><c>Spritemaps_MotherBrain_6</c> at $A9:A69B.</summary>
    private const ushort Spritemaps_MotherBrain_6 = 0xa69b;
    /// <summary><c>Spritemaps_MotherBrain_7</c> at $A9:A6D9.</summary>
    private const ushort Spritemaps_MotherBrain_7 = 0xa6d9;
    /// <summary><c>Spritemaps_MotherBrain_8</c> at $A9:A717.</summary>
    private const ushort Spritemaps_MotherBrain_8 = 0xa717;
    /// <summary><c>Spritemaps_MotherBrain_9</c> at $A9:A750.</summary>
    private const ushort Spritemaps_MotherBrain_9 = 0xa750;
    /// <summary><c>Spritemaps_MotherBrain_A</c> at $A9:A789.</summary>
    private const ushort Spritemaps_MotherBrain_A = 0xa789;
    /// <summary><c>Spritemaps_MotherBrain_18</c> at $A9:AD3E.</summary>
    private const ushort Spritemaps_MotherBrain_18 = 0xad3e;
    /// <summary><c>Spritemaps_MotherBrain_19</c> at $A9:AD6D.</summary>
    private const ushort Spritemaps_MotherBrain_19 = 0xad6d;

    /// <summary>
    /// The six head-list regions in native order. Each frame's visual operand is the compiled
    /// extended-spritemap identity the dedicated head interpreter draws; durations are authored
    /// cadence (reviewed under #1165) and every control word is a named instruction or target.
    /// </summary>
    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0x9b7f),
        Entry(InstList_MotherBrainHead_Stretching_Phase2_0),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SetMainShakeTimerTo50),
        Frame(2, Spritemaps_MotherBrain_2),
        Frame(2, Spritemaps_MotherBrain_3),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile),
        Frame(2, Spritemaps_MotherBrain_3),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnPurpleBreathBigProjectile),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib2_Max6, 0x007e),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile),
        Frame(16, Spritemaps_MotherBrain_4),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile),
        Frame(16, Spritemaps_MotherBrain_4),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile),
        Frame(32, Spritemaps_MotherBrain_4),
        Frame(4, Spritemaps_MotherBrain_3),
        Entry(InstList_MotherBrainHead_Stretching_Phase2_1),
        Frame(1, Spritemaps_MotherBrain_2),
        Op((ushort)MotherBrainInstruction.MotherBrain_GotoX, InstList_MotherBrainHead_Stretching_Phase2_1),
        Entry(InstList_MotherBrainHead_Stretching_Phase3_0),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SetMainShakeTimerTo50),
        Frame(2, Spritemaps_MotherBrain_8),
        Frame(2, Spritemaps_MotherBrain_9),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile),
        Frame(2, Spritemaps_MotherBrain_9),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnPurpleBreathBigProjectile),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib2_Max6, 0x007e),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile),
        Frame(16, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile),
        Frame(16, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile),
        Frame(32, Spritemaps_MotherBrain_A),
        Frame(4, Spritemaps_MotherBrain_9),
        Entry(InstList_MotherBrainHead_Stretching_Phase3_1),
        Frame(1, Spritemaps_MotherBrain_8),
        Op((ushort)MotherBrainInstruction.MotherBrain_GotoX, InstList_MotherBrainHead_Stretching_Phase3_1),
        Entry(InstList_MotherBrainHead_HyperBeamRecoil_0),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SetMainShakeTimerTo50),
        Frame(2, Spritemaps_MotherBrain_8),
        Frame(2, Spritemaps_MotherBrain_9),
        Frame(2, Spritemaps_MotherBrain_9),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnPurpleBreathBigProjectile),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib2_Max6, 0x007e),
        Frame(16, Spritemaps_MotherBrain_A),
        Frame(16, Spritemaps_MotherBrain_A),
        Frame(32, Spritemaps_MotherBrain_A),
        Frame(4, Spritemaps_MotherBrain_9),
        Entry(InstList_MotherBrainHead_HyperBeamRecoil_1),
        Frame(1, Spritemaps_MotherBrain_8),
        Op((ushort)MotherBrainInstruction.MotherBrain_GotoX, InstList_MotherBrainHead_HyperBeamRecoil_1),
        Entry(InstList_MotherBrainHead_InitialDummy),
        Frame(0, UNUSED_ExtendedSpritemap_MotherBrainBrain_A9A320),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Frame(8, Spritemaps_MotherBrain_2),
        Frame(4, Spritemaps_MotherBrain_1),
        Entry(InstList_MotherBrainHead_Initial),
        Frame(4, Spritemaps_MotherBrain_0),
        Op((ushort)MotherBrainInstruction.MotherBrain_GotoX, InstList_MotherBrainHead_Initial),
        Entry(InstList_MotherBrainHead_Decapitated_0),
        Frame(8, Spritemaps_MotherBrain_8),
        Frame(4, Spritemaps_MotherBrain_7),
        Entry(InstList_MotherBrainHead_Decapitated_1),
        Frame(4, Spritemaps_MotherBrain_6),
        Op((ushort)MotherBrainInstruction.MotherBrain_GotoX, InstList_MotherBrainHead_Decapitated_1),
        Entry(InstList_MotherBrainHead_DyingDrool_0),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SetMainShakeTimerTo50),
        Frame(4, Spritemaps_MotherBrain_8),
        Frame(4, Spritemaps_MotherBrain_9),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib2_Max6, 0x007e),
        Entry(InstList_MotherBrainHead_DyingDrool_1),
        Frame(2, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile),
        Frame(2, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile),
        Frame(2, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile),
        Frame(2, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile),
        Entry(InstList_MotherBrainHead_DyingDrool_2),
        Frame(2, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_GotoDyingDroolInstList),
        Origin(0x9c77),
        Entry(InstList_MotherBrainHead_FiringRainbowBeam),
        Frame(1, Spritemaps_MotherBrain_2),
        Op((ushort)MotherBrainInstruction.MotherBrain_GotoX, InstList_MotherBrainHead_FiringRainbowBeam),
        Frame(1, Spritemaps_MotherBrain_8),
        Op((ushort)MotherBrainInstruction.MotherBrain_GotoX, UNUSED_InstList_MotherBrainHead_A99C7F),
        Entry(InstList_MotherBrainHead_Neutral_Phase2_0),
        Frame(4, Spritemaps_MotherBrain_0),
        Frame(4, Spritemaps_MotherBrain_1),
        Frame(8, Spritemaps_MotherBrain_2),
        Frame(4, Spritemaps_MotherBrain_1),
        Frame(4, Spritemaps_MotherBrain_0),
        Frame(4, Spritemaps_MotherBrain_1),
        Entry(InstList_MotherBrainHead_Neutral_Phase2_1),
        Frame(8, Spritemaps_MotherBrain_2),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_MaybeGotoNeutralPhase2),
        Frame(4, Spritemaps_MotherBrain_1),
        Op((ushort)MotherBrainInstruction.MotherBrain_GotoX, InstList_MotherBrainHead_Neutral_Phase2_0),
        Origin(0x9cb9),
        Entry(InstList_MotherBrainHead_Neutral_Phase3_0),
        Frame(4, Spritemaps_MotherBrain_6),
        Frame(4, Spritemaps_MotherBrain_7),
        Frame(8, Spritemaps_MotherBrain_8),
        Frame(4, Spritemaps_MotherBrain_7),
        Frame(4, Spritemaps_MotherBrain_6),
        Frame(4, Spritemaps_MotherBrain_7),
        Entry(InstList_MotherBrainHead_Neutral_Phase3_1),
        Frame(8, Spritemaps_MotherBrain_8),
        Frame(8, Spritemaps_MotherBrain_7),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_MaybeGotoNeutralPhase3),
        Frame(4, Spritemaps_MotherBrain_6),
        Op((ushort)MotherBrainInstruction.MotherBrain_GotoX, InstList_MotherBrainHead_Neutral_Phase3_0),
        Frame(4, Spritemaps_MotherBrain_8),
        Frame(4, Spritemaps_MotherBrain_9),
        Frame(2, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib2_Max6, 0x006f),
        Frame(2, Spritemaps_MotherBrain_A),
        Frame(2, Spritemaps_MotherBrain_A),
        Frame(2, Spritemaps_MotherBrain_A),
        Frame(2, Spritemaps_MotherBrain_A),
        Frame(4, Spritemaps_MotherBrain_9),
        Frame(4, Spritemaps_MotherBrain_8),
        Op(Instruction_MotherBrainHead_GotoNeutralPhase3),
        Origin(0x9d25),
        Entry(InstList_MotherBrainHead_Corpse_0),
        Frame(2, Spritemaps_MotherBrain_6),
        Frame(2, Spritemaps_MotherBrain_7),
        Frame(64, Spritemaps_MotherBrain_8),
        Frame(64, Spritemaps_MotherBrain_18),
        Entry(InstList_MotherBrainHead_Corpse_1),
        Frame(2, Spritemaps_MotherBrain_19),
        Op((ushort)MotherBrainInstruction.MotherBrain_GotoX, InstList_MotherBrainHead_Corpse_1),
        Entry(InstList_MotherBrainHead_Attacking_4OnionRings_Phase2),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_DisableNeckMovement),
        Frame(4, Spritemaps_MotherBrain_2),
        Frame(4, Spritemaps_MotherBrain_3),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib2_Max6, 0x006f),
        Frame(8, Spritemaps_MotherBrain_4),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_AimOnionRingsAtSamus),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnOnionRingsProjectile),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib3_Max6, 0x0017),
        Frame(3, Spritemaps_MotherBrain_4),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_AimOnionRingsAtSamus),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnOnionRingsProjectile),
        Frame(3, Spritemaps_MotherBrain_4),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_AimOnionRingsAtSamus),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnOnionRingsProjectile),
        Frame(3, Spritemaps_MotherBrain_4),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_AimOnionRingsAtSamus),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnOnionRingsProjectile),
        Frame(16, Spritemaps_MotherBrain_4),
        Frame(4, Spritemaps_MotherBrain_3),
        Frame(16, Spritemaps_MotherBrain_2),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_EnableNeckMovement_GotoX, InstList_MotherBrainHead_Neutral_Phase2_0),
        Entry(InstList_MotherBrainHead_Attacking_2OnionRings_Phase2),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_DisableNeckMovement),
        Frame(4, Spritemaps_MotherBrain_2),
        Frame(4, Spritemaps_MotherBrain_3),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib2_Max6, 0x006f),
        Frame(8, Spritemaps_MotherBrain_4),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_AimOnionRingsAtSamus),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnOnionRingsProjectile),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib3_Max6, 0x0017),
        Frame(3, Spritemaps_MotherBrain_4),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_AimOnionRingsAtSamus),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnOnionRingsProjectile),
        Frame(16, Spritemaps_MotherBrain_4),
        Frame(4, Spritemaps_MotherBrain_3),
        Frame(16, Spritemaps_MotherBrain_2),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_EnableNeckMovement_GotoX, InstList_MotherBrainHead_Neutral_Phase2_0),
        Entry(InstList_MotherBrainHead_Attacking_BabyMetroid),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_IncBabyMetroidAttackCounter),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_DisableNeckMovement),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_AimOnionRingsAtBabyMetroid),
        Op((ushort)MotherBrainInstruction.MotherBrain_GotoX, InstList_MotherBrainHead_Attacking_4OnionRings_Phase3),
        Entry(InstList_MotherBrainHead_AttackingSamus_4OnionRings_Phase3),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_ResetBabyMetroidAttackCounter),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_DisableNeckMovement),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_AimOnionRingsAtSamus),
        Entry(InstList_MotherBrainHead_Attacking_4OnionRings_Phase3),
        Frame(4, Spritemaps_MotherBrain_8),
        Frame(4, Spritemaps_MotherBrain_9),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueBabyMetroidAttackSFX),
        Frame(8, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnOnionRingsProjectile),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib3_Max6, 0x0017),
        Frame(3, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnOnionRingsProjectile),
        Frame(3, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnOnionRingsProjectile),
        Frame(3, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnOnionRingsProjectile),
        Frame(16, Spritemaps_MotherBrain_A),
        Frame(4, Spritemaps_MotherBrain_9),
        Frame(16, Spritemaps_MotherBrain_8),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_EnableNeckMovement_GotoX, InstList_MotherBrainHead_Neutral_Phase3_0),
        Origin(0x9ecc),
        Entry(InstList_MotherBrainHead_Attacking_Bomb_Phase2),
        Frame(4, Spritemaps_MotherBrain_0),
        Frame(4, Spritemaps_MotherBrain_1),
        Frame(8, Spritemaps_MotherBrain_2),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_DisableNeckMovement),
        Frame(4, Spritemaps_MotherBrain_2),
        Frame(4, Spritemaps_MotherBrain_3),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib2_Max6, 0x006f),
        Frame(8, Spritemaps_MotherBrain_4),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnBombProjectileWithParamX, 0x0007),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnPurpleBreathBigProjectile),
        Frame(32, Spritemaps_MotherBrain_4),
        Frame(4, Spritemaps_MotherBrain_3),
        Frame(16, Spritemaps_MotherBrain_2),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_EnableNeckMovement_GotoX, InstList_MotherBrainHead_Neutral_Phase2_0),
        Entry(InstList_MotherBrainHead_Attacking_Bomb_Phase3),
        Frame(4, Spritemaps_MotherBrain_6),
        Frame(4, Spritemaps_MotherBrain_7),
        Frame(8, Spritemaps_MotherBrain_8),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_DisableNeckMovement),
        Frame(4, Spritemaps_MotherBrain_8),
        Frame(4, Spritemaps_MotherBrain_9),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib2_Max6, 0x006f),
        Frame(8, Spritemaps_MotherBrain_A),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnBombProjectileWithParamX, 0x0001),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnPurpleBreathBigProjectile),
        Frame(32, Spritemaps_MotherBrain_A),
        Frame(4, Spritemaps_MotherBrain_9),
        Frame(16, Spritemaps_MotherBrain_8),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_EnableNeckMovement_GotoX, InstList_MotherBrainHead_Neutral_Phase3_0),
        Entry(InstList_MotherBrainHead_Attacking_Laser),
        Frame(16, Spritemaps_MotherBrain_1),
        Frame(4, Spritemaps_MotherBrain_2),
        Op(InstList_MotherBrainHead_SpawnLaserProjectile),
        Frame(32, Spritemaps_MotherBrain_2),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_EnableNeckMovement_GotoX, InstList_MotherBrainHead_Neutral_Phase2_0),
        Origin(0x9f6c),
        Entry(InstList_MotherBrainHead_ChargingRainbowBeam_0),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SetupEffectsForRainbowBeamCharge),
        Frame(4, Spritemaps_MotherBrain_2),
        Frame(4, Spritemaps_MotherBrain_1),
        Frame(2, Spritemaps_MotherBrain_0),
        Entry(InstList_MotherBrainHead_ChargingRainbowBeam_1),
        Op((ushort)MotherBrainInstruction.MotherBrainHead_SpawnRainbowBeamChargingProj),
        Frame(30, Spritemaps_MotherBrain_0),
        Op((ushort)MotherBrainInstruction.MotherBrain_GotoX, InstList_MotherBrainHead_ChargingRainbowBeam_1));

    /// <summary>Whether a word belongs to one of the six compiled head-list regions.</summary>
    public static bool ContainsWord(ushort pointer) => Layout.Owns(pointer);

    /// <summary>Reads one compiled native word from a head-list data region.</summary>
    public static ushort ReadWord(ushort pointer) =>
        Layout.TryReadWord(pointer, out ushort value) ? value :
            throw new InvalidDataException(
                $"Mother Brain head word $A9:{pointer:X4} is outside the compiled lists.");
}
