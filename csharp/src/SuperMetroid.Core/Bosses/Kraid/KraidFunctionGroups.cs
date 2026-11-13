using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The AI functions and part programs Kraid's encounter tests as groups (#1452). The original game tests a function or
/// part program by its place in the program's layout, so each test admits every member laid out on one side of one
/// routine or animation; each group names those members instead of ordering the identities. The rise functions are
/// laid out after the death functions, so a test for "the death sequence has begun" also admits them.
/// </summary>
internal static class KraidFunctionGroups
{
    /// <summary>The functions laid out before the death sequence's first routine: combat, growth and the parts' AI.</summary>
    internal static readonly FrozenSet<KraidAiFunction> BeforeDeath = new[]
    {
        KraidAiFunction.None, KraidAiFunction.NoOperation, KraidAiFunction.GrowBreakCeilingPlatforms,
        KraidAiFunction.GrowBringBodyInFront, KraidAiFunction.GrowShowLowerBodyInFront, KraidAiFunction.GrowDrawRoomBackground,
        KraidAiFunction.GrowFadeInRoomBackground, KraidAiFunction.MainloopThinking, KraidAiFunction.SecondPhaseThinking,
        KraidAiFunction.MouthOpenReaction, KraidAiFunction.InitializeEyeGlow, KraidAiFunction.GlowEye, KraidAiFunction.UnglowEye,
        KraidAiFunction.LintInactive, KraidAiFunction.LintProduce, KraidAiFunction.LintCharge, KraidAiFunction.LintFire,
        KraidAiFunction.FingernailWaitForLint, KraidAiFunction.AlignPartToKraid, KraidAiFunction.HandleFunctionTimer,
        KraidAiFunction.DecrementFunctionTimerAndStartWalk, KraidAiFunction.FootFirstPhaseThinking,
        KraidAiFunction.AdvanceHeadAnimationAndTimer, KraidAiFunction.FootSecondPhaseThinking,
        KraidAiFunction.FootSecondPhaseWalkingRight, KraidAiFunction.FootSecondPhaseWalkToStart,
        KraidAiFunction.FootSecondPhaseInitialize, KraidAiFunction.FootSecondPhaseWalkingLeft,
        KraidAiFunction.MainAttackWithMouthOpen, KraidAiFunction.FingernailInitialize, KraidAiFunction.FingernailFire,
        KraidAiFunction.FootPrepareFirstPhaseLunge, KraidAiFunction.FootFirstPhaseLunge, KraidAiFunction.FootFirstPhaseRetreat,
        KraidAiFunction.GrowReleaseCamera,
    }.ToFrozenSet();

    /// <summary>The functions laid out before Kraid starts sinking: everything before death and the death sequence's fade and tilemap updates.</summary>
    internal static readonly FrozenSet<KraidAiFunction> BeforeSinking = BeforeDeath.Concat(new[]
    {
        KraidAiFunction.DeathInitialize, KraidAiFunction.DeathFadeOut, KraidAiFunction.DeathShowUpperBody,
        KraidAiFunction.DeathShowLowerBody,
    }).ToFrozenSet();

    /// <summary>Kraid rising through the floor at the start of the encounter.</summary>
    internal static readonly FrozenSet<KraidAiFunction> Rise = new[]
    {
        KraidAiFunction.RestrictSamusToFirstScreen, KraidAiFunction.RaiseKraidThroughFloor, KraidAiFunction.RaiseShowLowerBody,
        KraidAiFunction.RaiseRocksEvery16Frames, KraidAiFunction.RaiseRocksEvery8Frames, KraidAiFunction.RaiseBody,
    }.ToFrozenSet();

    /// <summary>The part programs laid out before the slow arm swing: the feet's and the normal arm's.</summary>
    internal static readonly FrozenSet<KraidPartSegment> BeforeArmSlow = new[]
    {
        KraidPartSegment.FootInitial, KraidPartSegment.FootNeutral, KraidPartSegment.FootWalkingForward,
        KraidPartSegment.FootWalkingForwardFinished, KraidPartSegment.FootLungeForward, KraidPartSegment.FootLungeForwardFinished,
        KraidPartSegment.FootWalkingBackward, KraidPartSegment.FootWalkingBackwardLoop, KraidPartSegment.ArmNormal,
        KraidPartSegment.ArmNormalPause,
    }.ToFrozenSet();

    /// <summary>The part programs laid out before the normal arm's pause: the feet's and the normal arm swing.</summary>
    internal static readonly FrozenSet<KraidPartSegment> BeforeArmNormalPause = new[]
    {
        KraidPartSegment.FootInitial, KraidPartSegment.FootNeutral, KraidPartSegment.FootWalkingForward,
        KraidPartSegment.FootWalkingForwardFinished, KraidPartSegment.FootLungeForward, KraidPartSegment.FootLungeForwardFinished,
        KraidPartSegment.FootWalkingBackward, KraidPartSegment.FootWalkingBackwardLoop, KraidPartSegment.ArmNormal,
    }.ToFrozenSet();

    /// <summary>The foot programs laid out before the backward walk's loop: the foot has not yet settled into walking back.</summary>
    internal static readonly FrozenSet<KraidPartSegment> BeforeFootWalkingBackwardLoop = new[]
    {
        KraidPartSegment.FootInitial, KraidPartSegment.FootNeutral, KraidPartSegment.FootWalkingForward,
        KraidPartSegment.FootWalkingForwardFinished, KraidPartSegment.FootLungeForward, KraidPartSegment.FootLungeForwardFinished,
        KraidPartSegment.FootWalkingBackward,
    }.ToFrozenSet();
}
