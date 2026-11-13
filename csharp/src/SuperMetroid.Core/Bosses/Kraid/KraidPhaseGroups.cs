using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The phases and part animations Kraid's encounter tests as groups (#1452). The game tests a phase or
/// part animation by its place in the animation's layout, so each test admits every member laid out on one side of one
/// behaviour or animation; each group names those members instead of ordering the identities. The rise phases are
/// laid out after the death phases, so a test for "the death sequence has begun" also admits them.
/// </summary>
internal static class KraidPhaseGroups
{
    /// <summary>The phases laid out before the death sequence's first step: combat, growth and the parts' AI.</summary>
    internal static readonly FrozenSet<KraidPhase> BeforeDeath = new[]
    {
        KraidPhase.None, KraidPhase.NoOperation, KraidPhase.GrowBreakCeilingPlatforms,
        KraidPhase.GrowBringBodyInFront, KraidPhase.GrowShowLowerBodyInFront, KraidPhase.GrowDrawRoomBackground,
        KraidPhase.GrowFadeInRoomBackground, KraidPhase.MainloopThinking, KraidPhase.SecondPhaseThinking,
        KraidPhase.MouthOpenReaction, KraidPhase.InitializeEyeGlow, KraidPhase.GlowEye, KraidPhase.UnglowEye,
        KraidPhase.LintInactive, KraidPhase.LintProduce, KraidPhase.LintCharge, KraidPhase.LintFire,
        KraidPhase.FingernailWaitForLint, KraidPhase.AlignPartToKraid, KraidPhase.HandleFunctionTimer,
        KraidPhase.DecrementFunctionTimerAndStartWalk, KraidPhase.FootFirstPhaseThinking,
        KraidPhase.AdvanceHeadAnimationAndTimer, KraidPhase.FootSecondPhaseThinking,
        KraidPhase.FootSecondPhaseWalkingRight, KraidPhase.FootSecondPhaseWalkToStart,
        KraidPhase.FootSecondPhaseInitialize, KraidPhase.FootSecondPhaseWalkingLeft,
        KraidPhase.MainAttackWithMouthOpen, KraidPhase.FingernailInitialize, KraidPhase.FingernailFire,
        KraidPhase.FootPrepareFirstPhaseLunge, KraidPhase.FootFirstPhaseLunge, KraidPhase.FootFirstPhaseRetreat,
        KraidPhase.GrowReleaseCamera,
    }.ToFrozenSet();

    /// <summary>The phases laid out before Kraid starts sinking: everything before death and the death sequence's fade and body-layer updates.</summary>
    internal static readonly FrozenSet<KraidPhase> BeforeSinking = BeforeDeath.Concat(new[]
    {
        KraidPhase.DeathInitialize, KraidPhase.DeathFadeOut, KraidPhase.DeathShowUpperBody,
        KraidPhase.DeathShowLowerBody,
    }).ToFrozenSet();

    /// <summary>Kraid rising through the floor at the start of the encounter.</summary>
    internal static readonly FrozenSet<KraidPhase> Rise = new[]
    {
        KraidPhase.RestrictSamusToFirstScreen, KraidPhase.RaiseKraidThroughFloor, KraidPhase.RaiseShowLowerBody,
        KraidPhase.RaiseRocksEvery16Frames, KraidPhase.RaiseRocksEvery8Frames, KraidPhase.RaiseBody,
    }.ToFrozenSet();

    /// <summary>The part animations laid out before the slow arm swing: the feet's and the normal arm's.</summary>
    internal static readonly FrozenSet<KraidPartSegment> BeforeArmSlow = new[]
    {
        KraidPartSegment.FootInitial, KraidPartSegment.FootNeutral, KraidPartSegment.FootWalkingForward,
        KraidPartSegment.FootWalkingForwardFinished, KraidPartSegment.FootLungeForward, KraidPartSegment.FootLungeForwardFinished,
        KraidPartSegment.FootWalkingBackward, KraidPartSegment.FootWalkingBackwardLoop, KraidPartSegment.ArmNormal,
        KraidPartSegment.ArmNormalPause,
    }.ToFrozenSet();

    /// <summary>The part animations laid out before the normal arm's pause: the feet's and the normal arm swing.</summary>
    internal static readonly FrozenSet<KraidPartSegment> BeforeArmNormalPause = new[]
    {
        KraidPartSegment.FootInitial, KraidPartSegment.FootNeutral, KraidPartSegment.FootWalkingForward,
        KraidPartSegment.FootWalkingForwardFinished, KraidPartSegment.FootLungeForward, KraidPartSegment.FootLungeForwardFinished,
        KraidPartSegment.FootWalkingBackward, KraidPartSegment.FootWalkingBackwardLoop, KraidPartSegment.ArmNormal,
    }.ToFrozenSet();

    /// <summary>The foot animations laid out before the backward walk's loop: the foot has not yet settled into walking back.</summary>
    internal static readonly FrozenSet<KraidPartSegment> BeforeFootWalkingBackwardLoop = new[]
    {
        KraidPartSegment.FootInitial, KraidPartSegment.FootNeutral, KraidPartSegment.FootWalkingForward,
        KraidPartSegment.FootWalkingForwardFinished, KraidPartSegment.FootLungeForward, KraidPartSegment.FootLungeForwardFinished,
        KraidPartSegment.FootWalkingBackward,
    }.ToFrozenSet();
}
