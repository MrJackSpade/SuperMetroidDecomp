using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Evir.EvirCueKind;
using P = SuperMetroid.Core.Enemies.Evir.EvirSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The Evir body's and arms' facing loops and its projectile's spit and regeneration animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="EvirCue"/> naming what it does.
/// </summary>
internal static class EvirAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EvirSegment, EnemyFrameId, EvirCue> Segments { get; } = new(
        new Dictionary<EvirSegment, AnimationBeat<EnemyFrameId, EvirCue>[]>
    {
        [P.BodyFacingLeft] =
        [
            Show(EnemyFrameId.EvirBodyFacingLeftLegsTucked, 10),
            Show(EnemyFrameId.EvirBodyFacingLeftLegsCurling, 10),
            Show(EnemyFrameId.EvirBodyFacingLeftLegsLowering, 10),
            Show(EnemyFrameId.EvirBodyFacingLeftLegsHanging, 10),
            Show(EnemyFrameId.EvirBodyFacingLeftLegsStretched, 10),
            Show(EnemyFrameId.EvirBodyFacingLeftLegsLifting, 10),
        ],
        [P.ArmsFacingLeft] =
        [
            Show(EnemyFrameId.EvirArmsFacingLeftReaching, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftCurling, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFolding, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftSpreading, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftReaching, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftCurling, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFolding, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftSpreading, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftReaching, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftCurling, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFolding, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftSpreading, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftReaching, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftCurling, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFolding, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftSpreading, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFolded, 48),
        ],
        [P.BodyFacingRight] =
        [
            Show(EnemyFrameId.EvirBodyFacingRightLegsTucked, 10),
            Show(EnemyFrameId.EvirBodyFacingRightLegsCurling, 10),
            Show(EnemyFrameId.EvirBodyFacingRightLegsLowering, 10),
            Show(EnemyFrameId.EvirBodyFacingRightLegsHanging, 10),
            Show(EnemyFrameId.EvirBodyFacingRightLegsStretched, 10),
            Show(EnemyFrameId.EvirBodyFacingRightLegsLifting, 10),
        ],
        [P.ArmsFacingRight] =
        [
            Show(EnemyFrameId.EvirArmsFacingRightReaching, 10),
            Show(EnemyFrameId.EvirArmsFacingRightCurling, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFolding, 10),
            Show(EnemyFrameId.EvirArmsFacingRightSpreading, 10),
            Show(EnemyFrameId.EvirArmsFacingRightReaching, 10),
            Show(EnemyFrameId.EvirArmsFacingRightCurling, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFolding, 10),
            Show(EnemyFrameId.EvirArmsFacingRightSpreading, 10),
            Show(EnemyFrameId.EvirArmsFacingRightReaching, 10),
            Show(EnemyFrameId.EvirArmsFacingRightCurling, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFolding, 10),
            Show(EnemyFrameId.EvirArmsFacingRightSpreading, 10),
            Show(EnemyFrameId.EvirArmsFacingRightReaching, 10),
            Show(EnemyFrameId.EvirArmsFacingRightCurling, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFolding, 10),
            Show(EnemyFrameId.EvirArmsFacingRightSpreading, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFolded, 48),
        ],
        [P.ProjectileNormal] =
        [
            Show(EnemyFrameId.EvirSpineBall, 1),
        ],
        [P.ProjectileRegenerating] =
        [
            Cue(new(K.SetInitialRegenerationOffset)),
            Cue(new(K.PlaySpitSound)),
        ],
        [P.ProjectileRegenerationLoop] =
        [
            Show(EnemyFrameId.EvirSpineBallRegrowing, 8),
            Cue(new(K.AdvanceRegenerationOffset)),
            Show(EnemyFrameId.EvirSpineBall, 16),
            Cue(new(K.FinishRegeneration)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EvirSegment, Animation<EvirSegment>> StartingAt { get; } = new Dictionary<EvirSegment, Animation<EvirSegment>>
    {
        [P.BodyFacingLeft] = new([new(P.BodyFacingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmsFacingLeft] = new([new(P.ArmsFacingLeft, 0, 17, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BodyFacingRight] = new([new(P.BodyFacingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmsFacingRight] = new([new(P.ArmsFacingRight, 0, 17, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ProjectileNormal] = new([new(P.ProjectileNormal, 0, 1, 1)], AnimationEnd.Holds),
        [P.ProjectileRegenerating] = new([new(P.ProjectileRegenerating, 0, 1, 1), new(P.ProjectileRegenerating, 1, 1, 1), new(P.ProjectileRegenerationLoop, 0, 2, 8), new(P.ProjectileRegenerationLoop, 2, 2, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EvirSegment> StartingWith(EvirSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EvirCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EvirCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EvirCue> Cue(EvirCue cue) => new AnimationBeat<EnemyFrameId, EvirCue>.Event(cue);
}
