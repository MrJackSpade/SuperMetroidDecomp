using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Evir.EvirCueKind;
using P = SuperMetroid.Core.Enemies.Evir.EvirSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The Evir body's and arms' facing loops and its projectile's spit and regeneration animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="EvirCue"/> naming what it does.
/// </summary>
internal static partial class EvirAnimations
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
        ],
        [P.ArmsFoldedFacingLeft] =
        [
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
        ],
        [P.ArmsFoldedFacingRight] =
        [
            Show(EnemyFrameId.EvirArmsFacingRightFolded, 48),
        ],
        [P.ProjectileIdle] = [new AnimationBeat<EnemyFrameId, EvirCue>.HeldFrame(EnemyFrameId.EvirSpineBall)],
        [P.ProjectileRegenerating] =
        [
            Cue(new(K.SetInitialRegenerationOffset)),
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

    /// <summary>How many times the arms wave before they rest folded.</summary>
    private const int ArmWaves = 4;

    /// <summary>How many times a regenerating spit regrows a step out from the mouth.</summary>
    private const int RegrowthSteps = 8;

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2275, #2369): the arms wave (<see cref="ArmWaves"/>)
    /// and rest folded, the spit is a still picture,
    /// and a regenerating spit starts its own regrowth, sounds, regrows its steps out from the mouth (<see cref="RegrowthSteps"/>) and is whole again.
    /// </summary>
    internal static Dictionary<EvirSegment, Animation<EvirSegment>> StartingAt { get; } = new Dictionary<EvirSegment, Animation<EvirSegment>>
    {
        [P.BodyFacingLeft] = Animation<EvirSegment>.Looping([Segments.Whole(P.BodyFacingLeft)]),
        [P.ArmsFacingLeft] = Animation<EvirSegment>.Looping([Segments.Repeated(P.ArmsFacingLeft, ArmWaves), Segments.Whole(P.ArmsFoldedFacingLeft)]),
        [P.BodyFacingRight] = Animation<EvirSegment>.Looping([Segments.Whole(P.BodyFacingRight)]),
        [P.ArmsFacingRight] = Animation<EvirSegment>.Looping([Segments.Repeated(P.ArmsFacingRight, ArmWaves), Segments.Whole(P.ArmsFoldedFacingRight)]),
        [P.ProjectileIdle] = new([Segments.Whole(P.ProjectileIdle)], AnimationEnd.Holds),
        [P.ProjectileRegenerating] = new(
            [Segments.Whole(P.ProjectileRegenerating), Segments.Repeated(P.ProjectileRegrowing, RegrowthSteps), Segments.Whole(P.ProjectileRegrown)],
            AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EvirSegment> StartingWith(EvirSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EvirCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EvirCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EvirCue> Cue(EvirCue cue) => new AnimationBeat<EnemyFrameId, EvirCue>.Event(cue);
}
