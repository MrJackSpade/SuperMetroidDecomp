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
        [P.EchoLeft] = [Hold(EnemyFrameId.DachoraStretchedUpFacingLeft)],
        [P.FallingLeft] = [Hold(EnemyFrameId.DachoraRunningLeftNearLegPassing)],
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
        [P.ChargeRight] = [Hold(EnemyFrameId.DachoraCrouchedFacingRight)],
        [P.EchoRight] = [Hold(EnemyFrameId.DachoraStretchedUpFacingRight)],
        [P.FallingRight] = [Hold(EnemyFrameId.DachoraRunningRightNearLegPassing)],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. The echoes, the falls and the charge's
    /// crouch are still pictures (#2275).
    /// </summary>
    internal static IReadOnlyDictionary<DachoraSegment, Animation<DachoraSegment>> StartingAt { get; } = new Dictionary<DachoraSegment, Animation<DachoraSegment>>
    {
        [P.RunningLeft] = new([Segments.Whole(P.RunningLeft)], AnimationEnd.LoopsFrom(0)),
        [P.RunningLeftFast] = new([Segments.Whole(P.RunningLeftFast)], AnimationEnd.LoopsFrom(0)),
        [P.RunningLeftVeryFast] = new([Segments.Whole(P.RunningLeftVeryFast)], AnimationEnd.LoopsFrom(0)),
        [P.IdleLeft] = new([Segments.Whole(P.IdleLeft)], AnimationEnd.LoopsFrom(0)),
        [P.BlinkLeft] = new([Segments.Whole(P.BlinkLeft)], AnimationEnd.LoopsFrom(0)),
        [P.EchoLeft] = new([Segments.Whole(P.EchoLeft)], AnimationEnd.Holds),
        [P.FallingLeft] = new([Segments.Whole(P.FallingLeft)], AnimationEnd.Holds),
        [P.RunningRight] = new([Segments.Whole(P.RunningRight)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRightFast] = new([Segments.Whole(P.RunningRightFast)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRightVeryFast] = new([Segments.Whole(P.RunningRightVeryFast)], AnimationEnd.LoopsFrom(0)),
        [P.IdleRight] = new([Segments.Whole(P.IdleRight)], AnimationEnd.LoopsFrom(0)),
        [P.BlinkRight] = new([Segments.Whole(P.BlinkRight)], AnimationEnd.LoopsFrom(0)),
        [P.ChargeRight] = new([Segments.Whole(P.ChargeRight)], AnimationEnd.Holds),
        [P.EchoRight] = new([Segments.Whole(P.EchoRight)], AnimationEnd.Holds),
        [P.FallingRight] = new([Segments.Whole(P.FallingRight)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DachoraSegment> StartingWith(DachoraSegment segment) => StartingAt[segment];

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
