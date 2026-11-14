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

    /// <summary>
    /// The entry of every native body list in bank $AA, in bank order: the lists the body animations play runs of, each
    /// ending where the next begins.
    /// </summary>
    internal static readonly ushort[] BodyLists =
    [
        0xb0e5, 0xb155, 0xb1c8, 0xb1d2, 0xb1e2, 0xb879, 0xb887, 0xb8c7,
        0xb935, 0xb962, 0xb96c, 0xb9b6, 0xba04, 0xba26, 0xba46, 0xba68,
        0xba88, 0xba90, 0xbaf2, 0xbafa, 0xbb5c, 0xbbde, 0xbc60, 0xbc70,
        0xbc78, 0xbc80, 0xbc88, 0xbc96, 0xbca6, 0xbcae, 0xbcb6, 0xbcbe,
        0xbcd2, 0xbce2, 0xbcea, 0xbcf2, 0xbcfa, 0xbd0e, 0xbd18, 0xbd52,
        0xbd90, 0xbd9c, 0xbdd8, 0xbde2, 0xbe30, 0xbe7e, 0xbea0, 0xbec0,
        0xbee2, 0xbf02, 0xbf0a, 0xbf6c, 0xbf74, 0xbfd6, 0xc058, 0xc0da,
        0xc0ea, 0xc0f2, 0xc0fa, 0xc102, 0xc110, 0xc120, 0xc128, 0xc130,
        0xc138, 0xc14c, 0xc15c, 0xc164, 0xc16c, 0xc174, 0xc188, 0xc192,
        0xc1cc, 0xc9cb, 0xc9e2, 0xc9e6, 0xca48, 0xcab6, 0xcaff, 0xcb1f,
        0xcb41, 0xcb61, 0xcb83, 0xcb8b, 0xcbed, 0xcbf5, 0xcc57, 0xcc77,
        0xcc99, 0xccb9, 0xccdb, 0xcce3, 0xcd45, 0xcd4d, 0xcdaf, 0xcdb9,
        0xcdc3, 0xcdcd, 0xcde1, 0xce43, 0xcea5, 0xceff, 0xcf59, 0xcfc5,
        0xd031, 0xd07b, 0xd087, 0xd10d, 0xd11f, 0xd133, 0xd193, 0xd1a1,
        0xd1f1, 0xd203, 0xd20d, 0xd259, 0xd2ad, 0xd2bf, 0xd2c9, 0xd315,
    ];

    /// <summary>The native list each body animation starts at (#1788).</summary>
    internal static readonly IReadOnlyDictionary<TorizoAnimation, ushort> AnimationEntries = new Dictionary<TorizoAnimation, ushort>
    {
        [TorizoAnimation.BombTorizoGutBlowingUp] = 0xb0e5,
        [TorizoAnimation.BombTorizoFaceBlowingUp] = 0xb155,
        [TorizoAnimation.Dying] = 0xb1c8,
        [TorizoAnimation.BombTorizoDormant] = 0xb879,
        [TorizoAnimation.BombTorizoAwakening] = 0xb887,
        [TorizoAnimation.Stunned] = 0xd193,
        [TorizoAnimation.TurningLeft] = 0xb962,
        [TorizoAnimation.FacingLeftSpewingChozoOrbsRightFootForward] = 0xba04,
        [TorizoAnimation.FacingLeftSpewingChozoOrbsLeftFootForward] = 0xba46,
        [TorizoAnimation.FacingLeftSonicBoomsRightFootForward] = 0xba88,
        [TorizoAnimation.FacingLeftSonicBoomsLeftFootForward] = 0xbaf2,
        [TorizoAnimation.FacingLeftExplosiveSwipeRightFootForward] = 0xbb5c,
        [TorizoAnimation.FacingLeftExplosiveSwipeLeftFootForward] = 0xbbde,
        [TorizoAnimation.FacingLeftJumpingForward] = 0xbc60,
        [TorizoAnimation.FacingLeftFalling] = 0xbc78,
        [TorizoAnimation.FacingLeftLanding] = 0xbc88,
        [TorizoAnimation.FacingLeftJumpingBackwardToLeftFoot] = 0xbc96,
        [TorizoAnimation.FacingLeftFallingBackwardToLeftFoot] = 0xbcae,
        [TorizoAnimation.FacingLeftLandingBackwardOnLeftFoot] = 0xbcbe,
        [TorizoAnimation.FacingLeftJumpingBackwardToRightFoot] = 0xbcd2,
        [TorizoAnimation.FacingLeftFallingBackwardToRightFoot] = 0xbcea,
        [TorizoAnimation.FacingLeftLandingBackwardOnRightFoot] = 0xbcfa,
        [TorizoAnimation.FacelessTurningLeft] = 0xbd0e,
        [TorizoAnimation.FacingLeftFacelessWalkingRightLegMoving] = 0xbd18,
        [TorizoAnimation.FacingLeftFacelessWalkingLeftLegMoving] = 0xbd52,
        [TorizoAnimation.BombTorizoDormantFacingRight] = 0xbd90,
        [TorizoAnimation.BombTorizoAwakeningFacingRight] = 0xbd9c,
        [TorizoAnimation.TurningRight] = 0xbdd8,
        [TorizoAnimation.FacingRightSpewingChozoOrbsLeftFootForward] = 0xbe7e,
        [TorizoAnimation.FacingRightSpewingChozoOrbsRightFootForward] = 0xbec0,
        [TorizoAnimation.FacingRightSonicBoomsLeftFootForward] = 0xbf02,
        [TorizoAnimation.FacingRightSonicBoomsRightFootForward] = 0xbf6c,
        [TorizoAnimation.FacingRightExplosiveSwipeLeftFootForward] = 0xbfd6,
        [TorizoAnimation.FacingRightExplosiveSwipeRightFootForward] = 0xc058,
        [TorizoAnimation.FacingRightJumpingForward] = 0xc0da,
        [TorizoAnimation.FacingRightFalling] = 0xc0f2,
        [TorizoAnimation.FacingRightLanding] = 0xc102,
        [TorizoAnimation.FacingRightJumpingBackwardToRightFoot] = 0xc110,
        [TorizoAnimation.FacingRightFallingBackwardToRightFoot] = 0xc128,
        [TorizoAnimation.FacingRightLandingBackwardOnRightFoot] = 0xc138,
        [TorizoAnimation.FacingRightJumpingBackwardToLeftFoot] = 0xc14c,
        [TorizoAnimation.FacingRightFallingBackwardToLeftFoot] = 0xc164,
        [TorizoAnimation.FacingRightLandingBackwardOnLeftFoot] = 0xc174,
        [TorizoAnimation.FacelessTurningRight] = 0xc188,
        [TorizoAnimation.FacingRightFacelessWalkingLeftLegMoving] = 0xc192,
        [TorizoAnimation.FacingRightFacelessWalkingRightLegMoving] = 0xc1cc,
        [TorizoAnimation.GoldenTorizoDormant] = 0xc9cb,
        [TorizoAnimation.GoldenTorizoAwakening] = 0xc9e2,
        [TorizoAnimation.GoldenFacingLeftSpewingChozoOrbsRightFootForward] = 0xcaff,
        [TorizoAnimation.GoldenFacingLeftSpewingChozoOrbsLeftFootForward] = 0xcb41,
        [TorizoAnimation.GoldenFacingLeftSonicBoomsRightFootForward] = 0xcb83,
        [TorizoAnimation.GoldenFacingLeftSonicBoomsLeftFootForward] = 0xcbed,
        [TorizoAnimation.GoldenFacingRightSpewingChozoOrbsLeftFootForward] = 0xcc57,
        [TorizoAnimation.GoldenFacingRightSpewingChozoOrbsRightFootForward] = 0xcc99,
        [TorizoAnimation.GoldenFacingRightSonicBoomsLeftFootForward] = 0xccdb,
        [TorizoAnimation.GoldenFacingRightSonicBoomsRightFootForward] = 0xcd45,
        [TorizoAnimation.GoldenFacingLeftLandedBackwardOnLeftFoot] = 0xcdaf,
        [TorizoAnimation.GoldenFacingLeftLandedBackwardOnRightFoot] = 0xcdb9,
        [TorizoAnimation.GoldenFacingRightLandedBackwardOnRightFoot] = 0xcdc3,
        [TorizoAnimation.GoldenFacingRightLandedBackwardOnLeftFoot] = 0xcdcd,
        [TorizoAnimation.GoldenFacingLeftCatchingSuperMissileLeftLegForward] = 0xcde1,
        [TorizoAnimation.GoldenFacingLeftCatchingSuperMissileRightLegForward] = 0xce43,
        [TorizoAnimation.GoldenFacingRightCatchingSuperMissileRightLegForward] = 0xcea5,
        [TorizoAnimation.GoldenFacingRightCatchingSuperMissileLeftLegForward] = 0xceff,
        [TorizoAnimation.GoldenFacingLeftSittingDownAttack] = 0xcf59,
        [TorizoAnimation.GoldenFacingRightSittingDownAttack] = 0xcfc5,
        [TorizoAnimation.GoldenReleasingEggs] = 0xd031,
        [TorizoAnimation.GoldenEyeBeamAttack] = 0xd10d,
        [TorizoAnimation.GoldenDodgeTurningLeft] = 0xd1f1,
        [TorizoAnimation.GoldenTurningLeft] = 0xd203,
        [TorizoAnimation.GoldenWalkingLeftLeftLegMoving] = 0xd259,
        [TorizoAnimation.GoldenDodgeTurningRight] = 0xd2ad,
        [TorizoAnimation.GoldenTurningRight] = 0xd2bf,
        [TorizoAnimation.GoldenWalkingRightRightLegMoving] = 0xd315,
    };

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
        [0xc55a] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Replay>("", "Replay WhileRising", cue => cue.Kind == TorizoReplayKind.WhileRising), [TorizoOperand.ReplayedRun]),
        [0xc567] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallIfSamusCloseBehind", cue => cue.Kind == TorizoBranchKind.CallIfSamusCloseBehind), [TorizoOperand.Target]),
        [0xc58b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch JumpBackIfSamusCloseInFront", cue => cue.Kind == TorizoBranchKind.JumpBackIfSamusCloseInFront), [TorizoOperand.Target]),
        [0xc5a4] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Choice>("", "Choice CallBombTorizoAttack", cue => cue.Kind == TorizoChoiceKind.CallBombTorizoAttack), [TorizoOperand.Target, TorizoOperand.Alternate]),
        [0xc5cb] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnBombTorizoChozoOrbs)), []),
        [0xc5e3] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnBombTorizoSonicBoom)), [TorizoOperand.Unread]),
        [0xc5f2] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnGoldenTorizoSonicBoom)), [TorizoOperand.Unread]),
        [0xc601] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Swipe>(""), [TorizoOperand.SwipePlacement]),
        [0xc610] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.PlayShotSound)), []),
        [0xc618] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.PlayFootstepSound)), []),
        [0xcace] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Replay>("", "Replay WhileAirborne", cue => cue.Kind == TorizoReplayKind.WhileAirborne), [TorizoOperand.ReplayedRun]),
        [0xcade] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.FadeToGoldenColors)), []),
        [0xcae2] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.StartGoldenTorizoFight)), []),
        [0xcdd7] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.ClearCaughtSuperMissile)), []),
        [0xd0e9] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnEgg)), []),
        [0xd0f3] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Replay>("", "Replay WhileEggsRemain", cue => cue.Kind == TorizoReplayKind.WhileEggsRemain), [TorizoOperand.ReplayedRun]),
        [0xd17b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.DisableEyeBeamExplosions)), []),
        [0xd187] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.EnableEyeBeamExplosions)), []),
        [0xd1e7] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.UnmarkStunned)), []),
        [0xd38f] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.PlayEggReleasedSound)), []),
        [0xd397] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.PlayLaserSound)), []),
        [0xd39f] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.PlaySonicBoomSound)), []),
        [0xd3e0] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnSuperMissile)), []),
        [0xd3ea] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch GoToIfSamusMorphedBehind", cue => cue.Kind == TorizoBranchKind.GoToIfSamusMorphedBehind), [TorizoOperand.Target]),
        [0xd436] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnEyeBeam)), [TorizoOperand.Unread]),
        [0xd445] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallSometimesIfSamusMorphedInFront", cue => cue.Kind == TorizoBranchKind.CallSometimesIfSamusMorphedInFront), [TorizoOperand.Target]),
        [0xd474] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallSometimesIfHealthLow", cue => cue.Kind == TorizoBranchKind.CallSometimesIfHealthLow), [TorizoOperand.Target]),
        [0xd49b] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch CallIfStunHealthHigh", cue => cue.Kind == TorizoBranchKind.CallIfStunHealthHigh), [TorizoOperand.Target]),
        [0xd4ba] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch JumpForwardIfSamusFar", cue => cue.Kind == TorizoBranchKind.JumpForwardIfSamusFar), [TorizoOperand.Target]),
        [0xd4f3] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Exactly(new TorizoCue.Signal(TorizoCueKind.SpawnGoldenTorizoChozoOrbs)), []),
        [0xd4fd] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Branch>("", "Branch JumpBackIfSamusCloseInFrontGolden", cue => cue.Kind == TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden), [TorizoOperand.Target]),
        [0xd526] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Choice>("", "Choice CallGoldenTorizoAttack", cue => cue.Kind == TorizoChoiceKind.CallGoldenTorizoAttack), [TorizoOperand.Target, TorizoOperand.Alternate]),
        [0xd54d] = new TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue>.Of<TorizoCue.Walk>("", "Walk WalkGoldenTorizo", cue => cue.Kind == TorizoWalkKind.WalkGoldenTorizo), [TorizoOperand.Value]),
    };

    /// <summary>
    /// The bank-$AA routine both Torizos' stand-up lists call between the colour fade and the unlock (#1870): it returned at
    /// once, so the animations have no event for it and the oracle reads past it without a beat.
    /// </summary>
    internal static readonly HashSet<ushort> OmittedBodyInstructions = [0xc2c8];

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

    /// <summary>
    /// The entry of every native Torizo projectile list in bank $86, in bank order: the lists the projectile animations play
    /// runs of, each ending where the next begins.
    /// </summary>
    internal static readonly ushort[] ProjectileLists =
    [
        0xa3cb, 0xa3d5, 0xa3fa, 0xa408, 0xa431, 0xa435, 0xa46a, 0xa46e,
        0xa472, 0xa482, 0xa48a, 0xa48e, 0xa4aa, 0xa4c3, 0xa4d4, 0xa4e5,
        0xa4f6, 0xa507, 0xa518, 0xa529, 0xa53a, 0xa54b, 0xa55c, 0xa56d,
        0xa57e, 0xa58f, 0xa5a0, 0xa5b1, 0xa5c2, 0xab15, 0xab1d, 0xab25,
        0xab41, 0xab68, 0xadbf, 0xadca, 0xadd2, 0xaddd, 0xade5, 0xadf1,
        0xaf9d, 0xafb5, 0xb104, 0xb10a, 0xb11c, 0xb122, 0xb134, 0xb14b,
        0xb152, 0xb166, 0xb16d, 0xb190, 0xb1a8, 0xb293, 0xb29d, 0xb2c1,
        0xb2cb, 0xb2ef, 0xb3cd, 0xb3e5, 0xb3e7, 0xb410,
    ];

    /// <summary>The native list each projectile animation starts at (#1788).</summary>
    internal static readonly IReadOnlyDictionary<TorizoProjectileSegment, ushort> ProjectileAnimationEntries = new Dictionary<TorizoProjectileSegment, ushort>
    {
        [TorizoProjectileSegment.LowHealthExplosion] = 0xa3cb,
        [TorizoProjectileSegment.DeathExplosion] = 0xa3fa,
        [TorizoProjectileSegment.DeathSmoke] = 0xa431,
        [TorizoProjectileSegment.DroolFourFrameDelay] = 0xa46a,
        [TorizoProjectileSegment.DroolTwoFrameDelay] = 0xa46e,
        [TorizoProjectileSegment.DroolNoDelay] = 0xa472,
        [TorizoProjectileSegment.DroolWallImpact] = 0xa48a,
        [TorizoProjectileSegment.DroolFloorImpact] = 0xa48e,
        [TorizoProjectileSegment.ExplosiveSwipe] = 0xa4aa,
        [TorizoProjectileSegment.FirstStatueFragment] = 0xa4c3,
        [TorizoProjectileSegment.SecondStatueFragment] = 0xa4d4,
        [TorizoProjectileSegment.ThirdStatueFragment] = 0xa4e5,
        [TorizoProjectileSegment.FourthStatueFragment] = 0xa4f6,
        [TorizoProjectileSegment.FifthStatueFragment] = 0xa507,
        [TorizoProjectileSegment.SixthStatueFragment] = 0xa518,
        [TorizoProjectileSegment.SeventhStatueFragment] = 0xa529,
        [TorizoProjectileSegment.EighthStatueFragment] = 0xa53a,
        [TorizoProjectileSegment.NinthStatueFragment] = 0xa54b,
        [TorizoProjectileSegment.TenthStatueFragment] = 0xa55c,
        [TorizoProjectileSegment.EleventhStatueFragment] = 0xa56d,
        [TorizoProjectileSegment.TwelfthStatueFragment] = 0xa57e,
        [TorizoProjectileSegment.ThirteenthStatueFragment] = 0xa58f,
        [TorizoProjectileSegment.FourteenthStatueFragment] = 0xa5a0,
        [TorizoProjectileSegment.FifteenthStatueFragment] = 0xa5b1,
        [TorizoProjectileSegment.SixteenthStatueFragment] = 0xa5c2,
        [TorizoProjectileSegment.ChozoOrbMovingLeft] = 0xab15,
        [TorizoProjectileSegment.ChozoOrbMovingRight] = 0xab1d,
        [TorizoProjectileSegment.ChozoOrbWallImpact] = 0xab25,
        [TorizoProjectileSegment.ChozoOrbFloorImpact] = 0xab41,
        [TorizoProjectileSegment.ChozoOrbShot] = 0xab68,
        [TorizoProjectileSegment.SonicBoomFiredLeft] = 0xadbf,
        [TorizoProjectileSegment.SonicBoomFiredRight] = 0xadd2,
        [TorizoProjectileSegment.SonicBoomWallImpact] = 0xade5,
        [TorizoProjectileSegment.LandingDustRightFoot] = 0xaf9d,
        [TorizoProjectileSegment.LandingDustLeftFoot] = 0xafb5,
        [TorizoProjectileSegment.EggBouncingLeft] = 0xb104,
        [TorizoProjectileSegment.EggHatchingLeft] = 0xb10a,
        [TorizoProjectileSegment.EggBouncingRight] = 0xb11c,
        [TorizoProjectileSegment.EggHatchingRight] = 0xb122,
        [TorizoProjectileSegment.EggHatchedLeft] = 0xb14b,
        [TorizoProjectileSegment.EggHatchedRight] = 0xb166,
        [TorizoProjectileSegment.EggBreakLeft] = 0xb190,
        [TorizoProjectileSegment.EggBreakRight] = 0xb1a8,
        [TorizoProjectileSegment.SuperMissileRight] = 0xb293,
        [TorizoProjectileSegment.SuperMissileLeft] = 0xb2c1,
        [TorizoProjectileSegment.SuperMissileImpact] = 0xb2ef,
        [TorizoProjectileSegment.EyeBeamWallImpact] = 0xb3cd,
        [TorizoProjectileSegment.EyeBeamFloorImpact] = 0xb3e5,
        [TorizoProjectileSegment.EyeBeam] = 0xb410,
    };

    /// <summary>The native list <paramref name="animation"/> starts at.</summary>
    internal static ushort Entry(TorizoAnimation animation) =>
        AnimationEntries.TryGetValue(animation, out ushort entry)
            ? entry
            : throw new ArgumentOutOfRangeException(nameof(animation), animation, "No native Torizo list starts this animation.");

    /// <summary>The native list projectile animation <paramref name="animation"/> starts at.</summary>
    internal static ushort ProjectileEntry(TorizoProjectileSegment animation) =>
        ProjectileAnimationEntries.TryGetValue(animation, out ushort entry)
            ? entry
            : throw new ArgumentOutOfRangeException(nameof(animation), animation, "No native Torizo list starts this projectile animation.");

    /// <summary>The projectile animation that starts at the native list at <paramref name="address"/>, if one does.</summary>
    internal static TorizoProjectileSegment? ProjectileAnimationAt(ushort address) =>
        ProjectileAnimationEntries.Where(pair => pair.Value == address).Select(pair => (TorizoProjectileSegment?)pair.Key).SingleOrDefault();

    /// <summary>The body animation that starts at the native list at <paramref name="address"/>, if one does.</summary>
    internal static TorizoAnimation? AnimationAt(ushort address) =>
        AnimationEntries.Where(pair => pair.Value == address).Select(pair => (TorizoAnimation?)pair.Key).SingleOrDefault();

    /// <summary>Whether a native body list starts at <paramref name="address"/>.</summary>
    internal static bool IsBodyList(ushort address) => Array.IndexOf(BodyLists, address) >= 0;
}
