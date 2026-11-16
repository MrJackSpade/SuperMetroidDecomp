using P = SuperMetroid.Core.Enemies.Animals.DachoraSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Dachora's run, idle, blink, shinespark charge, echo and fall animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class DachoraAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DachoraSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<DachoraSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.RunningLeft] =
        [
            Show(EnemyFrameId.DachoraRunningLeftFarLegBack, 5),
            Show(EnemyFrameId.DachoraRunningLeftFarLegPassing, 5),
            Show(EnemyFrameId.DachoraRunningLeftFarLegReaching, 5),
            Show(EnemyFrameId.DachoraRunningLeftNearLegBack, 5),
            Show(EnemyFrameId.DachoraRunningLeftNearLegPassing, 5),
            Show(EnemyFrameId.DachoraRunningLeftNearLegReaching, 5),
        ],
        [P.RunningLeftFast] =
        [
            Show(EnemyFrameId.DachoraRunningLeftFarLegBack, 3),
            Show(EnemyFrameId.DachoraRunningLeftFarLegPassing, 3),
            Show(EnemyFrameId.DachoraRunningLeftFarLegReaching, 3),
            Show(EnemyFrameId.DachoraRunningLeftNearLegBack, 3),
            Show(EnemyFrameId.DachoraRunningLeftNearLegPassing, 3),
            Show(EnemyFrameId.DachoraRunningLeftNearLegReaching, 3),
        ],
        [P.RunningLeftVeryFast] =
        [
            Show(EnemyFrameId.DachoraRunningLeftFarLegBack, 1),
            Show(EnemyFrameId.DachoraRunningLeftFarLegPassing, 1),
            Show(EnemyFrameId.DachoraRunningLeftFarLegReaching, 1),
            Show(EnemyFrameId.DachoraRunningLeftNearLegBack, 1),
            Show(EnemyFrameId.DachoraRunningLeftNearLegPassing, 1),
            Show(EnemyFrameId.DachoraRunningLeftNearLegReaching, 1),
        ],
        [P.IdleLeft] =
        [
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadUp, 48),
            Show(EnemyFrameId.DachoraStandingFacingLeftNeckStretched, 10),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadLowered, 7),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadToGround, 7),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadRaising, 7),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadToGround, 7),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadLowered, 7),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadToGround, 7),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadRaising, 7),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadToGround, 7),
            Show(EnemyFrameId.DachoraStandingFacingLeftNeckStretched, 10),
        ],
        [P.BlinkLeft] =
        [
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadUp, 11),
            Show(EnemyFrameId.DachoraStandingFacingLeftEyeHalfShut, 8),
            Show(EnemyFrameId.DachoraStandingFacingLeftEyeShut, 8),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadUp, 4),
            Show(EnemyFrameId.DachoraStandingFacingLeftEyeHalfShut, 4),
            Show(EnemyFrameId.DachoraStandingFacingLeftEyeShut, 4),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadUp, 10),
            Show(EnemyFrameId.DachoraStandingFacingLeftEyeHalfShut, 5),
            Show(EnemyFrameId.DachoraStandingFacingLeftEyeShut, 11),
        ],
        [P.EchoLeft] =
        [
            Show(EnemyFrameId.DachoraStretchedUpFacingLeft, 10),
        ],
        [P.FallingLeft] =
        [
            Show(EnemyFrameId.DachoraRunningLeftNearLegPassing, 5),
        ],
        [P.RunningRight] =
        [
            Show(EnemyFrameId.DachoraRunningRightFarLegBack, 5),
            Show(EnemyFrameId.DachoraRunningRightFarLegPassing, 5),
            Show(EnemyFrameId.DachoraRunningRightFarLegReaching, 5),
            Show(EnemyFrameId.DachoraRunningRightNearLegBack, 5),
            Show(EnemyFrameId.DachoraRunningRightNearLegPassing, 5),
            Show(EnemyFrameId.DachoraRunningRightNearLegReaching, 5),
        ],
        [P.RunningRightFast] =
        [
            Show(EnemyFrameId.DachoraRunningRightFarLegBack, 3),
            Show(EnemyFrameId.DachoraRunningRightFarLegPassing, 3),
            Show(EnemyFrameId.DachoraRunningRightFarLegReaching, 3),
            Show(EnemyFrameId.DachoraRunningRightNearLegBack, 3),
            Show(EnemyFrameId.DachoraRunningRightNearLegPassing, 3),
            Show(EnemyFrameId.DachoraRunningRightNearLegReaching, 3),
        ],
        [P.RunningRightVeryFast] =
        [
            Show(EnemyFrameId.DachoraRunningRightFarLegBack, 1),
            Show(EnemyFrameId.DachoraRunningRightFarLegPassing, 1),
            Show(EnemyFrameId.DachoraRunningRightFarLegReaching, 1),
            Show(EnemyFrameId.DachoraRunningRightNearLegBack, 1),
            Show(EnemyFrameId.DachoraRunningRightNearLegPassing, 1),
            Show(EnemyFrameId.DachoraRunningRightNearLegReaching, 1),
        ],
        [P.IdleRight] =
        [
            Show(EnemyFrameId.DachoraStandingFacingRightHeadUp, 48),
            Show(EnemyFrameId.DachoraStandingFacingRightNeckStretched, 10),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadLowered, 7),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadToGround, 7),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadRaising, 7),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadToGround, 7),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadLowered, 7),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadToGround, 7),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadRaising, 7),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadToGround, 7),
            Show(EnemyFrameId.DachoraStandingFacingRightNeckStretched, 10),
        ],
        [P.BlinkRight] =
        [
            Show(EnemyFrameId.DachoraStandingFacingRightHeadUp, 11),
            Show(EnemyFrameId.DachoraStandingFacingRightEyeHalfShut, 8),
            Show(EnemyFrameId.DachoraStandingFacingRightEyeShut, 8),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadUp, 4),
            Show(EnemyFrameId.DachoraStandingFacingRightEyeHalfShut, 4),
            Show(EnemyFrameId.DachoraStandingFacingRightEyeShut, 4),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadUp, 10),
            Show(EnemyFrameId.DachoraStandingFacingRightEyeHalfShut, 5),
            Show(EnemyFrameId.DachoraStandingFacingRightEyeShut, 11),
        ],
        [P.ChargeRight] =
        [
            Show(EnemyFrameId.DachoraCrouchedFacingRight, 1),
        ],
        [P.EchoRight] =
        [
            Show(EnemyFrameId.DachoraStretchedUpFacingRight, 10),
        ],
        [P.FallingRight] =
        [
            Show(EnemyFrameId.DachoraRunningRightNearLegPassing, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DachoraSegment, Animation<DachoraSegment>> StartingAt { get; } = new Dictionary<DachoraSegment, Animation<DachoraSegment>>
    {
        [P.RunningLeft] = new([new(P.RunningLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningLeftFast] = new([new(P.RunningLeftFast, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningLeftVeryFast] = new([new(P.RunningLeftVeryFast, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IdleLeft] = new([new(P.IdleLeft, 0, 11, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BlinkLeft] = new([new(P.BlinkLeft, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EchoLeft] = new([new(P.EchoLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FallingLeft] = new([new(P.FallingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRight] = new([new(P.RunningRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRightFast] = new([new(P.RunningRightFast, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRightVeryFast] = new([new(P.RunningRightVeryFast, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IdleRight] = new([new(P.IdleRight, 0, 11, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BlinkRight] = new([new(P.BlinkRight, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargeRight] = new([new(P.ChargeRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.EchoRight] = new([new(P.EchoRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FallingRight] = new([new(P.FallingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DachoraSegment> StartingWith(DachoraSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
