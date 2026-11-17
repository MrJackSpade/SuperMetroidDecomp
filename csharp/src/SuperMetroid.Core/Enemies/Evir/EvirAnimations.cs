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
        [P.ProjectileNormal] = [new AnimationBeat<EnemyFrameId, EvirCue>.HeldFrame(EnemyFrameId.EvirSpineBall)],
        [P.ProjectileRegenerating] =
        [
            Cue(new(K.SetInitialRegenerationOffset)),
        ],
        [P.ProjectileRegeneratingSound] =
        [
            Cue(new(K.PlaySpitSound)),
        ],
        [P.ProjectileRegrowing] =
        [
            Show(EnemyFrameId.EvirSpineBallRegrowing, 8),
            Cue(new(K.AdvanceRegenerationOffset)),
        ],
        [P.ProjectileRegrown] =
        [
            Show(EnemyFrameId.EvirSpineBall, 16),
            Cue(new(K.FinishRegeneration)),
        ],
    });

    /// <summary>How many times a regenerating spit regrows a step out from the mouth.</summary>
    private const int RegrowthSteps = 8;

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2275): the spit is a still picture,
    /// and a regenerating spit starts its own regrowth, sounds, regrows eight steps out from the mouth and is whole again.
    /// </summary>
    internal static IReadOnlyDictionary<EvirSegment, Animation<EvirSegment>> StartingAt { get; } = new Dictionary<EvirSegment, Animation<EvirSegment>>
    {
        [P.BodyFacingLeft] = new([Whole(P.BodyFacingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.ArmsFacingLeft] = new([Whole(P.ArmsFacingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.BodyFacingRight] = new([Whole(P.BodyFacingRight)], AnimationEnd.LoopsFrom(0)),
        [P.ArmsFacingRight] = new([Whole(P.ArmsFacingRight)], AnimationEnd.LoopsFrom(0)),
        [P.ProjectileNormal] = new([Whole(P.ProjectileNormal)], AnimationEnd.Holds),
        [P.ProjectileRegenerating] = new(
            [Whole(P.ProjectileRegenerating), Whole(P.ProjectileRegeneratingSound), Repeated(P.ProjectileRegrowing, RegrowthSteps), Whole(P.ProjectileRegrown)],
            AnimationEnd.Holds),
    };

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<EvirSegment> Whole(EvirSegment segment) => Repeated(segment, 1);

    /// <summary><paramref name="plays"/> plays of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<EvirSegment> Repeated(EvirSegment segment, int plays) => new(segment, 0, Segments.Beats(segment).Count, plays);

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EvirSegment> StartingWith(EvirSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EvirCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EvirCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EvirCue> Cue(EvirCue cue) => new AnimationBeat<EnemyFrameId, EvirCue>.Event(cue);
}
