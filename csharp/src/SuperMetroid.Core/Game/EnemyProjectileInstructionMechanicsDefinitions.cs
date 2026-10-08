namespace SuperMetroid.Core.Game;

/// <summary>One immutable mechanics-owned word in a translated bank-$86 projectile program.</summary>
internal readonly record struct EnemyProjectileMechanicsWordDefinition(
    ushort Address,
    ushort Value);

/// <summary>
/// One bank-$86 timed program whose frame durations and control flow affect simulation.
/// Spritemap operands remain outside this definition and select installed presentation content.
/// </summary>
internal readonly record struct EnemyProjectileTimedProgramDefinition(
    ushort InitialPointer,
    int FrameCount,
    ushort? PrefixInstruction,
    ushort TerminalInstruction,
    ushort? TerminalOperand);

/// <summary>One presentation-only spritemap operand in a compiled bank-$86 program.</summary>
internal readonly record struct EnemyProjectilePresentationFrameDefinition(
    ushort OperandAddress,
    string Name);

/// <summary>
/// Compiled mechanics words for the translated Mother Brain and shared misc-dust enemy
/// projectile programs in bank $86.
/// </summary>
/// <remarks>
/// Native enemy-projectile lists interleave behavior words with bank-$8D spritemap pointers.
/// Only durations, opcodes, branch targets, and collision-radius operands belong here. Keeping
/// graphics operands in installed artwork preserves editable presentation data while preventing fixed
/// gameplay timing and control flow from depending on a runtime cartridge read.
/// </remarks>
internal static class EnemyProjectileInstructionMechanicsDefinitions
{
    /// <summary><c>$86:C432</c>, Mother Brain blue/onion-ring radius-and-frame program.</summary>
    internal const ushort MotherBrainBlueRingInitial = 0xc432;

    /// <summary><c>$86:C464</c>, Mother Brain blue/onion-ring impact program.</summary>
    internal const ushort MotherBrainBlueRingTouch = 0xc464;

    /// <summary><c>$86:C76E</c>, looping Mother Brain bomb animation program.</summary>
    internal const ushort MotherBrainBombInitial = 0xc76e;

    /// <summary><c>$86:CAA4</c>, finite large purple-breath animation program.</summary>
    internal const ushort MotherBrainPurpleBreathInitial = 0xcaa4;

    /// <summary><c>$86:CAC8</c>, finite small purple-breath program ending in $86:CAEE.</summary>
    internal const ushort MotherBrainPurpleBreathSmallInitial = 0xcac8;

    /// <summary><c>$86:C829</c>, finite Mother Brain rainbow-beam charge program.</summary>
    internal const ushort MotherBrainRainbowBeamChargingInitial = 0xc829;

    /// <summary><c>$86:C8B0</c>, attached Mother Brain drool release program.</summary>
    internal const ushort MotherBrainDroolInitial = 0xc8b0;

    /// <summary><c>$86:C8E1</c>, finite falling-drool splash program.</summary>
    internal const ushort MotherBrainDroolFalling = 0xc8e1;

    /// <summary><c>$86:CA22</c>, looping exploded escape-door fragment program.</summary>
    internal const ushort MotherBrainEscapeDoorFragmentInitial = 0xca22;

    /// <summary><c>$86:CB0D</c>, alternate-language subtitle frame followed by sleep.</summary>
    internal const ushort MotherBrainSubtitleInitial = 0xcb0d;

    /// <summary><c>$86:E152</c>, Mother Brain rainbow-impact explosion program.</summary>
    internal const ushort MotherBrainRainbowExplosionInitial = 0xe152;

    /// <summary><c>$86:E138</c>, Mother Brain small death-explosion program.</summary>
    internal const ushort MotherBrainSmallDeathExplosionInitial = 0xe138;

    /// <summary><c>$86:E1EA</c>, Mother Brain death-smoke program.</summary>
    internal const ushort MotherBrainDeathSmokeInitial = 0xe1ea;

    /// <summary><c>$86:E208</c>, Mother Brain large death-explosion program.</summary>
    internal const ushort MotherBrainBigDeathExplosionInitial = 0xe208;

    /// <summary>$86:E0EE, <c>UNUSED_InstList_EnemyProj_MiscDust_0_BeamCharge_86E0EE</c>, native misc-dust selector 0: BeamCharge.</summary>
    internal const ushort MiscDustBeamCharge = 0xe0ee;
    /// <summary>$86:E100, <c>InstList_EnemyProj_MiscDust_1_MotherBrainElbowChargeParticle</c>, native misc-dust selector 1: ElbowChargeParticle.</summary>
    internal const ushort MiscDustElbowChargeParticle = 0xe100;
    /// <summary>$86:E11A, <c>InstList_EnemyProj_MiscDust_2_MotherBrainElbowChargeEnergy</c>, native misc-dust selector 2: ElbowChargeEnergy.</summary>
    internal const ushort MiscDustElbowChargeEnergy = 0xe11a;
    /// <summary>$86:E168, <c>UNUSED_InstList_EnemyProj_MiscDust_5_BeamTrail_86E168</c>, native misc-dust selector 5: BeamTrail.</summary>
    internal const ushort MiscDustBeamTrail = 0xe168;
    /// <summary>$86:E17E, <c>InstList_EnemyProj_MiscDust_6_DudShot_TinyExplosion</c>, native misc-dust selector 6: DudShot.</summary>
    internal const ushort MiscDustDudShot = 0xe17e;
    /// <summary>$86:E198, <c>UNUSED_InstList_EnemyProj_MiscDust_7_PowerBomb_86E198</c>, native misc-dust selector 7: PowerBomb.</summary>
    internal const ushort MiscDustPowerBomb = 0xe198;
    /// <summary>$86:E1A6, <c>UNUSED_InstList_EnemyProj_MiscDust_8_ElevatorPad_86E1A6</c>, native misc-dust selector 8: ElevatorPad.</summary>
    internal const ushort MiscDustElevatorPad = 0xe1a6;
    /// <summary>$86:E1B0, <c>InstList_EnemyProj_MiscDust_9_SmallDustCloud</c>, native misc-dust selector 9: SmallDustCloud.</summary>
    internal const ushort MiscDustSmallDustCloud = 0xe1b0;
    /// <summary>$86:E1C6, <c>InstList_EnemyProj_MiscDust_A_CorpseDustCloud</c>, native misc-dust selector 10: CorpseDustCloud.</summary>
    internal const ushort MiscDustCorpseDustCloud = 0xe1c6;
    /// <summary>$86:E1D8, <c>InstList_EnemyProj_MiscDust_B_EyeDoorSweatDrop</c>, native misc-dust selector 11: EyeDoorSweat.</summary>
    internal const ushort MiscDustEyeDoorSweat = 0xe1d8;
    /// <summary>$86:E222, <c>UNUSED_InstList_EnemyProj_MiscDust_D_SmallEnergyDrop_86E222</c>, native misc-dust selector 13: SmallHealthDrop.</summary>
    internal const ushort MiscDustSmallHealthDrop = 0xe222;
    /// <summary>$86:E234, <c>UNUSED_InstList_EnemyProj_MiscDust_E_BigEnergyDrop_86E22E</c>, native misc-dust selector 14: BigHealthDrop.</summary>
    internal const ushort MiscDustBigHealthDrop = 0xe234;
    /// <summary>$86:E246, <c>UNUSED_InstList_EnemyProj_MiscDust_F_Bomb_86E246</c>, native misc-dust selector 15: Bomb.</summary>
    internal const ushort MiscDustBomb = 0xe246;
    /// <summary>$86:E258, <c>UNUSED_InstList_EnemyProj_MiscDust_10_WeirdEnergyDrop_86E258</c>, native misc-dust selector 16: WeirdHealthDrop.</summary>
    internal const ushort MiscDustWeirdHealthDrop = 0xe258;
    /// <summary>$86:E266, <c>InstList_EnemyProj_MiscDust_11_RockParticles</c>, native misc-dust selector 17: RockParticles.</summary>
    internal const ushort MiscDustRockParticles = 0xe266;
    /// <summary>$86:E2A8, <c>InstList_EnemyProj_MiscDust_12_ShortBigDustCloud</c>, native misc-dust selector 18: ShortBigDustCloud.</summary>
    internal const ushort MiscDustShortBigDustCloud = 0xe2a8;
    /// <summary>$86:E2BA, <c>UNUSED_InstList_EnemyProj_MiscDust_13_CloudShortBeam_86E2BA</c>, native misc-dust selector 19: ShortBeamCloud.</summary>
    internal const ushort MiscDustShortBeamCloud = 0xe2ba;
    /// <summary>$86:E2D4, <c>UNUSED_InstList_EnemyProj_MiscDust_14_CloudMediumBeam_86E2D4</c>, native misc-dust selector 20: MediumBeamCloud.</summary>
    internal const ushort MiscDustMediumBeamCloud = 0xe2d4;
    /// <summary>$86:E2F2, <c>InstList_EnemyProj_MiscDust_15_BigDustCloud</c>, native misc-dust selector 21: BigDustCloud.</summary>
    internal const ushort MiscDustBigDustCloud = 0xe2f2;
    /// <summary>$86:E314, <c>UNUSED_InstList_EnemyProj_MiscDust_16_LongBeam_86E314</c>, native misc-dust selector 22: LongBeam.</summary>
    internal const ushort MiscDustLongBeam = 0xe314;
    /// <summary>$86:E392, <c>UNUSED_InstList_EnemyProj_MiscDust_17_FlickerLongBeam_86E392</c>, native misc-dust selector 23: FlickeringBeam.</summary>
    internal const ushort MiscDustFlickeringBeam = 0xe392;
    /// <summary>$86:E3A0, <c>InstList_EnemyProj_MiscDust_18_LongDraygonBreathBubbles</c>, native misc-dust selector 24: DraygonBubbles.</summary>
    internal const ushort MiscDustDraygonBubbles = 0xe3a0;
    /// <summary>$86:E3C6, <c>UNUSED_InstList_EnemyProj_MiscDust_19_SaveStationElec_86E3C6</c>, native misc-dust selector 25: SaveStationLaser.</summary>
    internal const ushort MiscDustSaveStationLaser = 0xe3c6;
    /// <summary>$86:E3E8, <c>UNUSED_InstList_EnemyProj_MiscDust_ExpandVerticalGate_86E3E8</c>, native misc-dust selector 26: ExpandingGate.</summary>
    internal const ushort MiscDustExpandingGate = 0xe3e8;
    /// <summary>$86:E40A, <c>UNUSED_InstList_EnemyProj_MiscDust_ContractVertGate_86E40A</c>, native misc-dust selector 27: ContractingGate.</summary>
    internal const ushort MiscDustContractingGate = 0xe40a;
    /// <summary>$86:E1FC, <c>UNUSED_InstList_EnemyProj_MiscDust_1C_ElevatorPad_86E1FC</c>, native misc-dust selector 28: LoopingElevatorPad.</summary>
    internal const ushort MiscDustLoopingElevatorPad = 0xe1fc;

    private const int BlueRingRadiusCount = 6;

    /// <summary>$8D:8276/827D/8284..82C6: mutually exclusive tiny seed, single ring and composite radial growth forms.</summary>
    private enum RingGrowthPhase { Seed, Formation, RadialExpansion }
    /// <summary>$8D:B098/B0AE versus B0C4/B0DA/B0F0: four small versus four large burst quadrants.</summary>
    private enum RainbowBurstPhase { Compact, Expanded }
    /// <summary>$86:C436 holds the single 8x8 seed tile $1AD selected by $8D:8276.</summary>
    private const ushort UnresolvedRingSeedFrames = 16;
    /// <summary>$86:C43E holds the single 16x16 ring tile $1A7 selected by $8D:827D.</summary>
    private const ushort UnresolvedRingFormationFrames = 10;
    /// <summary>$86:C446/C44E/C456/C45E: composite quadrant growth holds 8/7/6/5 ticks at collision radii 3/4/5/6.</summary>
    private const int UnresolvedRingExpansionCadenceBase = 11;
    /// <summary>$86:E152/E156: compact 16x16 burst formed from four 8x8 quadrants at $8D:B098/B0AE.</summary>
    private const ushort UnresolvedCompactRainbowBurstFrames = 3;
    /// <summary>$86:E15A/E15E/E162: expanded 32x32 burst formed from four 16x16 quadrants at $8D:B0C4/B0DA/B0F0.</summary>
    private const ushort UnresolvedExpandedRainbowBurstFrames = 4;
    /// <summary>
    /// $86:E2CE holds final centered beam tile $6C ($8D:AD16) for five ticks before deletion.
    /// This independent dwell remains required by #1165: program termination does not
    /// yet justify its five-versus-three timing. The medium-beam sibling continues
    /// through the same pose for three ticks at $86:E2E8 before adding a second tile.
    /// </summary>
    private const ushort UnresolvedShortBeamTerminalFrames = 5;


    /// <summary>$86:C829: RainbowCharge selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedRainbowChargeFrames = 5;
    /// <summary>$86:C8E3: DroolSplash selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedDroolSplashFrames = 10;
    /// <summary>$86:CB0D: Subtitle selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedSubtitleFrames = 1;
    /// <summary>$86:E0EE: BeamCharge selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedBeamChargeFrames = 3;
    /// <summary>$86:E1C6: CorpseDust selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedCorpseDustFrames = 3;
    /// <summary>$86:E2D4: MediumBeamCloud selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedMediumBeamCloudFrames = 3;
    /// <summary>$86:E17E: DudShot selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedDudShotFrames = 4;
    /// <summary>$86:E40A: ContractingGate selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedContractingGateFrames = 4;
    /// <summary>$86:E198: PowerBomb selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedPowerBombFrames = 5;
    /// <summary>$86:E1B0: SmallDust selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedSmallDustFrames = 5;
    /// <summary>$86:E1D8: EyeSweat selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedEyeSweatFrames = 5;
    /// <summary>$86:E246: DustBomb selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedDustBombFrames = 5;
    /// <summary>$86:E2F2: BigDust selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedBigDustFrames = 5;
    /// <summary>$86:E208: BigDeathExplosion selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedBigDeathExplosionFrames = 5;
    /// <summary>$86:E1A6: ElevatorPad selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedElevatorPadFrames = 1;
    /// <summary>$86:E314: LongBeam selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedLongBeamFrames = 1;
    /// <summary>$86:E392: FlickeringBeam selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedFlickeringBeamFrames = 1;
    /// <summary>$86:E3C6: SaveStationLaser selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedSaveStationLaserFrames = 1;
    /// <summary>$86:E1FC: LoopingElevatorPad selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedLoopingElevatorPadFrames = 1;
    /// <summary>$86:E1EA: DeathSmoke selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedDeathSmokeFrames = 8;
    /// <summary>$86:E222: SmallHealth selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedSmallHealthFrames = 8;
    /// <summary>$86:E234: BigHealth selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedBigHealthFrames = 8;
    /// <summary>$86:E3A0: DraygonBubbles selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedDraygonBubblesFrames = 8;
    /// <summary>$86:E258: WeirdHealth selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedWeirdHealthFrames = 16;
    /// <summary>$86:E3E8: ExpandingGate selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedExpandingGateFrames = 16;
    /// <summary>$86:E266: RockParticles selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedRockParticlesFrames = 2;
    /// <summary>$86:E2A8: ShortBigDust selected repeated hold remains required; repeated program structure does not justify its magnitude.</summary>
    private const ushort UnresolvedShortBigDustFrames = 2;
    /// <summary>$86:C76E: bomb hold ramp peak; this chosen timing input remains required.</summary>
    private const ushort UnresolvedBombPeakFrames = 6;
    /// <summary>$86:C76E-C78E: bomb hold change per pose; this chosen timing input remains required.</summary>
    private const ushort UnresolvedBombHoldStepFrames = 1;
    /// <summary>$86:CAA6: purple-breath initial paired dwell; this chosen timing input remains required.</summary>
    private const ushort UnresolvedPurpleBreathStartFrames = 8;
    /// <summary>$86:CAA6-CAC2: purple-breath increment per pair; this chosen timing input remains required.</summary>
    private const ushort UnresolvedPurpleBreathStepFrames = 1;
    /// <summary>$86:CACA-CACE: the small breath's first two poses hold eight frames each.</summary>
    private const ushort SmallPurpleBreathOpeningFrames = 8;
    /// <summary>$86:CAD2-CAE6: its later pairs hold ten, eleven and twelve frames.</summary>
    private const ushort SmallPurpleBreathLaterStartFrames = 10;
    /// <summary>$86:CA22-CA2E: first four fragment poses; this chosen timing input remains required.</summary>
    private const ushort UnresolvedDoorEarlyFrames = 1;
    /// <summary>$86:CA32: first later fragment pair; this chosen timing input remains required.</summary>
    private const ushort UnresolvedDoorLaterStartFrames = 3;
    /// <summary>$86:CA32-CA3E: fragment increment per later pair; this chosen timing input remains required.</summary>
    private const ushort UnresolvedDoorLaterStepFrames = 1;
    /// <summary>$86:E100: elbow-particle initial hold; this chosen timing input remains required.</summary>
    private const ushort UnresolvedElbowParticleStartFrames = 5;
    /// <summary>$86:E108-E114: elbow-particle hold floor; this chosen timing input remains required.</summary>
    private const ushort UnresolvedElbowParticleMinimumFrames = 3;
    /// <summary>$86:E100-E108: elbow-particle decline per pose; this chosen timing input remains required.</summary>
    private const ushort UnresolvedElbowParticleStepFrames = 1;
    /// <summary>$86:E11A: elbow-energy initial hold; this chosen timing input remains required.</summary>
    private const ushort UnresolvedElbowEnergyStartFrames = 4;
    /// <summary>$86:E122-E12E: elbow-energy hold floor; this chosen timing input remains required.</summary>
    private const ushort UnresolvedElbowEnergyMinimumFrames = 2;
    /// <summary>$86:E11A-E122: elbow-energy decline per pose; this chosen timing input remains required.</summary>
    private const ushort UnresolvedElbowEnergyStepFrames = 1;
    /// <summary>$86:E132: terminal held ignition pose; this chosen timing input remains required.</summary>
    private const ushort UnresolvedElbowEnergyTerminalFrames = 12;
    /// <summary>$86:E168-E174: beam trail poses; this chosen timing input remains required.</summary>
    private const ushort UnresolvedTrailFrames = 8;
    /// <summary>$86:E178: terminal charge-particle pose; this chosen timing input remains required.</summary>
    private const ushort UnresolvedTrailTerminalFrames = 24;
    /// <summary>$86:E2BA-E2CA: ordinary short-beam cloud poses; this chosen timing input remains required.</summary>
    private const ushort UnresolvedShortBeamOrdinaryFrames = 3;
    /// <summary>$86:C468-C47C: ring impact poses; this chosen timing input remains required.</summary>
    private const ushort UnresolvedRingTouchFrames = 5;
    /// <summary>$86:C8B0-C8C0: attached drool poses; this chosen timing input remains required.</summary>
    private const ushort UnresolvedDroolAttachedFrames = 10;
    /// <summary>$86:C8CA: released drool pose before Sleep; this chosen timing input remains required.</summary>
    private const ushort UnresolvedDroolReleaseFrames = 10;

    private static ushort BlueRingDuration(int radius)
    {
        RingGrowthPhase phase = radius switch
        {
            1 => RingGrowthPhase.Seed,
            2 => RingGrowthPhase.Formation,
            >= 3 and <= BlueRingRadiusCount => RingGrowthPhase.RadialExpansion,
            _ => throw new ArgumentOutOfRangeException(nameof(radius)),
        };
        return phase switch
        {
            RingGrowthPhase.Seed => UnresolvedRingSeedFrames,
            RingGrowthPhase.Formation => UnresolvedRingFormationFrames,
            _ => (ushort)(UnresolvedRingExpansionCadenceBase - radius),
        };
    }

    private static ushort RainbowExplosionDuration(int frame)
    {
        RainbowBurstPhase phase = frame < 2 ? RainbowBurstPhase.Compact : RainbowBurstPhase.Expanded;
        return phase == RainbowBurstPhase.Compact ? UnresolvedCompactRainbowBurstFrames : UnresolvedExpandedRainbowBurstFrames;
    }
    private const int TimedProgramCount = 37;
    private static EnemyProjectileTimedProgramDefinition TimedProgram(int index) => index switch
    {
        0 => new(MotherBrainBombInitial, 9, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY, MotherBrainBombInitial),
        1 => new(MotherBrainRainbowBeamChargingInitial, 6, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        2 => new(MotherBrainDroolFalling, 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        3 => new(MotherBrainEscapeDoorFragmentInitial, 8, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY, MotherBrainEscapeDoorFragmentInitial),
        4 => new(MotherBrainPurpleBreathInitial, 8, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        5 => new(MotherBrainSubtitleInitial, 1, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep, null),
        6 => new(MiscDustBeamCharge, 4, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        7 => new(MiscDustElbowChargeParticle, 6, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        8 => new(MiscDustElbowChargeEnergy, 7, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        9 => new(MotherBrainSmallDeathExplosionInitial, 6, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        10 => new(MotherBrainRainbowExplosionInitial, 5, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        11 => new(MiscDustBeamTrail, 5, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        12 => new(MiscDustDudShot, 6, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        13 => new(MiscDustPowerBomb, 3, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        14 => new(MiscDustElevatorPad, 2, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        15 => new(MiscDustSmallDustCloud, 5, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        16 => new(MiscDustCorpseDustCloud, 4, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        17 => new(MiscDustEyeDoorSweat, 4, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        18 => new(MotherBrainDeathSmokeInitial, 4, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        19 => new(MiscDustLoopingElevatorPad, 2, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY, MiscDustLoopingElevatorPad),
        20 => new(MotherBrainBigDeathExplosionInitial, 6, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        21 => new(MiscDustSmallHealthDrop, 4, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        22 => new(MiscDustBigHealthDrop, 4, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        23 => new(MiscDustBomb, 4, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        24 => new(MiscDustWeirdHealthDrop, 3, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        25 => new(MiscDustRockParticles, 16, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        26 => new(MiscDustShortBigDustCloud, 4, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        27 => new(MiscDustShortBeamCloud, 6, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        28 => new(MiscDustMediumBeamCloud, 7, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        29 => new(MiscDustBigDustCloud, 8, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        30 => new(MiscDustLongBeam, 31, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        31 => new(MiscDustFlickeringBeam, 3, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        32 => new(MiscDustDraygonBubbles, 9, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        33 => new(MiscDustSaveStationLaser, 8, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        34 => new(MiscDustExpandingGate, 8, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        35 => new(MiscDustContractingGate, 8, null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        // The breath-inactive opcode takes no operand; the word after it is the Delete opcode.
        36 => new(MotherBrainPurpleBreathSmallInitial, 8, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_MotherBrainPurpleBreath_Inactive,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    private static ushort TimedDuration(ushort program, int frame) => program switch
    {
        MotherBrainBombInitial => (ushort)(UnresolvedBombPeakFrames - Math.Min(frame, 9 - frame) * UnresolvedBombHoldStepFrames),
        MotherBrainPurpleBreathInitial => (ushort)(UnresolvedPurpleBreathStartFrames + frame / 2 * UnresolvedPurpleBreathStepFrames),
        MotherBrainPurpleBreathSmallInitial => frame < 2
            ? SmallPurpleBreathOpeningFrames
            : (ushort)(SmallPurpleBreathLaterStartFrames + (frame - 2) / 2),
        MotherBrainEscapeDoorFragmentInitial => (ushort)(frame < 4 ? UnresolvedDoorEarlyFrames : UnresolvedDoorLaterStartFrames + (frame - 4) / 2 * UnresolvedDoorLaterStepFrames),
        MiscDustElbowChargeParticle => (ushort)Math.Max(UnresolvedElbowParticleMinimumFrames, UnresolvedElbowParticleStartFrames - frame * UnresolvedElbowParticleStepFrames),
        // The final pose changes to small-explosion ignition; its selected hold remains required.
        MiscDustElbowChargeEnergy => (ushort)(frame == 6 ? UnresolvedElbowEnergyTerminalFrames : Math.Max(UnresolvedElbowEnergyMinimumFrames, UnresolvedElbowEnergyStartFrames - frame * UnresolvedElbowEnergyStepFrames)),
        MotherBrainSmallDeathExplosionInitial => SmallExplosionAnimationDefinitions.Duration(frame),
        MotherBrainRainbowExplosionInitial => RainbowExplosionDuration(frame),
        // The final distinct charge-particle pose and trail dwell magnitudes remain independent.
        MiscDustBeamTrail => frame == 4 ? UnresolvedTrailTerminalFrames : UnresolvedTrailFrames,
        MiscDustShortBeamCloud => frame == 5 ? UnresolvedShortBeamTerminalFrames : UnresolvedShortBeamOrdinaryFrames,
        MotherBrainRainbowBeamChargingInitial => UnresolvedRainbowChargeFrames,
        MotherBrainDroolFalling => UnresolvedDroolSplashFrames,
        MotherBrainSubtitleInitial => UnresolvedSubtitleFrames,
        MiscDustBeamCharge => UnresolvedBeamChargeFrames,
        MiscDustCorpseDustCloud => UnresolvedCorpseDustFrames,
        MiscDustMediumBeamCloud => UnresolvedMediumBeamCloudFrames,
        MiscDustDudShot => UnresolvedDudShotFrames,
        MiscDustContractingGate => UnresolvedContractingGateFrames,
        MiscDustPowerBomb => UnresolvedPowerBombFrames,
        MiscDustSmallDustCloud => UnresolvedSmallDustFrames,
        MiscDustEyeDoorSweat => UnresolvedEyeSweatFrames,
        MiscDustBomb => UnresolvedDustBombFrames,
        MiscDustBigDustCloud => UnresolvedBigDustFrames,
        MotherBrainBigDeathExplosionInitial => UnresolvedBigDeathExplosionFrames,
        MiscDustElevatorPad => UnresolvedElevatorPadFrames,
        MiscDustLongBeam => UnresolvedLongBeamFrames,
        MiscDustFlickeringBeam => UnresolvedFlickeringBeamFrames,
        MiscDustSaveStationLaser => UnresolvedSaveStationLaserFrames,
        MiscDustLoopingElevatorPad => UnresolvedLoopingElevatorPadFrames,
        MotherBrainDeathSmokeInitial => UnresolvedDeathSmokeFrames,
        MiscDustSmallHealthDrop => UnresolvedSmallHealthFrames,
        MiscDustBigHealthDrop => UnresolvedBigHealthFrames,
        MiscDustDraygonBubbles => UnresolvedDraygonBubblesFrames,
        MiscDustWeirdHealthDrop => UnresolvedWeirdHealthFrames,
        MiscDustExpandingGate => UnresolvedExpandingGateFrames,
        MiscDustRockParticles => UnresolvedRockParticlesFrames,
        MiscDustShortBigDustCloud => UnresolvedShortBigDustFrames,
        _ => throw new InvalidDataException($"Unknown timed projectile program ${program:X4}."),
    };
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.MotherBrainOnionRing or
        RoomEnemyProjectileKind.MotherBrainBomb or
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig or
        RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall or
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging or
        RoomEnemyProjectileKind.MotherBrainDrool or
        RoomEnemyProjectileKind.MotherBrainDyingDrool or
        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion or
        RoomEnemyProjectileKind.MotherBrainDeathExplosion or
        RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment or
        RoomEnemyProjectileKind.MotherBrainEscapeSubtitle or
        RoomEnemyProjectileKind.EyeDoorSmoke or
        RoomEnemyProjectileKind.MiscDustExplosion;

    internal static ushort MiscDustInitialPointer(ushort animationIndex)
    {
        if (animationIndex >= 30)
        {
            throw new ArgumentOutOfRangeException(
                nameof(animationIndex), animationIndex,
                "Bank-$86 misc-dust animation index must be in the native $00..$1D range.");
        }

        return animationIndex switch
        {
            0 => MiscDustBeamCharge,
            1 => MiscDustElbowChargeParticle,
            2 => MiscDustElbowChargeEnergy,
            3 => MotherBrainSmallDeathExplosionInitial,
            4 => MotherBrainRainbowExplosionInitial,
            5 => MiscDustBeamTrail,
            6 => MiscDustDudShot,
            7 => MiscDustPowerBomb,
            8 => MiscDustElevatorPad,
            9 => MiscDustSmallDustCloud,
            10 => MiscDustCorpseDustCloud,
            11 => MiscDustEyeDoorSweat,
            12 => MotherBrainDeathSmokeInitial,
            13 => MiscDustSmallHealthDrop,
            14 => MiscDustBigHealthDrop,
            15 => MiscDustBomb,
            16 => MiscDustWeirdHealthDrop,
            17 => MiscDustRockParticles,
            18 => MiscDustShortBigDustCloud,
            19 => MiscDustShortBeamCloud,
            20 => MiscDustMediumBeamCloud,
            21 => MiscDustBigDustCloud,
            22 => MiscDustLongBeam,
            23 => MiscDustFlickeringBeam,
            24 => MiscDustDraygonBubbles,
            25 => MiscDustSaveStationLaser,
            26 => MiscDustExpandingGate,
            27 => MiscDustContractingGate,
            28 => MiscDustLoopingElevatorPad,
            29 => MotherBrainBigDeathExplosionInitial,
            _ => throw new ArgumentOutOfRangeException(nameof(animationIndex)),
        };
    }

    /// <summary>Native $86:E42C selector domain, including its final two out-of-order roots.</summary>
    internal static int MiscDustProgramCount => 30;

    internal static int NativeWordCount
    {
        get
        {
            int count = 38;
            for (int index = 0; index < TimedProgramCount; index++)
            {
                var program = TimedProgram(index);
                count += program.FrameCount + 1 + (program.PrefixInstruction.HasValue ? 1 : 0)
                    + (program.TerminalOperand.HasValue ? 1 : 0);
            }
            return count;
        }
    }

    internal static ushort ReadMechanicsWord(ushort address) => TryReadMechanicsWord(address, out ushort value)
        ? value : throw new InvalidDataException($"Bank-$86 projectile mechanics pointer ${address:X4} is outside the translated program domain.");

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        int offset = address - MotherBrainBlueRingInitial;
        if (offset is >= 0 and <= 48 && (offset & 1) == 0)
        {
            if (offset == 48) { value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep; return true; }
            int radius = offset / 8 + 1;
            switch (offset % 8)
            {
                case 0: value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_XYRadiusInY; return true;
                case 2: value = (ushort)(radius | radius << 8); return true;
                case 4: value = BlueRingDuration(radius); return true;
            }
        }
        offset = address - MotherBrainBlueRingTouch;
        if (offset == 0) { value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_UsePalette0_Duplicate; return true; }
        if (offset == 2) { value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction; return true; }
        if (offset is >= 4 and < 28 && offset % 4 == 0) { value = UnresolvedRingTouchFrames; return true; }
        if (offset == 28) { value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete; return true; }
        offset = address - MotherBrainDroolInitial;
        if (offset is >= 0 and < 20 && offset % 4 == 0) { value = UnresolvedDroolAttachedFrames; return true; }
        switch (offset)
        {
            case 20: value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY; return true;
            case 22: value = EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MotherBrainsDrool_Falling; return true;
            case 24: value = EnemyProjectileCodePointers.Instruction_EnemyProj_MotherBrainsDrool_MoveDownCPixels; return true;
            case 26: value = UnresolvedDroolReleaseFrames; return true;
            case 30: value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep; return true;
        }
        for (int index = 0; index < TimedProgramCount; index++)
        {
            var program = TimedProgram(index);
            offset = address - program.InitialPointer;
            if (program.PrefixInstruction is { } prefix)
            {
                if (offset == 0) { value = prefix; return true; }
                offset -= 2;
            }
            if (offset >= 0 && offset < program.FrameCount * 4 && offset % 4 == 0)
            { value = TimedDuration(program.InitialPointer, offset / 4); return true; }
            if (offset == program.FrameCount * 4) { value = program.TerminalInstruction; return true; }
            if (offset == program.FrameCount * 4 + 2 && program.TerminalOperand is { } operand)
            { value = operand; return true; }
        }
        value = 0;
        return false;
    }

    internal static bool IsCompiledMechanicsByte(int address) => (address & 0xff0000) == 0x860000 &&
        (TryReadMechanicsWord((ushort)address, out _) || TryReadMechanicsWord(unchecked((ushort)(address - 1)), out _));

    internal static EnemyProjectileMechanicsWordDefinition NativeWord(int index)
    {
        if ((uint)index >= NativeWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        foreach (ushort address in MechanicsAddresses())
            if (index-- == 0) return new(address, ReadMechanicsWord(address));
        throw new InvalidOperationException("Projectile mechanics enumeration count disagrees with its programs.");
    }

    private static IEnumerable<ushort> MechanicsAddresses()
    {
        for (int frame = 0; frame < BlueRingRadiusCount; frame++)
            for (int field = 0; field < 3; field++)
                yield return (ushort)(MotherBrainBlueRingInitial + frame * 8 + field * 2);
        yield return MotherBrainBlueRingInitial + 48;
        yield return MotherBrainBlueRingTouch;
        yield return MotherBrainBlueRingTouch + 2;
        for (int frame = 0; frame < 6; frame++) yield return (ushort)(MotherBrainBlueRingTouch + 4 + frame * 4);
        yield return MotherBrainBlueRingTouch + 28;
        for (int index = 0; index < TimedProgramCount; index++)
        {
            var program = TimedProgram(index);
            if (program.InitialPointer == MotherBrainDroolFalling)
            {
                for (int frame = 0; frame < 5; frame++) yield return (ushort)(MotherBrainDroolInitial + frame * 4);
                for (int field = 0; field < 4; field++) yield return (ushort)(MotherBrainDroolInitial + 20 + field * 2);
                yield return MotherBrainDroolInitial + 30;
            }
            int first = program.InitialPointer;
            if (program.PrefixInstruction.HasValue) { yield return (ushort)first; first += 2; }
            for (int frame = 0; frame < program.FrameCount; frame++) yield return (ushort)(first + frame * 4);
            yield return (ushort)(first + program.FrameCount * 4);
            if (program.TerminalOperand.HasValue) yield return (ushort)(first + program.FrameCount * 4 + 2);
        }
    }

    internal static VisualFrameList VisualFrames { get; } = new();

    internal static bool IsVisualOperand(ushort address)
    {
        int offset = address - MotherBrainBlueRingInitial;
        if (offset is >= 6 and < 48 && offset % 8 == 6) return true;
        offset = address - MotherBrainBlueRingTouch;
        if (offset is >= 6 and < 28 && offset % 4 == 2) return true;
        offset = address - MotherBrainDroolInitial;
        if (offset is >= 2 and < 20 && offset % 4 == 2 || offset == 28) return true;
        for (int index = 0; index < TimedProgramCount; index++)
        {
            var program = TimedProgram(index);
            offset = address - program.InitialPointer - (program.PrefixInstruction.HasValue ? 2 : 0);
            if (offset is >= 2 && offset < program.FrameCount * 4 && offset % 4 == 2) return true;
        }
        return false;
    }

    internal sealed class VisualFrameList : IReadOnlyList<EnemyProjectilePresentationFrameDefinition>
    {
        public int Count
        {
            get
            {
                int count = 18;
                for (int index = 0; index < TimedProgramCount; index++) count += TimedProgram(index).FrameCount;
                return count;
            }
        }
        public int Length => Count;
        public EnemyProjectilePresentationFrameDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                foreach (var frame in this) if (index-- == 0) return frame;
                throw new InvalidOperationException("Projectile visual enumeration count disagrees with its programs.");
            }
        }
        public IEnumerator<EnemyProjectilePresentationFrameDefinition> GetEnumerator()
        {
            for (int frame = 0; frame < 6; frame++)
                yield return new((ushort)(MotherBrainBlueRingInitial + frame * 8 + 6), $"mother_brain_blue_ring_{frame:D2}");
            for (int frame = 0; frame < 6; frame++)
                yield return new((ushort)(MotherBrainBlueRingTouch + frame * 4 + 6), $"mother_brain_blue_ring_impact_{frame:D2}");
            for (int index = 0; index < TimedProgramCount; index++)
            {
                var program = TimedProgram(index);
                if (program.InitialPointer == MotherBrainDroolFalling)
                {
                    for (int frame = 0; frame < 5; frame++)
                        yield return new((ushort)(MotherBrainDroolInitial + frame * 4 + 2), $"mother_brain_drool_attached_{frame:D2}");
                    yield return new(MotherBrainDroolInitial + 28, "mother_brain_drool_released");
                }
                int first = program.InitialPointer + (program.PrefixInstruction.HasValue ? 2 : 0);
                for (int frame = 0; frame < program.FrameCount; frame++)
                    yield return new((ushort)(first + frame * 4 + 2), $"projectile_86_{program.InitialPointer:X4}_frame_{frame:D2}".ToLowerInvariant());
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
