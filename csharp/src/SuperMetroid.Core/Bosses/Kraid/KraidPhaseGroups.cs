using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The phases and part animations Kraid's encounter tests as groups (#1452, #1737), each named for what its test decides
/// in the fight: whether he is fighting, whether shots collide with him, whether he is rising through the floor, and
/// how far his arm's swing and his foot's step have gone. Kraid's rise through the floor is not part of the fight: a
/// killing hit during it does not start his death and his body does not hurt Samus.
/// </summary>
internal static class KraidPhaseGroups
{
    /// <summary>
    /// The phases in which Kraid fights: combat, growth and his parts' own behaviour. Only in these does his body's touch
    /// hurt Samus and a mouth hit that leaves him without health start his death; his death sequence and his rise through
    /// the floor lie outside it.
    /// </summary>
    internal static readonly FrozenSet<KraidPhase> Fighting = new[]
    {
        KraidPhase.GrowBreakCeilingPlatforms,
        KraidPhase.GrowBringBodyInFront, KraidPhase.GrowShowLowerBodyInFront, KraidPhase.GrowDrawRoomBackground,
        KraidPhase.GrowFadeInRoomBackground, KraidPhase.MainloopThinking, KraidPhase.SecondPhaseThinking,
        KraidPhase.MouthOpenReaction, KraidPhase.InitializeEyeGlow, KraidPhase.GlowEye, KraidPhase.UnglowEye,
        KraidPhase.LintInactive, KraidPhase.LintProduce, KraidPhase.LintCharge, KraidPhase.LintFire,
        KraidPhase.FingernailWaitForLint, KraidPhase.AlignPartToKraid, KraidPhase.WaitToResumePhase,
        KraidPhase.FootWaitBeforeWalk, KraidPhase.FootFirstPhaseThinking,
        KraidPhase.AdvanceHeadAnimationAndTimer, KraidPhase.FootSecondPhaseThinking,
        KraidPhase.FootSecondPhaseWalkingRight, KraidPhase.FootSecondPhaseWalkToStart,
        KraidPhase.FootSecondPhaseInitialize, KraidPhase.FootSecondPhaseWalkingLeft,
        KraidPhase.MainAttackWithMouthOpen, KraidPhase.FingernailInitialize, KraidPhase.FingernailFire,
        KraidPhase.FootPrepareFirstPhaseLunge, KraidPhase.FootFirstPhaseLunge, KraidPhase.FootFirstPhaseRetreat,
        KraidPhase.GrowReleaseCamera,
    }.ToFrozenSet();

    /// <summary>
    /// The phases in which Samus's shots collide with Kraid: while he fights, and through his death's fade and the
    /// updates that show his upper and lower body, until he starts sinking. While he rises through the floor and once he
    /// sinks, his mouth and body passes let every shot through.
    /// </summary>
    internal static readonly FrozenSet<KraidPhase> ShotsCollide = Fighting.Concat(new[]
    {
        KraidPhase.DeathInitialize, KraidPhase.DeathFadeOut, KraidPhase.DeathShowUpperBody,
        KraidPhase.DeathShowLowerBody,
    }).ToFrozenSet();

    /// <summary>The phases in which Kraid rises through the floor at the start of the encounter, which run his rise instead of the fight.</summary>
    internal static readonly FrozenSet<KraidPhase> Rising = new[]
    {
        KraidPhase.RestrictSamusToFirstScreen, KraidPhase.RaiseKraidThroughFloor, KraidPhase.RaiseShowLowerBody,
        KraidPhase.RaiseRocksEvery16Frames, KraidPhase.RaiseRocksEvery8Frames, KraidPhase.RaiseBody,
    }.ToFrozenSet();

    /// <summary>
    /// The arm animations from which Kraid's arm switches to its slow swing once he is below half health: its normal swing
    /// and that swing's pause. An arm already in its slow swing, or dying or preparing a lunge, keeps its animation. The
    /// foot animations belong to the group too, though the arm never shows them.
    /// </summary>
    internal static readonly FrozenSet<KraidPartSegment> ArmSwingingNormally = new[]
    {
        KraidPartSegment.FootInitial, KraidPartSegment.FootNeutral, KraidPartSegment.FootWalkingForward,
        KraidPartSegment.FootWalkingForwardFinished, KraidPartSegment.FootLungeForward, KraidPartSegment.FootLungeForwardFinished,
        KraidPartSegment.FootWalkingBackward, KraidPartSegment.FootWalkingBackwardLoop, KraidPartSegment.ArmNormal,
        KraidPartSegment.ArmNormalPause,
    }.ToFrozenSet();

    /// <summary>
    /// The arm animations the small Kraid's foot waits out before it lunges: the arm's normal swing until it reaches the
    /// swing's pause. Once the arm has paused, or shows any later animation, the lunge begins. The foot animations belong
    /// to the group too, though the arm never shows them.
    /// </summary>
    internal static readonly FrozenSet<KraidPartSegment> ArmYetToPause = new[]
    {
        KraidPartSegment.FootInitial, KraidPartSegment.FootNeutral, KraidPartSegment.FootWalkingForward,
        KraidPartSegment.FootWalkingForwardFinished, KraidPartSegment.FootLungeForward, KraidPartSegment.FootLungeForwardFinished,
        KraidPartSegment.FootWalkingBackward, KraidPartSegment.FootWalkingBackwardLoop, KraidPartSegment.ArmNormal,
    }.ToFrozenSet();

    /// <summary>
    /// The foot animations Kraid's retreat after a lunge and his walks wait out before they end: his stances, the forward
    /// walk and lunge, and the backward walk until it first reaches its loop. Once his foot is looping the backward walk
    /// (or shows an arm animation, which a foot never does), the walk can end.
    /// </summary>
    internal static readonly FrozenSet<KraidPartSegment> FootYetToLoopBackwardWalk = new[]
    {
        KraidPartSegment.FootInitial, KraidPartSegment.FootNeutral, KraidPartSegment.FootWalkingForward,
        KraidPartSegment.FootWalkingForwardFinished, KraidPartSegment.FootLungeForward, KraidPartSegment.FootLungeForwardFinished,
        KraidPartSegment.FootWalkingBackward,
    }.ToFrozenSet();
}
