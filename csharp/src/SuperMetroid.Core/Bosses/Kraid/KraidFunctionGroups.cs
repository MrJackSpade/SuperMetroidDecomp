using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The AI functions and part programs Kraid's encounter tests as groups (#1452). The cartridge compares a function or
/// instruction-list pointer against one routine's or list's address, so each test admits what it lays out on one side
/// of it; each group names those members instead of ordering the identities. The rise functions are laid out after the
/// death functions, so a test for "the death sequence has begun" also admits them.
/// </summary>
internal static class KraidFunctionGroups
{
    /// <summary>The functions laid out before the death sequence's first routine: combat, growth and the parts' AI.</summary>
    internal static readonly FrozenSet<KraidAiFunction> BeforeDeath = new[]
    {
        KraidAiFunction.None, KraidAiFunction.NoOperation, KraidAiFunction.GrowBreakCeilingPlatforms,
        KraidAiFunction.GrowSetBg2Priority, KraidAiFunction.GrowFinishBg2Update, KraidAiFunction.GrowDrawRoomBackground,
        KraidAiFunction.GrowFadeInRoomBackground, KraidAiFunction.MainloopThinking, KraidAiFunction.SecondPhaseThinking,
        KraidAiFunction.MouthOpenReaction, KraidAiFunction.InitializeEyeGlow, KraidAiFunction.GlowEye, KraidAiFunction.UnglowEye,
        KraidAiFunction.LintInactive, KraidAiFunction.LintProduce, KraidAiFunction.LintCharge, KraidAiFunction.LintFire,
        KraidAiFunction.FingernailWaitForLint, KraidAiFunction.AlignPartToKraid, KraidAiFunction.HandleFunctionTimer,
        KraidAiFunction.DecrementFunctionTimerAndStartWalk, KraidAiFunction.FootFirstPhaseThinking,
        KraidAiFunction.ProcessHeadInstructionAndTimer, KraidAiFunction.FootSecondPhaseThinking,
        KraidAiFunction.FootSecondPhaseWalkingRight, KraidAiFunction.FootSecondPhaseWalkToStart,
        KraidAiFunction.FootSecondPhaseInitialize, KraidAiFunction.FootSecondPhaseWalkingLeft,
        KraidAiFunction.MainAttackWithMouthOpen, KraidAiFunction.FingernailInitialize, KraidAiFunction.FingernailFire,
        KraidAiFunction.FootPrepareFirstPhaseLunge, KraidAiFunction.FootFirstPhaseLunge, KraidAiFunction.FootFirstPhaseRetreat,
        KraidAiFunction.GrowReleaseCamera,
    }.ToFrozenSet();

    /// <summary>The functions laid out before Kraid starts sinking: everything before death and the death sequence's fade and tilemap updates.</summary>
    internal static readonly FrozenSet<KraidAiFunction> BeforeSinking = BeforeDeath.Concat(new[]
    {
        KraidAiFunction.DeathInitialize, KraidAiFunction.DeathFadeOut, KraidAiFunction.DeathUpdateTopTilemap,
        KraidAiFunction.DeathUpdateBottomTilemap,
    }).ToFrozenSet();

    /// <summary>Kraid rising through the floor at the start of the encounter.</summary>
    internal static readonly FrozenSet<KraidAiFunction> Rise = new[]
    {
        KraidAiFunction.RestrictSamusToFirstScreen, KraidAiFunction.RaiseKraidThroughFloor, KraidAiFunction.RaiseLoadBottomTilemap,
        KraidAiFunction.RaiseRocksEvery16Frames, KraidAiFunction.RaiseRocksEvery8Frames, KraidAiFunction.RaiseBody,
    }.ToFrozenSet();

    /// <summary>The part programs laid out before the slow arm swing: the feet's and the normal arm's.</summary>
    internal static readonly FrozenSet<KraidPartProgram> BeforeArmSlow = new[]
    {
        KraidPartProgram.FootInitial, KraidPartProgram.FootNeutral, KraidPartProgram.FootWalkingForward,
        KraidPartProgram.FootWalkingForwardFinished, KraidPartProgram.FootLungeForward, KraidPartProgram.FootLungeForwardFinished,
        KraidPartProgram.FootWalkingBackward, KraidPartProgram.FootWalkingBackwardLoop, KraidPartProgram.ArmNormal,
        KraidPartProgram.ArmNormalPause,
    }.ToFrozenSet();

    /// <summary>The part programs laid out before the normal arm's pause: the feet's and the normal arm swing.</summary>
    internal static readonly FrozenSet<KraidPartProgram> BeforeArmNormalPause = new[]
    {
        KraidPartProgram.FootInitial, KraidPartProgram.FootNeutral, KraidPartProgram.FootWalkingForward,
        KraidPartProgram.FootWalkingForwardFinished, KraidPartProgram.FootLungeForward, KraidPartProgram.FootLungeForwardFinished,
        KraidPartProgram.FootWalkingBackward, KraidPartProgram.FootWalkingBackwardLoop, KraidPartProgram.ArmNormal,
    }.ToFrozenSet();

    /// <summary>The foot programs laid out before the backward walk's loop: the foot has not yet settled into walking back.</summary>
    internal static readonly FrozenSet<KraidPartProgram> BeforeFootWalkingBackwardLoop = new[]
    {
        KraidPartProgram.FootInitial, KraidPartProgram.FootNeutral, KraidPartProgram.FootWalkingForward,
        KraidPartProgram.FootWalkingForwardFinished, KraidPartProgram.FootLungeForward, KraidPartProgram.FootLungeForwardFinished,
        KraidPartProgram.FootWalkingBackward,
    }.ToFrozenSet();
}
