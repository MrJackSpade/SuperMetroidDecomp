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
        [P.RunningLeft] = Stride(DachoraStride.Left, 5),
        [P.RunningLeftFast] = Stride(DachoraStride.Left, 3),
        [P.RunningLeftVeryFast] = Stride(DachoraStride.Left, 1),
        [P.IdleLeft] =
        [
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadUp, 48),
            Show(EnemyFrameId.DachoraStandingFacingLeftNeckStretched, 10),
        ],
        [P.PeckLeft] =
        [
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadLowered, 7),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadToGround, 7),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadRaising, 7),
            Show(EnemyFrameId.DachoraStandingFacingLeftHeadToGround, 7),
        ],
        [P.RisingFromPecksLeft] = [Show(EnemyFrameId.DachoraStandingFacingLeftNeckStretched, 10)],
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
        [P.RunningRight] = Stride(DachoraStride.Right, 5),
        [P.RunningRightFast] = Stride(DachoraStride.Right, 3),
        [P.RunningRightVeryFast] = Stride(DachoraStride.Right, 1),
        [P.IdleRight] =
        [
            Show(EnemyFrameId.DachoraStandingFacingRightHeadUp, 48),
            Show(EnemyFrameId.DachoraStandingFacingRightNeckStretched, 10),
        ],
        [P.PeckRight] =
        [
            Show(EnemyFrameId.DachoraStandingFacingRightHeadLowered, 7),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadToGround, 7),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadRaising, 7),
            Show(EnemyFrameId.DachoraStandingFacingRightHeadToGround, 7),
        ],
        [P.RisingFromPecksRight] = [Show(EnemyFrameId.DachoraStandingFacingRightNeckStretched, 10)],
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

    /// <summary>How many times an idle Dachora pecks at the ground before it stretches back up.</summary>
    private const int IdlePecks = 2;

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. An idle Dachora stands, stretches its
    /// neck, pecks <see cref="IdlePecks"/> times and stretches back up, repeating (#2432). The echoes, the falls and the
    /// charge's crouch are still pictures (#2275).
    /// </summary>
    private static Dictionary<DachoraSegment, Animation<DachoraSegment>> StartingAt { get; } = new Dictionary<DachoraSegment, Animation<DachoraSegment>>
    {
        [P.RunningLeft] = Animation<DachoraSegment>.Looping([Segments.Whole(P.RunningLeft)]),
        [P.RunningLeftFast] = Animation<DachoraSegment>.Looping([Segments.Whole(P.RunningLeftFast)]),
        [P.RunningLeftVeryFast] = Animation<DachoraSegment>.Looping([Segments.Whole(P.RunningLeftVeryFast)]),
        [P.IdleLeft] = Animation<DachoraSegment>.Looping([Segments.Whole(P.IdleLeft), Segments.Repeated(P.PeckLeft, IdlePecks), Segments.Whole(P.RisingFromPecksLeft)]),
        [P.BlinkLeft] = Animation<DachoraSegment>.Looping([Segments.Whole(P.BlinkLeft)]),
        [P.EchoLeft] = new([Segments.Whole(P.EchoLeft)], AnimationEnd.Holds),
        [P.FallingLeft] = new([Segments.Whole(P.FallingLeft)], AnimationEnd.Holds),
        [P.RunningRight] = Animation<DachoraSegment>.Looping([Segments.Whole(P.RunningRight)]),
        [P.RunningRightFast] = Animation<DachoraSegment>.Looping([Segments.Whole(P.RunningRightFast)]),
        [P.RunningRightVeryFast] = Animation<DachoraSegment>.Looping([Segments.Whole(P.RunningRightVeryFast)]),
        [P.IdleRight] = Animation<DachoraSegment>.Looping([Segments.Whole(P.IdleRight), Segments.Repeated(P.PeckRight, IdlePecks), Segments.Whole(P.RisingFromPecksRight)]),
        [P.BlinkRight] = Animation<DachoraSegment>.Looping([Segments.Whole(P.BlinkRight)]),
        [P.ChargeRight] = new([Segments.Whole(P.ChargeRight)], AnimationEnd.Holds),
        [P.EchoRight] = new([Segments.Whole(P.EchoRight)], AnimationEnd.Holds),
        [P.FallingRight] = new([Segments.Whole(P.FallingRight)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DachoraSegment> StartingWith(DachoraSegment segment) => StartingAt[segment];

    /// <summary>
    /// One <paramref name="stride"/> (#2477), each of its poses shown for <paramref name="updates"/> updates: the runs at every
    /// pace play the same poses, so a run that quickens keeps its place in them.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] Stride(DachoraStride stride, int updates) =>
        [.. stride.Poses.Select(pose => Show(pose, updates))];

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
