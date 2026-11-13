using SuperMetroid.TestSupport.MovieReplay;
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
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.SpecialCallableBlowUpBombTorizosGut, 0xb0e5),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.CallableBlowUpBombTorizosFace, 0xb155),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.DeathSequence0, 0xb1c8),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.DeathSequence1, 0xb1d2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.DeathSequence2, 0xb1e2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.BombTorizoInitial0, 0xb879),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.BombTorizoInitial0Resumed, 0xb887),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.BombTorizoInitial1, 0xb8c7),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.BombTorizoInitial2, 0xb935),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftTurningLeft, 0xb962),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftWalkingRightLegMoving, 0xb96c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftWalkingLeftLegMoving, 0xb9b6),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftSpewingChozoOrbsRightFootFwd0, 0xba04),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftSpewingChozoOrbsRightFootFwd1, 0xba26),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftSpewingChozoOrbsLeftFootFwd0, 0xba46),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftSpewingChozoOrbsLeftFootFwd1, 0xba68),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftSonicBoomsRightFootForward0, 0xba88),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftSonicBoomsRightFootForward1, 0xba90),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftSonicBoomsLeftFootForward0, 0xbaf2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftSonicBoomsLeftFootForward1, 0xbafa),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftExplosiveSwipeRightFootForward, 0xbb5c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftExplosiveSwipeLeftFootForward, 0xbbde),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftJumpingForwards0, 0xbc60),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftJumpingForwards1, 0xbc70),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftFalling0, 0xbc78),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftFalling1, 0xbc80),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftFalling2, 0xbc88),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftJumpingBackwardLandLeftFootFwd0, 0xbc96),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftJumpingBackwardLandLeftFootFwd1, 0xbca6),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftJumpingBackwardLandLeftFootFwd2, 0xbcae),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftJumpingBackwardLandLeftFootFwd3, 0xbcb6),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftJumpingBackwardLandLeftFootFwd4, 0xbcbe),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftJumpingBackwardRightFootFwd0, 0xbcd2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftJumpingBackwardRightFootFwd1, 0xbce2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftJumpingBackwardRightFootFwd2, 0xbcea),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftJumpingBackwardRightFootFwd3, 0xbcf2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftJumpingBackwardRightFootFwd4, 0xbcfa),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftFacelessTurningLeft, 0xbd0e),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftFacelessWalkingRightLegMoving, 0xbd18),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftFacelessWalkingLeftLegMoving, 0xbd52),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftFacelessWalkingLeftLegMovingTrailing, 0xbd90),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingLeftFacelessWalkingLeftLegMovingTrailingResumed, 0xbd9c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightTurningRight, 0xbdd8),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightWalkingLeftLegMoving, 0xbde2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightWalkingRightLegMoving, 0xbe30),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightSpewingChozoOrbsLeftFootFwd0, 0xbe7e),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightSpewingChozoOrbsLeftFootFwd1, 0xbea0),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightSpewingChozoOrbsRightFootFwd0, 0xbec0),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightSpewingChozoOrbsRightFootFwd1, 0xbee2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightSonicBoomsLeftFootForward0, 0xbf02),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightSonicBoomsLeftFootForward1, 0xbf0a),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightSonicBoomsRightFootForward0, 0xbf6c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightSonicBoomsRightFootForward1, 0xbf74),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightExplosiveSwipeLeftFootForward, 0xbfd6),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightExplosiveSwipeRightFootForward, 0xc058),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightJumpingForwards0, 0xc0da),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightJumpingForwards1, 0xc0ea),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightFalling0, 0xc0f2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightFalling1, 0xc0fa),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightFalling2, 0xc102),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightJumpBackwardLandRightFootFwd0, 0xc110),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightJumpBackwardLandRightFootFwd1, 0xc120),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightJumpBackwardLandRightFootFwd2, 0xc128),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightJumpBackwardLandRightFootFwd3, 0xc130),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightJumpBackwardLandRightFootFwd4, 0xc138),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightJumpBackwardsLandLeftFootFwd0, 0xc14c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightJumpBackwardsLandLeftFootFwd1, 0xc15c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightJumpBackwardsLandLeftFootFwd2, 0xc164),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightJumpBackwardsLandLeftFootFwd3, 0xc16c),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightJumpBackwardsLandLeftFootFwd4, 0xc174),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightFacelessTurningRight, 0xc188),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightFacelessWalkingLeftLegMoving, 0xc192),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.FacingRightFacelessWalkingRightLegMoving, 0xc1cc),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenInitial0, 0xc9cb),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenInitial0Resumed, 0xc9e2),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenInitial1, 0xc9e6),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenInitial2, 0xca48),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenInitial3, 0xcab6),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSpewChozoOrbsFaceLeftRightFootFwd0, 0xcaff),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSpewChozoOrbsFaceLeftRightFootFwd1, 0xcb1f),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0, 0xcb41),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1, 0xcb61),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSonicBoomsFacingLeftRightFootFwd0, 0xcb83),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSonicBoomsFacingLeftRightFootFwd1, 0xcb8b),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSonicBoomsFacingLeftLeftFootFwd0, 0xcbed),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSonicBoomsFacingLeftLeftFootFwd1, 0xcbf5),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSpewChozoOrbFacingLeftLeftFootFwd0, 0xcc57),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 0xcc77),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSpewChozoOrbFacingLeftRightFootFwd0, 0xcc99),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 0xccb9),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSonicBoomsFacingRightLeftFootFwd0, 0xccdb),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSonicBoomsFacingRightLeftFootFwd1, 0xcce3),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSonicBoomsFacingRightRightFootFwd0, 0xcd45),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSonicBoomsFacingRightRightFootFwd1, 0xcd4d),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd, 0xcdaf),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd, 0xcdb9),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenLandedFromBackwardsJumpFacingRightRightFootFwd, 0xcdc3),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd, 0xcdcd),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenCaughtSuperFacingLeftLeftLegFwd, 0xcde1),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenCaughtSuperFacingLeftRightLegFwd, 0xce43),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenCaughtSuperFacingRightRightLegFwd, 0xcea5),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenCaughtSuperFacingRightLeftLegFwd, 0xceff),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSitDownAttackFacingLeft, 0xcf59),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenSitDownAttackFacingRight, 0xcfc5),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenReleaseGoldenTorizoEggs0, 0xd031),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenReleaseGoldenTorizoEggs1, 0xd07b),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenReleaseGoldenTorizoEggs2, 0xd087),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenEyeBeamAttack0, 0xd10d),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenEyeBeamAttack1, 0xd11f),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenEyeBeamAttack2, 0xd133),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.Stunned0, 0xd193),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.Stunned1, 0xd1a1),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenDodgeTurningLeft, 0xd1f1),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenTurningLeft, 0xd203),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenWalkingLeftRightLegMoving, 0xd20d),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenWalkingLeftLeftLegMoving, 0xd259),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenDodgeTurningRight, 0xd2ad),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenTurningRight, 0xd2bf),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenWalkingRightLeftLegMoving, 0xd2c9),
        new TorizoNativeListAddressesBodyEntry(TorizoBodySegment.GoldenWalkingRightRightLegMoving, 0xd315),
    ];

    /// <summary>
    /// Each bank-$AA Torizo instruction the body lists use, keyed by its native routine: the cue it must become and
    /// what each operand word carries.
    /// </summary>
    internal static readonly Dictionary<ushort, TorizoNativeListAddressesBodyInstruction> BodyInstructions = new()
    {
        [0x806b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.SetFrameBehavior>(""), [TorizoOperand.FrameBehavior]),
        [0x8074] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.ClearFrameBehavior)), []),
        [0xb09c] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.SetPhase>(""), [TorizoOperand.Function]),
        [0xb11d] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.BlowUpGut)), []),
        [0xb1be] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.MarkFaceBlownUp)), []),
        [0xb224] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.MakeVisible)), []),
        [0xb22e] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.MakeInvisible)), []),
        [0xb238] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.FadeToBlack)), []),
        [0xb24d] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.FinishDeath)), []),
        [0xb271] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.AdvanceColourFade)), []),
        [0xb94d] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.FadeToNormalColors)), []),
        [0xb951] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.StartBombTorizoFight)), []),
        [0xc2c8] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.NoOperation)), []),
        [0xc2c9] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.LockAnimation)), []),
        [0xc2d1] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.UnlockAnimation)), []),
        [0xc2d9] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Choice>("", "Choice GoToIfFaceBlownUpElseIfGolden", cue => cue.Kind == TorizoChoiceKind.GoToIfFaceBlownUpElseIfGolden), [TorizoOperand.Target, TorizoOperand.Alternate]),
        [0xc2ed] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch Link", cue => cue.Kind == TorizoBranchKind.Link), [TorizoOperand.Target]),
        [0xc2f7] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.Return)), []),
        [0xc2fd] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.ResumeInterrupted)), []),
        [0xc303] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.LowHealthExplosions>(""), [TorizoOperand.Value]),
        [0xc32f] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnDeathExplosion)), []),
        [0xc34a] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnLandingDust)), []),
        [0xc35b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnLowHealthDrool)), []),
        [0xc36d] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SetTurningAround)), []),
        [0xc377] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SteppedLeftWithLeftFoot)), []),
        [0xc38a] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SteppedRightWithRightFoot)), []),
        [0xc3a0] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SteppedLeftWithRightFoot)), []),
        [0xc3b6] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SteppedRightWithLeftFoot)), []),
        [0xc3cc] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Posture>("", "Posture StandUp", cue => cue.Kind == TorizoPostureKind.StandUp), [TorizoOperand.Value]),
        [0xc41e] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Posture>("", "Posture SitDown", cue => cue.Kind == TorizoPostureKind.SitDown), [TorizoOperand.Value]),
        [0xc470] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Walk>("", "Walk WalkBombTorizo", cue => cue.Kind == TorizoWalkKind.WalkBombTorizo), [TorizoOperand.Value]),
        [0xc4e5] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Walk>("", "Walk WalkFacelessBombTorizo", cue => cue.Kind == TorizoWalkKind.WalkFacelessBombTorizo), [TorizoOperand.Value]),
        [0xc55a] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfRising", cue => cue.Kind == TorizoBranchKind.GoToIfRising), [TorizoOperand.Target]),
        [0xc567] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallIfSamusCloseBehind", cue => cue.Kind == TorizoBranchKind.CallIfSamusCloseBehind), [TorizoOperand.Target]),
        [0xc58b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch JumpBackIfSamusCloseInFront", cue => cue.Kind == TorizoBranchKind.JumpBackIfSamusCloseInFront), [TorizoOperand.Target]),
        [0xc5a4] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Choice>("", "Choice CallBombTorizoAttack", cue => cue.Kind == TorizoChoiceKind.CallBombTorizoAttack), [TorizoOperand.Target, TorizoOperand.Alternate]),
        [0xc5cb] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnBombTorizoChozoOrbs)), []),
        [0xc5e3] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnBombTorizoSonicBoom", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom), [TorizoOperand.Value]),
        [0xc5f2] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnGoldenTorizoSonicBoom", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnGoldenTorizoSonicBoom), [TorizoOperand.Value]),
        [0xc601] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnExplosiveSwipe", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnExplosiveSwipe), [TorizoOperand.Value]),
        [0xc610] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.PlayShotSound)), []),
        [0xc618] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.PlayFootstepSound)), []),
        [0xcace] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfAirborne", cue => cue.Kind == TorizoBranchKind.GoToIfAirborne), [TorizoOperand.Target]),
        [0xcade] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.LoadGoldenTorizoPalettes)), []),
        [0xcae2] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.StartGoldenTorizoFight)), []),
        [0xcdd7] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.ClearCaughtSuperMissile)), []),
        [0xd0e9] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnEgg)), []),
        [0xd0f3] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfEyeBeamActive", cue => cue.Kind == TorizoBranchKind.GoToIfEyeBeamActive), [TorizoOperand.Target]),
        [0xd17b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.DisableEyeBeamExplosions)), []),
        [0xd187] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.EnableEyeBeamExplosions)), []),
        [0xd1e7] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.UnmarkStunned)), []),
        [0xd38f] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.PlayEggReleasedSound)), []),
        [0xd397] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.PlayLaserSound)), []),
        [0xd39f] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.PlaySonicBoomSound)), []),
        [0xd3e0] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnSuperMissile)), []),
        [0xd3ea] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfSamusMorphedBehind", cue => cue.Kind == TorizoBranchKind.GoToIfSamusMorphedBehind), [TorizoOperand.Target]),
        [0xd436] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnEyeBeam", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnEyeBeam), [TorizoOperand.Value]),
        [0xd445] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallSometimesIfSamusMorphedInFront", cue => cue.Kind == TorizoBranchKind.CallSometimesIfSamusMorphedInFront), [TorizoOperand.Target]),
        [0xd474] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallSometimesIfHealthLow", cue => cue.Kind == TorizoBranchKind.CallSometimesIfHealthLow), [TorizoOperand.Target]),
        [0xd49b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallIfStunHealthHigh", cue => cue.Kind == TorizoBranchKind.CallIfStunHealthHigh), [TorizoOperand.Target]),
        [0xd4ba] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch JumpForwardIfSamusFar", cue => cue.Kind == TorizoBranchKind.JumpForwardIfSamusFar), [TorizoOperand.Target]),
        [0xd4f3] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnGoldenTorizoChozoOrbs)), []),
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
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.ExplosionLowHealthInitial, 0xa3cb),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.ExplosionLowHealthInitialLoop, 0xa3d5),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.ExplosionDeathInitial, 0xa3fa),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.ExplosionDeathInitialLoop, 0xa408),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.ExplosionDeathSmokeSetup, 0xa431),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.ExplosionDeathSmokeSetupLoop, 0xa435),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.DroolFourFrameDelay, 0xa46a),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.DroolTwoFrameDelay, 0xa46e),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.DroolNoDelay, 0xa472),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.DroolFallingLoop, 0xa482),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.DroolWallImpact, 0xa48a),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.DroolFloorImpact, 0xa48e),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.SwipeInitial, 0xa4aa),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment0, 0xa4c3),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment1, 0xa4d4),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment2, 0xa4e5),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment3, 0xa4f6),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment4, 0xa507),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment5, 0xa518),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment6, 0xa529),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment7, 0xa53a),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment8, 0xa54b),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment9, 0xa55c),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment10, 0xa56d),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment11, 0xa57e),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment12, 0xa58f),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment13, 0xa5a0),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment14, 0xa5b1),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.StatueFragment15, 0xa5c2),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.ChozoOrbMovingLeft, 0xab15),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.ChozoOrbMovingRight, 0xab1d),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.ChozoOrbWallImpact, 0xab25),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.ChozoOrbFloorImpact, 0xab41),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.ChozoOrbShot, 0xab68),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.SonicBoomFiredLeft, 0xadbf),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.SonicBoomMovingLeft, 0xadca),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.SonicBoomFiredRight, 0xadd2),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.SonicBoomMovingRight, 0xaddd),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.SonicBoomWallImpact, 0xade5),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.SonicBoomWallImpactLoop, 0xadf1),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.LandingDustRightFoot, 0xaf9d),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.LandingDustLeftFoot, 0xafb5),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EggBouncingLeft, 0xb104),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EggBouncingLeftHatching, 0xb10a),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EggBouncingRight, 0xb11c),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EggBouncingRightHatching, 0xb122),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EggHatch, 0xb134),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EggHatchedLeft, 0xb14b),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EggHatchedLeftLoop, 0xb152),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EggHatchedRight, 0xb166),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EggHatchedRightLoop, 0xb16d),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EggBreakLeft, 0xb190),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EggBreakRight, 0xb1a8),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.SuperMissileRightInitial, 0xb293),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.SuperMissileRightLoop, 0xb29d),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.SuperMissileLeftInitial, 0xb2c1),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.SuperMissileLeftLoop, 0xb2cb),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.SuperMissileImpact, 0xb2ef),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EyeBeamWallImpact, 0xb3cd),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EyeBeamFloorImpact, 0xb3e5),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EyeBeamFloorImpactLoop, 0xb3e7),
        new TorizoNativeListAddressesProjectileEntry(TorizoProjectileSegment.EyeBeamNormal, 0xb410),
    ];

    /// <summary>The native entry point of <paramref name="program"/>.</summary>
    internal static ushort Entry(TorizoBodySegment program) =>
        Array.Find(BodyEntries, entry => entry.Program == program) is { Entry: not 0 } found
            ? found.Entry
            : throw new ArgumentOutOfRangeException(nameof(program), program, "No native Torizo list starts this program.");

    /// <summary>The native entry point of projectile program <paramref name="program"/>.</summary>
    internal static ushort ProjectileEntry(TorizoProjectileSegment program) =>
        Array.Find(ProjectileEntries, entry => entry.Program == program) is { Entry: not 0 } found
            ? found.Entry
            : throw new ArgumentOutOfRangeException(nameof(program), program, "No native Torizo list starts this projectile program.");

    /// <summary>The projectile program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static TorizoProjectileSegment? ProjectileProgramAt(ushort address) =>
        Array.Find(ProjectileEntries, entry => entry.Entry == address) is { Entry: not 0 } found ? found.Program : null;

    /// <summary>The body program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static TorizoBodySegment? ProgramAt(ushort address) =>
        Array.Find(BodyEntries, entry => entry.Entry == address) is { Entry: not 0 } found ? found.Program : null;
}
