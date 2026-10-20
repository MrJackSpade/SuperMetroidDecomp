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
    internal static readonly (int Entry, int Word)[] DroolDelayListTable =
    [
        (0, 0x86a64d), (1, 0x86a64f), (2, 0x86a651), (3, 0x86a653),
        (4, 0x86a655), (5, 0x86a657), (6, 0x86a659), (7, 0x86a65b),
    ];

    /// <summary>Torizo's instruction bank, $AA.</summary>
    internal const byte BodyBankNumber = 0xaa;

    /// <summary>Each typed body program's native entry point in bank $AA, in authored order.</summary>
    internal static readonly (TorizoBodyProgram Program, ushort Entry)[] BodyEntries =
    [
        (TorizoBodyProgram.SpecialCallableBlowUpBombTorizosGut, 0xb0e5),
        (TorizoBodyProgram.CallableBlowUpBombTorizosFace, 0xb155),
        (TorizoBodyProgram.DeathSequence0, 0xb1c8),
        (TorizoBodyProgram.DeathSequence1, 0xb1d2),
        (TorizoBodyProgram.DeathSequence2, 0xb1e2),
        (TorizoBodyProgram.BombTorizoInitial0, 0xb879),
        (TorizoBodyProgram.BombTorizoInitial0Resumed, 0xb887),
        (TorizoBodyProgram.BombTorizoInitial1, 0xb8c7),
        (TorizoBodyProgram.BombTorizoInitial2, 0xb935),
        (TorizoBodyProgram.FacingLeftTurningLeft, 0xb962),
        (TorizoBodyProgram.FacingLeftWalkingRightLegMoving, 0xb96c),
        (TorizoBodyProgram.FacingLeftWalkingLeftLegMoving, 0xb9b6),
        (TorizoBodyProgram.FacingLeftSpewingChozoOrbsRightFootFwd0, 0xba04),
        (TorizoBodyProgram.FacingLeftSpewingChozoOrbsRightFootFwd1, 0xba26),
        (TorizoBodyProgram.FacingLeftSpewingChozoOrbsLeftFootFwd0, 0xba46),
        (TorizoBodyProgram.FacingLeftSpewingChozoOrbsLeftFootFwd1, 0xba68),
        (TorizoBodyProgram.FacingLeftSonicBoomsRightFootForward0, 0xba88),
        (TorizoBodyProgram.FacingLeftSonicBoomsRightFootForward1, 0xba90),
        (TorizoBodyProgram.FacingLeftSonicBoomsLeftFootForward0, 0xbaf2),
        (TorizoBodyProgram.FacingLeftSonicBoomsLeftFootForward1, 0xbafa),
        (TorizoBodyProgram.FacingLeftExplosiveSwipeRightFootForward, 0xbb5c),
        (TorizoBodyProgram.FacingLeftExplosiveSwipeLeftFootForward, 0xbbde),
        (TorizoBodyProgram.FacingLeftJumpingForwards0, 0xbc60),
        (TorizoBodyProgram.FacingLeftJumpingForwards1, 0xbc70),
        (TorizoBodyProgram.FacingLeftFalling0, 0xbc78),
        (TorizoBodyProgram.FacingLeftFalling1, 0xbc80),
        (TorizoBodyProgram.FacingLeftFalling2, 0xbc88),
        (TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd0, 0xbc96),
        (TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd1, 0xbca6),
        (TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd2, 0xbcae),
        (TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd3, 0xbcb6),
        (TorizoBodyProgram.FacingLeftJumpingBackwardLandLeftFootFwd4, 0xbcbe),
        (TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd0, 0xbcd2),
        (TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd1, 0xbce2),
        (TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd2, 0xbcea),
        (TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd3, 0xbcf2),
        (TorizoBodyProgram.FacingLeftJumpingBackwardRightFootFwd4, 0xbcfa),
        (TorizoBodyProgram.FacingLeftFacelessTurningLeft, 0xbd0e),
        (TorizoBodyProgram.FacingLeftFacelessWalkingRightLegMoving, 0xbd18),
        (TorizoBodyProgram.FacingLeftFacelessWalkingLeftLegMoving, 0xbd52),
        (TorizoBodyProgram.FacingLeftFacelessWalkingLeftLegMovingTrailing, 0xbd90),
        (TorizoBodyProgram.FacingLeftFacelessWalkingLeftLegMovingTrailingResumed, 0xbd9c),
        (TorizoBodyProgram.FacingRightTurningRight, 0xbdd8),
        (TorizoBodyProgram.FacingRightWalkingLeftLegMoving, 0xbde2),
        (TorizoBodyProgram.FacingRightWalkingRightLegMoving, 0xbe30),
        (TorizoBodyProgram.FacingRightSpewingChozoOrbsLeftFootFwd0, 0xbe7e),
        (TorizoBodyProgram.FacingRightSpewingChozoOrbsLeftFootFwd1, 0xbea0),
        (TorizoBodyProgram.FacingRightSpewingChozoOrbsRightFootFwd0, 0xbec0),
        (TorizoBodyProgram.FacingRightSpewingChozoOrbsRightFootFwd1, 0xbee2),
        (TorizoBodyProgram.FacingRightSonicBoomsLeftFootForward0, 0xbf02),
        (TorizoBodyProgram.FacingRightSonicBoomsLeftFootForward1, 0xbf0a),
        (TorizoBodyProgram.FacingRightSonicBoomsRightFootForward0, 0xbf6c),
        (TorizoBodyProgram.FacingRightSonicBoomsRightFootForward1, 0xbf74),
        (TorizoBodyProgram.FacingRightExplosiveSwipeLeftFootForward, 0xbfd6),
        (TorizoBodyProgram.FacingRightExplosiveSwipeRightFootForward, 0xc058),
        (TorizoBodyProgram.FacingRightJumpingForwards0, 0xc0da),
        (TorizoBodyProgram.FacingRightJumpingForwards1, 0xc0ea),
        (TorizoBodyProgram.FacingRightFalling0, 0xc0f2),
        (TorizoBodyProgram.FacingRightFalling1, 0xc0fa),
        (TorizoBodyProgram.FacingRightFalling2, 0xc102),
        (TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd0, 0xc110),
        (TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd1, 0xc120),
        (TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd2, 0xc128),
        (TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd3, 0xc130),
        (TorizoBodyProgram.FacingRightJumpBackwardLandRightFootFwd4, 0xc138),
        (TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd0, 0xc14c),
        (TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd1, 0xc15c),
        (TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd2, 0xc164),
        (TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd3, 0xc16c),
        (TorizoBodyProgram.FacingRightJumpBackwardsLandLeftFootFwd4, 0xc174),
        (TorizoBodyProgram.FacingRightFacelessTurningRight, 0xc188),
        (TorizoBodyProgram.FacingRightFacelessWalkingLeftLegMoving, 0xc192),
        (TorizoBodyProgram.FacingRightFacelessWalkingRightLegMoving, 0xc1cc),
        (TorizoBodyProgram.GoldenInitial0, 0xc9cb),
        (TorizoBodyProgram.GoldenInitial0Resumed, 0xc9e2),
        (TorizoBodyProgram.GoldenInitial1, 0xc9e6),
        (TorizoBodyProgram.GoldenInitial2, 0xca48),
        (TorizoBodyProgram.GoldenInitial3, 0xcab6),
        (TorizoBodyProgram.GoldenSpewChozoOrbsFaceLeftRightFootFwd0, 0xcaff),
        (TorizoBodyProgram.GoldenSpewChozoOrbsFaceLeftRightFootFwd1, 0xcb1f),
        (TorizoBodyProgram.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0, 0xcb41),
        (TorizoBodyProgram.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1, 0xcb61),
        (TorizoBodyProgram.GoldenSonicBoomsFacingLeftRightFootFwd0, 0xcb83),
        (TorizoBodyProgram.GoldenSonicBoomsFacingLeftRightFootFwd1, 0xcb8b),
        (TorizoBodyProgram.GoldenSonicBoomsFacingLeftLeftFootFwd0, 0xcbed),
        (TorizoBodyProgram.GoldenSonicBoomsFacingLeftLeftFootFwd1, 0xcbf5),
        (TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftLeftFootFwd0, 0xcc57),
        (TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 0xcc77),
        (TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftRightFootFwd0, 0xcc99),
        (TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 0xccb9),
        (TorizoBodyProgram.GoldenSonicBoomsFacingRightLeftFootFwd0, 0xccdb),
        (TorizoBodyProgram.GoldenSonicBoomsFacingRightLeftFootFwd1, 0xcce3),
        (TorizoBodyProgram.GoldenSonicBoomsFacingRightRightFootFwd0, 0xcd45),
        (TorizoBodyProgram.GoldenSonicBoomsFacingRightRightFootFwd1, 0xcd4d),
        (TorizoBodyProgram.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd, 0xcdaf),
        (TorizoBodyProgram.GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd, 0xcdb9),
        (TorizoBodyProgram.GoldenLandedFromBackwardsJumpFacingRightRightFootFwd, 0xcdc3),
        (TorizoBodyProgram.GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd, 0xcdcd),
        (TorizoBodyProgram.GoldenCaughtSuperFacingLeftLeftLegFwd, 0xcde1),
        (TorizoBodyProgram.GoldenCaughtSuperFacingLeftRightLegFwd, 0xce43),
        (TorizoBodyProgram.GoldenCaughtSuperFacingRightRightLegFwd, 0xcea5),
        (TorizoBodyProgram.GoldenCaughtSuperFacingRightLeftLegFwd, 0xceff),
        (TorizoBodyProgram.GoldenSitDownAttackFacingLeft, 0xcf59),
        (TorizoBodyProgram.GoldenSitDownAttackFacingRight, 0xcfc5),
        (TorizoBodyProgram.GoldenReleaseGoldenTorizoEggs0, 0xd031),
        (TorizoBodyProgram.GoldenReleaseGoldenTorizoEggs1, 0xd07b),
        (TorizoBodyProgram.GoldenReleaseGoldenTorizoEggs2, 0xd087),
        (TorizoBodyProgram.GoldenEyeBeamAttack0, 0xd10d),
        (TorizoBodyProgram.GoldenEyeBeamAttack1, 0xd11f),
        (TorizoBodyProgram.GoldenEyeBeamAttack2, 0xd133),
        (TorizoBodyProgram.Stunned0, 0xd193),
        (TorizoBodyProgram.Stunned1, 0xd1a1),
        (TorizoBodyProgram.GoldenDodgeTurningLeft, 0xd1f1),
        (TorizoBodyProgram.GoldenTurningLeft, 0xd203),
        (TorizoBodyProgram.GoldenWalkingLeftRightLegMoving, 0xd20d),
        (TorizoBodyProgram.GoldenWalkingLeftLeftLegMoving, 0xd259),
        (TorizoBodyProgram.GoldenDodgeTurningRight, 0xd2ad),
        (TorizoBodyProgram.GoldenTurningRight, 0xd2bf),
        (TorizoBodyProgram.GoldenWalkingRightLeftLegMoving, 0xd2c9),
        (TorizoBodyProgram.GoldenWalkingRightRightLegMoving, 0xd315),
    ];

    /// <summary>
    /// Each bank-$AA Torizo instruction the body lists use, keyed by its native routine: the cue it must become and
    /// what each operand word carries.
    /// </summary>
    internal static readonly Dictionary<ushort, (NativeCueShape<TorizoCue> Cue, TorizoOperand[] Operands)> BodyInstructions = new()
    {
        [0x806b] = (NativeCueShape<TorizoCue>.Of<TorizoCue.SetPreInstruction>(""), [TorizoOperand.PreInstruction]),
        [0x8074] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.ClearPreInstruction)), []),
        [0xb09c] = (NativeCueShape<TorizoCue>.Of<TorizoCue.SetFunction>(""), [TorizoOperand.Function]),
        [0xb11d] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.BlowUpGut)), []),
        [0xb1be] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.MarkFaceBlownUp)), []),
        [0xb224] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.MakeVisible)), []),
        [0xb22e] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.MakeInvisible)), []),
        [0xb238] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.FadeToBlack)), []),
        [0xb24d] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.FinishDeath)), []),
        [0xb271] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.AdvancePaletteFade)), []),
        [0xb94d] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.FadeToNormalColors)), []),
        [0xb951] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.StartBombTorizoFight)), []),
        [0xc2c8] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.NoOperation)), []),
        [0xc2c9] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.LockAnimation)), []),
        [0xc2d1] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.UnlockAnimation)), []),
        [0xc2d9] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Choice>("", "Choice GoToIfFaceBlownUpElseIfGolden", cue => cue.Kind == TorizoChoiceKind.GoToIfFaceBlownUpElseIfGolden), [TorizoOperand.Target, TorizoOperand.Alternate]),
        [0xc2ed] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch Link", cue => cue.Kind == TorizoBranchKind.Link), [TorizoOperand.Target]),
        [0xc2f7] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.Return)), []),
        [0xc2fd] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.ResumeInterrupted)), []),
        [0xc303] = (NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnLowHealthExplosions", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnLowHealthExplosions), [TorizoOperand.Value]),
        [0xc32f] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnDeathExplosion)), []),
        [0xc34a] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnLandingDust)), []),
        [0xc35b] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnLowHealthDrool)), []),
        [0xc36d] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SetTurningAround)), []),
        [0xc377] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SteppedLeftWithLeftFoot)), []),
        [0xc38a] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SteppedRightWithRightFoot)), []),
        [0xc3a0] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SteppedLeftWithRightFoot)), []),
        [0xc3b6] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SteppedRightWithLeftFoot)), []),
        [0xc3cc] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Posture>("", "Posture StandUp", cue => cue.Kind == TorizoPostureKind.StandUp), [TorizoOperand.Value]),
        [0xc41e] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Posture>("", "Posture SitDown", cue => cue.Kind == TorizoPostureKind.SitDown), [TorizoOperand.Value]),
        [0xc470] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Walk>("", "Walk WalkBombTorizo", cue => cue.Kind == TorizoWalkKind.WalkBombTorizo), [TorizoOperand.Value]),
        [0xc4e5] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Walk>("", "Walk WalkFacelessBombTorizo", cue => cue.Kind == TorizoWalkKind.WalkFacelessBombTorizo), [TorizoOperand.Value]),
        [0xc55a] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfRising", cue => cue.Kind == TorizoBranchKind.GoToIfRising), [TorizoOperand.Target]),
        [0xc567] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallIfSamusCloseBehind", cue => cue.Kind == TorizoBranchKind.CallIfSamusCloseBehind), [TorizoOperand.Target]),
        [0xc58b] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch JumpBackIfSamusCloseInFront", cue => cue.Kind == TorizoBranchKind.JumpBackIfSamusCloseInFront), [TorizoOperand.Target]),
        [0xc5a4] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Choice>("", "Choice CallBombTorizoAttack", cue => cue.Kind == TorizoChoiceKind.CallBombTorizoAttack), [TorizoOperand.Target, TorizoOperand.Alternate]),
        [0xc5cb] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnBombTorizoChozoOrbs)), []),
        [0xc5e3] = (NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnBombTorizoSonicBoom", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom), [TorizoOperand.Value]),
        [0xc5f2] = (NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnGoldenTorizoSonicBoom", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnGoldenTorizoSonicBoom), [TorizoOperand.Value]),
        [0xc601] = (NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnExplosiveSwipe", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnExplosiveSwipe), [TorizoOperand.Value]),
        [0xc610] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.PlayShotSound)), []),
        [0xc618] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.PlayFootstepSound)), []),
        [0xcace] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfAirborne", cue => cue.Kind == TorizoBranchKind.GoToIfAirborne), [TorizoOperand.Target]),
        [0xcade] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.LoadGoldenTorizoPalettes)), []),
        [0xcae2] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.StartGoldenTorizoFight)), []),
        [0xcdd7] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.ClearCaughtSuperMissile)), []),
        [0xd0e9] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnEgg)), []),
        [0xd0f3] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfEyeBeamActive", cue => cue.Kind == TorizoBranchKind.GoToIfEyeBeamActive), [TorizoOperand.Target]),
        [0xd17b] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.DisableEyeBeamExplosions)), []),
        [0xd187] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.EnableEyeBeamExplosions)), []),
        [0xd1e7] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.UnmarkStunned)), []),
        [0xd38f] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.PlayEggReleasedSound)), []),
        [0xd397] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.PlayLaserSound)), []),
        [0xd39f] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.PlaySonicBoomSound)), []),
        [0xd3e0] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnSuperMissile)), []),
        [0xd3ea] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfSamusMorphedBehind", cue => cue.Kind == TorizoBranchKind.GoToIfSamusMorphedBehind), [TorizoOperand.Target]),
        [0xd436] = (NativeCueShape<TorizoCue>.Of<TorizoCue.ParameterizedSpawn>("", "ParameterizedSpawn SpawnEyeBeam", cue => cue.Kind == TorizoParameterizedSpawnKind.SpawnEyeBeam), [TorizoOperand.Value]),
        [0xd445] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallSometimesIfSamusMorphedInFront", cue => cue.Kind == TorizoBranchKind.CallSometimesIfSamusMorphedInFront), [TorizoOperand.Target]),
        [0xd474] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallSometimesIfHealthLow", cue => cue.Kind == TorizoBranchKind.CallSometimesIfHealthLow), [TorizoOperand.Target]),
        [0xd49b] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallIfStunHealthHigh", cue => cue.Kind == TorizoBranchKind.CallIfStunHealthHigh), [TorizoOperand.Target]),
        [0xd4ba] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch JumpForwardIfSamusFar", cue => cue.Kind == TorizoBranchKind.JumpForwardIfSamusFar), [TorizoOperand.Target]),
        [0xd4f3] = (NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Command(TorizoCueKind.SpawnGoldenTorizoChozoOrbs)), []),
        [0xd4fd] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch JumpBackIfSamusCloseInFrontGolden", cue => cue.Kind == TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden), [TorizoOperand.Target]),
        [0xd526] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Choice>("", "Choice CallGoldenTorizoAttack", cue => cue.Kind == TorizoChoiceKind.CallGoldenTorizoAttack), [TorizoOperand.Target, TorizoOperand.Alternate]),
        [0xd54d] = (NativeCueShape<TorizoCue>.Of<TorizoCue.Walk>("", "Walk WalkGoldenTorizo", cue => cue.Kind == TorizoWalkKind.WalkGoldenTorizo), [TorizoOperand.Value]),
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
    internal static readonly (TorizoProjectileProgram Program, ushort Entry)[] ProjectileEntries =
    [
        (TorizoProjectileProgram.ExplosionLowHealthInitial, 0xa3cb),
        (TorizoProjectileProgram.ExplosionLowHealthInitialLoop, 0xa3d5),
        (TorizoProjectileProgram.ExplosionDeathInitial, 0xa3fa),
        (TorizoProjectileProgram.ExplosionDeathInitialLoop, 0xa408),
        (TorizoProjectileProgram.ExplosionDeathSmokeSetup, 0xa431),
        (TorizoProjectileProgram.ExplosionDeathSmokeSetupLoop, 0xa435),
        (TorizoProjectileProgram.DroolFourFrameDelay, 0xa46a),
        (TorizoProjectileProgram.DroolTwoFrameDelay, 0xa46e),
        (TorizoProjectileProgram.DroolNoDelay, 0xa472),
        (TorizoProjectileProgram.DroolFallingLoop, 0xa482),
        (TorizoProjectileProgram.DroolWallImpact, 0xa48a),
        (TorizoProjectileProgram.DroolFloorImpact, 0xa48e),
        (TorizoProjectileProgram.SwipeInitial, 0xa4aa),
        (TorizoProjectileProgram.StatueFragment0, 0xa4c3),
        (TorizoProjectileProgram.StatueFragment1, 0xa4d4),
        (TorizoProjectileProgram.StatueFragment2, 0xa4e5),
        (TorizoProjectileProgram.StatueFragment3, 0xa4f6),
        (TorizoProjectileProgram.StatueFragment4, 0xa507),
        (TorizoProjectileProgram.StatueFragment5, 0xa518),
        (TorizoProjectileProgram.StatueFragment6, 0xa529),
        (TorizoProjectileProgram.StatueFragment7, 0xa53a),
        (TorizoProjectileProgram.StatueFragment8, 0xa54b),
        (TorizoProjectileProgram.StatueFragment9, 0xa55c),
        (TorizoProjectileProgram.StatueFragment10, 0xa56d),
        (TorizoProjectileProgram.StatueFragment11, 0xa57e),
        (TorizoProjectileProgram.StatueFragment12, 0xa58f),
        (TorizoProjectileProgram.StatueFragment13, 0xa5a0),
        (TorizoProjectileProgram.StatueFragment14, 0xa5b1),
        (TorizoProjectileProgram.StatueFragment15, 0xa5c2),
        (TorizoProjectileProgram.ChozoOrbMovingLeft, 0xab15),
        (TorizoProjectileProgram.ChozoOrbMovingRight, 0xab1d),
        (TorizoProjectileProgram.ChozoOrbWallImpact, 0xab25),
        (TorizoProjectileProgram.ChozoOrbFloorImpact, 0xab41),
        (TorizoProjectileProgram.ChozoOrbShot, 0xab68),
        (TorizoProjectileProgram.SonicBoomFiredLeft, 0xadbf),
        (TorizoProjectileProgram.SonicBoomMovingLeft, 0xadca),
        (TorizoProjectileProgram.SonicBoomFiredRight, 0xadd2),
        (TorizoProjectileProgram.SonicBoomMovingRight, 0xaddd),
        (TorizoProjectileProgram.SonicBoomWallImpact, 0xade5),
        (TorizoProjectileProgram.SonicBoomWallImpactLoop, 0xadf1),
        (TorizoProjectileProgram.LandingDustRightFoot, 0xaf9d),
        (TorizoProjectileProgram.LandingDustLeftFoot, 0xafb5),
        (TorizoProjectileProgram.EggBouncingLeft, 0xb104),
        (TorizoProjectileProgram.EggBouncingLeftHatching, 0xb10a),
        (TorizoProjectileProgram.EggBouncingRight, 0xb11c),
        (TorizoProjectileProgram.EggBouncingRightHatching, 0xb122),
        (TorizoProjectileProgram.EggHatch, 0xb134),
        (TorizoProjectileProgram.EggHatchedLeft, 0xb14b),
        (TorizoProjectileProgram.EggHatchedLeftLoop, 0xb152),
        (TorizoProjectileProgram.EggHatchedRight, 0xb166),
        (TorizoProjectileProgram.EggHatchedRightLoop, 0xb16d),
        (TorizoProjectileProgram.EggBreakLeft, 0xb190),
        (TorizoProjectileProgram.EggBreakRight, 0xb1a8),
        (TorizoProjectileProgram.SuperMissileRightInitial, 0xb293),
        (TorizoProjectileProgram.SuperMissileRightLoop, 0xb29d),
        (TorizoProjectileProgram.SuperMissileLeftInitial, 0xb2c1),
        (TorizoProjectileProgram.SuperMissileLeftLoop, 0xb2cb),
        (TorizoProjectileProgram.SuperMissileImpact, 0xb2ef),
        (TorizoProjectileProgram.EyeBeamWallImpact, 0xb3cd),
        (TorizoProjectileProgram.EyeBeamFloorImpact, 0xb3e5),
        (TorizoProjectileProgram.EyeBeamFloorImpactLoop, 0xb3e7),
        (TorizoProjectileProgram.EyeBeamNormal, 0xb410),
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
