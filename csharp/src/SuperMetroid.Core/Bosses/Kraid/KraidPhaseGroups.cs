using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The phases Kraid's encounter tests as groups (#1452, #1737), each named for what its test decides in the fight: whether
/// he is fighting, whether shots collide with him and whether he is rising through the floor. Kraid's rise through the
/// floor is not part of the fight: a killing hit during it does not start his death and his body does not hurt Samus.
/// </summary>
internal static class KraidPhaseGroups
{
    /// <summary>
    /// The phases in which Kraid fights: combat, growth and his parts' own behaviour, their rest included. Only in these
    /// does his body's touch hurt Samus and a mouth hit that leaves him without health start his death; his death
    /// sequence and his rise through the floor lie outside it.
    /// </summary>
    internal static readonly FrozenSet<KraidPhase> Fighting = new[]
    {
        KraidPhase.Resting,
        KraidPhase.GrowBreakCeilingPlatforms,
        KraidPhase.GrowBringBodyInFront, KraidPhase.GrowSettleParts, KraidPhase.GrowShowRoomBackdrop,
        KraidPhase.GrowFadeInRoomBackground, KraidPhase.MainloopThinking, KraidPhase.SecondPhaseThinking,
        KraidPhase.MouthOpenReaction, KraidPhase.InitializeEyeGlow, KraidPhase.GlowEye, KraidPhase.UnglowEye,
        KraidPhase.LintInactive, KraidPhase.LintProduce, KraidPhase.LintCharge, KraidPhase.LintFire,
        KraidPhase.FingernailWaitForLint, KraidPhase.AlignPartToKraid, KraidPhase.WaitToResumePhase,
        KraidPhase.FootWaitBeforeWalk, KraidPhase.FootFirstPhaseThinking,
        KraidPhase.AdvanceHeadAnimationAndTimer, KraidPhase.FootSecondPhaseThinking,
        KraidPhase.FootSecondPhaseWalking, KraidPhase.FootSecondPhaseWalkToStart,
        KraidPhase.FootSecondPhaseInitialize,
        KraidPhase.MainAttackWithMouthOpen, KraidPhase.FingernailInitialize, KraidPhase.FingernailFire,
        KraidPhase.FootPrepareFirstPhaseLunge, KraidPhase.FootFirstPhaseLunge, KraidPhase.FootFirstPhaseRetreat,
        KraidPhase.GrowReleaseCamera,
    }.ToFrozenSet();

    /// <summary>
    /// The phases in which Samus's shots collide with Kraid: while he fights, and through his death's fade, the update
    /// that shows his body and the one that starts his sinking. While he rises through the floor and once he
    /// sinks, his mouth and body passes let every shot through.
    /// </summary>
    internal static readonly FrozenSet<KraidPhase> ShotsCollide = Fighting.Concat(new[]
    {
        KraidPhase.DeathInitialize, KraidPhase.DeathFadeOut, KraidPhase.DeathShowBody, KraidPhase.DeathStartSinking,
    }).ToFrozenSet();

    /// <summary>The phases in which Kraid rises through the floor at the start of the encounter, which run his rise instead of the fight.</summary>
    internal static readonly FrozenSet<KraidPhase> Rising = new[]
    {
        KraidPhase.RestrictSamusToFirstScreen, KraidPhase.RaiseShowBody, KraidPhase.RaiseStartQuakeAndMusic,
        KraidPhase.RaiseRocksEvery16Frames, KraidPhase.RaiseRocksEvery8Frames, KraidPhase.RaiseBody,
    }.ToFrozenSet();
}
