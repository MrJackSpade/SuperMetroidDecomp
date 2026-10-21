using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// The native bank-$AA lists the Torizo ROM oracle (#1289) walks: each body program's entry point and the bank-$AA
/// Torizo instructions the lists use, with the cue each must become and the operand words it carries. Only
/// verification reads them; the port runs typed programs.
/// </summary>
internal static class TorizoNativeListAddresses
{
    /// <summary>The low-health drool's eight-entry list table at $86:A64D: each entry, selected by random bits 2-4, and its word.</summary>
    internal static readonly DroolDelayListTableValue[] DroolDelayListTable =
    [
        new DroolDelayListTableValue(0, 0x86a64d), new DroolDelayListTableValue(1, 0x86a64f), new DroolDelayListTableValue(2, 0x86a651), new DroolDelayListTableValue(3, 0x86a653),
        new DroolDelayListTableValue(4, 0x86a655), new DroolDelayListTableValue(5, 0x86a657), new DroolDelayListTableValue(6, 0x86a659), new DroolDelayListTableValue(7, 0x86a65b),
    ];

    /// <summary>Torizo's instruction bank, $AA.</summary>
    internal const byte BodyBankNumber = 0xaa;

    /// <summary>Each typed body program's native entry point in bank $AA, in authored order.</summary>
    internal static readonly TorizoNativeListAddressesBodyEntry[] BodyEntries =
    [
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.SpecialCallableBlowUpBombTorizosGut, 0xb0e5),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.CallableBlowUpBombTorizosFace, 0xb155),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.DeathSequence0, 0xb1c8),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.DeathSequence1, 0xb1d2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.DeathSequence2, 0xb1e2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.BombTorizoInitial0, 0xb879),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.BombTorizoInitial0Resumed, 0xb887),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.BombTorizoInitial1, 0xb8c7),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.BombTorizoInitial2, 0xb935),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftTurningLeft, 0xb962),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftWalkingRightLegMoving, 0xb96c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftWalkingLeftLegMoving, 0xb9b6),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftSpewingChozoOrbsRightFootFwd0, 0xba04),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftSpewingChozoOrbsRightFootFwd1, 0xba26),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftSpewingChozoOrbsLeftFootFwd0, 0xba46),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftSpewingChozoOrbsLeftFootFwd1, 0xba68),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftSonicBoomsRightFootForward0, 0xba88),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftSonicBoomsRightFootForward1, 0xba90),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftSonicBoomsLeftFootForward0, 0xbaf2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftSonicBoomsLeftFootForward1, 0xbafa),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftExplosiveSwipeRightFootForward, 0xbb5c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftExplosiveSwipeLeftFootForward, 0xbbde),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftJumpingForwards0, 0xbc60),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftJumpingForwards1, 0xbc70),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftFalling0, 0xbc78),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftFalling1, 0xbc80),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftFalling2, 0xbc88),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd0, 0xbc96),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd1, 0xbca6),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd2, 0xbcae),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd3, 0xbcb6),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd4, 0xbcbe),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd0, 0xbcd2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd1, 0xbce2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd2, 0xbcea),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd3, 0xbcf2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd4, 0xbcfa),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftFacelessTurningLeft, 0xbd0e),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftFacelessWalkingRightLegMoving, 0xbd18),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftFacelessWalkingLeftLegMoving, 0xbd52),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftFacelessWalkingLeftLegMovingTrailing, 0xbd90),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingLeftFacelessWalkingLeftLegMovingTrailingResumed, 0xbd9c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightTurningRight, 0xbdd8),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightWalkingLeftLegMoving, 0xbde2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightWalkingRightLegMoving, 0xbe30),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightSpewingChozoOrbsLeftFootFwd0, 0xbe7e),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightSpewingChozoOrbsLeftFootFwd1, 0xbea0),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightSpewingChozoOrbsRightFootFwd0, 0xbec0),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightSpewingChozoOrbsRightFootFwd1, 0xbee2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightSonicBoomsLeftFootForward0, 0xbf02),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightSonicBoomsLeftFootForward1, 0xbf0a),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightSonicBoomsRightFootForward0, 0xbf6c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightSonicBoomsRightFootForward1, 0xbf74),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightExplosiveSwipeLeftFootForward, 0xbfd6),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightExplosiveSwipeRightFootForward, 0xc058),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightJumpingForwards0, 0xc0da),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightJumpingForwards1, 0xc0ea),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightFalling0, 0xc0f2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightFalling1, 0xc0fa),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightFalling2, 0xc102),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd0, 0xc110),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd1, 0xc120),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd2, 0xc128),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd3, 0xc130),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd4, 0xc138),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd0, 0xc14c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd1, 0xc15c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd2, 0xc164),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd3, 0xc16c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd4, 0xc174),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightFacelessTurningRight, 0xc188),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightFacelessWalkingLeftLegMoving, 0xc192),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.FacingRightFacelessWalkingRightLegMoving, 0xc1cc),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenInitial0, 0xc9cb),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenInitial0Resumed, 0xc9e2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenInitial1, 0xc9e6),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenInitial2, 0xca48),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenInitial3, 0xcab6),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSpewChozoOrbsFaceLeftRightFootFwd0, 0xcaff),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSpewChozoOrbsFaceLeftRightFootFwd1, 0xcb1f),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0, 0xcb41),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1, 0xcb61),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSonicBoomsFacingLeftRightFootFwd0, 0xcb83),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSonicBoomsFacingLeftRightFootFwd1, 0xcb8b),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSonicBoomsFacingLeftLeftFootFwd0, 0xcbed),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSonicBoomsFacingLeftLeftFootFwd1, 0xcbf5),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftLeftFootFwd0, 0xcc57),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 0xcc77),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftRightFootFwd0, 0xcc99),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 0xccb9),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSonicBoomsFacingRightLeftFootFwd0, 0xccdb),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSonicBoomsFacingRightLeftFootFwd1, 0xcce3),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSonicBoomsFacingRightRightFootFwd0, 0xcd45),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSonicBoomsFacingRightRightFootFwd1, 0xcd4d),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd, 0xcdaf),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd, 0xcdb9),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenLandedFromBackwardsJumpFacingRightRightFootFwd, 0xcdc3),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd, 0xcdcd),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenCaughtSuperFacingLeftLeftLegFwd, 0xcde1),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenCaughtSuperFacingLeftRightLegFwd, 0xce43),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenCaughtSuperFacingRightRightLegFwd, 0xcea5),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenCaughtSuperFacingRightLeftLegFwd, 0xceff),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSitDownAttackFacingLeft, 0xcf59),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenSitDownAttackFacingRight, 0xcfc5),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenReleaseGoldenTorizoEggs0, 0xd031),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenReleaseGoldenTorizoEggs1, 0xd07b),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenReleaseGoldenTorizoEggs2, 0xd087),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenEyeBeamAttack0, 0xd10d),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenEyeBeamAttack1, 0xd11f),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenEyeBeamAttack2, 0xd133),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.Stunned0, 0xd193),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.Stunned1, 0xd1a1),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenDodgeTurningLeft, 0xd1f1),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenTurningLeft, 0xd203),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenWalkingLeftRightLegMoving, 0xd20d),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenWalkingLeftLeftLegMoving, 0xd259),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenDodgeTurningRight, 0xd2ad),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenTurningRight, 0xd2bf),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenWalkingRightLeftLegMoving, 0xd2c9),
        new TorizoNativeListAddressesBodyEntry(TorizoBodyProgram.GoldenWalkingRightRightLegMoving, 0xd315),
    ];

    /// <summary>
    /// Each bank-$AA Torizo instruction the body lists use, keyed by its native routine: the cue it must become and
    /// what each operand word carries.
    /// </summary>
    internal static readonly Dictionary<ushort, TorizoNativeListAddressesBodyInstruction> BodyInstructions = new()
    {
        [0x806b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.SetPreInstruction>(""), [TorizoOperand.PreInstruction]),
        [0x8074] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.ClearPreInstruction)), []),
        [0xb09c] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.SetFunction>(""), [TorizoOperand.Function]),
        [0xb11d] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.BlowUpGut)), []),
        [0xb1be] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.MarkFaceBlownUp)), []),
        [0xb224] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.MakeVisible)), []),
        [0xb22e] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.MakeInvisible)), []),
        [0xb238] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.FadeToBlack)), []),
        [0xb24d] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.FinishDeath)), []),
        [0xb271] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.AdvancePaletteFade)), []),
        [0xb94d] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.FadeToNormalColors)), []),
        [0xb951] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.StartBombTorizoFight)), []),
        [0xc2c8] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.NoOperation)), []),
        [0xc2c9] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.LockAnimation)), []),
        [0xc2d1] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.UnlockAnimation)), []),
        [0xc2d9] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Choice>("", "Choice GoToIfFaceBlownUpElseIfGolden", cue => cue.Kind == TorizoChoiceKind.GoToIfFaceBlownUpElseIfGolden), [TorizoOperand.Target, TorizoOperand.Alternate]),
        [0xc2ed] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch Link", cue => cue.Kind == TorizoBranchKind.Link), [TorizoOperand.Target]),
        [0xc2f7] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.Return)), []),
        [0xc2fd] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.ResumeInterrupted)), []),
        [0xc303] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnLowHealthExplosions", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnLowHealthExplosions), [TorizoOperand.Value]),
        [0xc32f] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnDeathExplosion)), []),
        [0xc34a] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnLandingDust)), []),
        [0xc35b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnLowHealthDrool)), []),
        [0xc36d] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SetTurningAround)), []),
        [0xc377] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SteppedLeftWithLeftFoot)), []),
        [0xc38a] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SteppedRightWithRightFoot)), []),
        [0xc3a0] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SteppedLeftWithRightFoot)), []),
        [0xc3b6] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SteppedRightWithLeftFoot)), []),
        [0xc3cc] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Posture>("", "Posture StandUp", cue => cue.Kind == TorizoPostureKind.StandUp), [TorizoOperand.Value]),
        [0xc41e] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Posture>("", "Posture SitDown", cue => cue.Kind == TorizoPostureKind.SitDown), [TorizoOperand.Value]),
        [0xc470] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Walk>("", "Walk WalkBombTorizo", cue => cue.Kind == TorizoWalkKind.WalkBombTorizo), [TorizoOperand.Value]),
        [0xc4e5] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Walk>("", "Walk WalkFacelessBombTorizo", cue => cue.Kind == TorizoWalkKind.WalkFacelessBombTorizo), [TorizoOperand.Value]),
        [0xc55a] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfRising", cue => cue.Kind == TorizoBranchKind.GoToIfRising), [TorizoOperand.Target]),
        [0xc567] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallIfSamusCloseBehind", cue => cue.Kind == TorizoBranchKind.CallIfSamusCloseBehind), [TorizoOperand.Target]),
        [0xc58b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch JumpBackIfSamusCloseInFront", cue => cue.Kind == TorizoBranchKind.JumpBackIfSamusCloseInFront), [TorizoOperand.Target]),
        [0xc5a4] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Choice>("", "Choice CallBombTorizoAttack", cue => cue.Kind == TorizoChoiceKind.CallBombTorizoAttack), [TorizoOperand.Target, TorizoOperand.Alternate]),
        [0xc5cb] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnBombTorizoChozoOrbs)), []),
        [0xc5e3] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnBombTorizoSonicBoom", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom), [TorizoOperand.Value]),
        [0xc5f2] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnGoldenTorizoSonicBoom", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnGoldenTorizoSonicBoom), [TorizoOperand.Value]),
        [0xc601] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnExplosiveSwipe", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnExplosiveSwipe), [TorizoOperand.Value]),
        [0xc610] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.PlayShotSound)), []),
        [0xc618] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.PlayFootstepSound)), []),
        [0xcace] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfAirborne", cue => cue.Kind == TorizoBranchKind.GoToIfAirborne), [TorizoOperand.Target]),
        [0xcade] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.LoadGoldenTorizoPalettes)), []),
        [0xcae2] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.StartGoldenTorizoFight)), []),
        [0xcdd7] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.ClearCaughtSuperMissile)), []),
        [0xd0e9] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnEgg)), []),
        [0xd0f3] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfEyeBeamActive", cue => cue.Kind == TorizoBranchKind.GoToIfEyeBeamActive), [TorizoOperand.Target]),
        [0xd17b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.DisableEyeBeamExplosions)), []),
        [0xd187] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.EnableEyeBeamExplosions)), []),
        [0xd1e7] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.UnmarkStunned)), []),
        [0xd38f] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.PlayEggReleasedSound)), []),
        [0xd397] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.PlayLaserSound)), []),
        [0xd39f] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.PlaySonicBoomSound)), []),
        [0xd3e0] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnSuperMissile)), []),
        [0xd3ea] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfSamusMorphedBehind", cue => cue.Kind == TorizoBranchKind.GoToIfSamusMorphedBehind), [TorizoOperand.Target]),
        [0xd436] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnEyeBeam", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnEyeBeam), [TorizoOperand.Value]),
        [0xd445] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallSometimesIfSamusMorphedInFront", cue => cue.Kind == TorizoBranchKind.CallSometimesIfSamusMorphedInFront), [TorizoOperand.Target]),
        [0xd474] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallSometimesIfHealthLow", cue => cue.Kind == TorizoBranchKind.CallSometimesIfHealthLow), [TorizoOperand.Target]),
        [0xd49b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallIfStunHealthHigh", cue => cue.Kind == TorizoBranchKind.CallIfStunHealthHigh), [TorizoOperand.Target]),
        [0xd4ba] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch JumpForwardIfSamusFar", cue => cue.Kind == TorizoBranchKind.JumpForwardIfSamusFar), [TorizoOperand.Target]),
        [0xd4f3] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnGoldenTorizoChozoOrbs)), []),
        [0xd4fd] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch JumpBackIfSamusCloseInFrontGolden", cue => cue.Kind == TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden), [TorizoOperand.Target]),
        [0xd526] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Choice>("", "Choice CallGoldenTorizoAttack", cue => cue.Kind == TorizoChoiceKind.CallGoldenTorizoAttack), [TorizoOperand.Target, TorizoOperand.Alternate]),
        [0xd54d] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Walk>("", "Walk WalkGoldenTorizo", cue => cue.Kind == TorizoWalkKind.WalkGoldenTorizo), [TorizoOperand.Value]),
    };

    /// <summary>What a native Torizo list word does: the shared kinds, plus the common $814B tile transfer.</summary>
    internal enum BodyWordKind
    {
        /// <summary>A word the shared reader decodes.</summary>
        Shared,
        /// <summary>The common $814B tile transfer, followed by its seven-byte descriptor.</summary>
        TileTransfer,
    }

    /// <summary>The common commands Torizo's lists use beyond the shared reader's.</summary>
    private static readonly Dictionary<ushort, BodyWordKind> BodyWords = new()
    {
        [CommonEnemyInstructionConstants.CopyToVram] = BodyWordKind.TileTransfer,
    };

    /// <summary>Decodes a native Torizo list word.</summary>
    internal static BodyWordKind ClassifyBodyWord(ushort word) =>
        BodyWords.TryGetValue(word, out BodyWordKind kind) ? kind : BodyWordKind.Shared;

    /// <summary>Each typed projectile program's native entry point in bank $86, in authored order.</summary>
    internal static readonly TorizoNativeListAddressesProjectileEntry[] ProjectileEntries =
    [
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.ExplosionLowHealthInitial, 0xa3cb),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.ExplosionLowHealthInitialLoop, 0xa3d5),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.ExplosionDeathInitial, 0xa3fa),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.ExplosionDeathInitialLoop, 0xa408),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.ExplosionDeathSmokeSetup, 0xa431),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.ExplosionDeathSmokeSetupLoop, 0xa435),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.DroolFourFrameDelay, 0xa46a),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.DroolTwoFrameDelay, 0xa46e),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.DroolNoDelay, 0xa472),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.DroolFallingLoop, 0xa482),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.DroolWallImpact, 0xa48a),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.DroolFloorImpact, 0xa48e),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.SwipeInitial, 0xa4aa),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment0, 0xa4c3),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment1, 0xa4d4),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment2, 0xa4e5),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment3, 0xa4f6),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment4, 0xa507),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment5, 0xa518),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment6, 0xa529),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment7, 0xa53a),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment8, 0xa54b),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment9, 0xa55c),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment10, 0xa56d),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment11, 0xa57e),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment12, 0xa58f),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment13, 0xa5a0),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment14, 0xa5b1),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.StatueFragment15, 0xa5c2),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.ChozoOrbMovingLeft, 0xab15),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.ChozoOrbMovingRight, 0xab1d),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.ChozoOrbWallImpact, 0xab25),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.ChozoOrbFloorImpact, 0xab41),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.ChozoOrbShot, 0xab68),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.SonicBoomFiredLeft, 0xadbf),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.SonicBoomMovingLeft, 0xadca),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.SonicBoomFiredRight, 0xadd2),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.SonicBoomMovingRight, 0xaddd),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.SonicBoomWallImpact, 0xade5),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.SonicBoomWallImpactLoop, 0xadf1),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.LandingDustRightFoot, 0xaf9d),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.LandingDustLeftFoot, 0xafb5),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EggBouncingLeft, 0xb104),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EggBouncingLeftHatching, 0xb10a),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EggBouncingRight, 0xb11c),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EggBouncingRightHatching, 0xb122),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EggHatch, 0xb134),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EggHatchedLeft, 0xb14b),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EggHatchedLeftLoop, 0xb152),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EggHatchedRight, 0xb166),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EggHatchedRightLoop, 0xb16d),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EggBreakLeft, 0xb190),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EggBreakRight, 0xb1a8),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.SuperMissileRightInitial, 0xb293),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.SuperMissileRightLoop, 0xb29d),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.SuperMissileLeftInitial, 0xb2c1),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.SuperMissileLeftLoop, 0xb2cb),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.SuperMissileImpact, 0xb2ef),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EyeBeamWallImpact, 0xb3cd),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EyeBeamFloorImpact, 0xb3e5),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EyeBeamFloorImpactLoop, 0xb3e7),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram.EyeBeamNormal, 0xb410),
    ];

    /// <summary>The native entry point of <paramref name="program"/>.</summary>
    internal static ushort Entry(TorizoBodyProgram program) =>
        Array.Find(BodyEntries, entry => entry.Program == program) is { Entry: not 0 } found
            ? found.Entry
            : throw new ArgumentOutOfRangeException(nameof(program), program, "No native Torizo list starts this program.");

    /// <summary>The native entry point of projectile program <paramref name="program"/>.</summary>
    internal static ushort ProjectileEntry(TorizoProjectileProgram program) =>
        Array.Find(ProjectileEntries, entry => entry.Program == program) is { Entry: not 0 } found
            ? found.Entry
            : throw new ArgumentOutOfRangeException(nameof(program), program, "No native Torizo list starts this projectile program.");

    /// <summary>The projectile program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static TorizoProjectileProgram? ProjectileProgramAt(ushort address) =>
        Array.Find(ProjectileEntries, entry => entry.Entry == address) is { Entry: not 0 } found ? found.Program : null;

    /// <summary>The body program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static TorizoBodyProgram? ProgramAt(ushort address) =>
        Array.Find(BodyEntries, entry => entry.Entry == address) is { Entry: not 0 } found ? found.Program : null;
}
