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
        [P.IdleLeft] = Idle(DachoraStance.Left),
        [P.PeckLeft] = Peck(DachoraStance.Left),
        [P.RisingFromPecksLeft] = [Show(DachoraStance.Left.NeckStretched, 10)],
        [P.BlinkLeft] = Blink(DachoraStance.Left),
        [P.EchoLeft] = [Hold(EnemyFrameId.DachoraStretchedUpFacingLeft)],
        [P.FallingLeft] = [Hold(EnemyFrameId.DachoraRunningLeftNearLegPassing)],
        [P.RunningRight] = Stride(DachoraStride.Right, 5),
        [P.RunningRightFast] = Stride(DachoraStride.Right, 3),
        [P.RunningRightVeryFast] = Stride(DachoraStride.Right, 1),
        [P.IdleRight] = Idle(DachoraStance.Right),
        [P.PeckRight] = Peck(DachoraStance.Right),
        [P.RisingFromPecksRight] = [Show(DachoraStance.Right.NeckStretched, 10)],
        [P.BlinkRight] = Blink(DachoraStance.Right),
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
    internal static Dictionary<DachoraSegment, Animation<DachoraSegment>> StartingAt { get; } = new Dictionary<DachoraSegment, Animation<DachoraSegment>>
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

    /// <summary>An idle Dachora standing with its head up, then stretching its neck, facing as <paramref name="stance"/> does.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] Idle(DachoraStance stance) =>
    [
        Show(stance.HeadUp, 48),
        Show(stance.NeckStretched, 10),
    ];

    /// <summary>One peck at the ground, facing as <paramref name="stance"/> does.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] Peck(DachoraStance stance) =>
    [
        Show(stance.HeadLowered, 7),
        Show(stance.HeadToGround, 7),
        Show(stance.HeadRaising, 7),
        Show(stance.HeadToGround, 7),
    ];

    /// <summary>A standing Dachora's three blinks, facing as <paramref name="stance"/> does.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] Blink(DachoraStance stance) =>
    [
        Show(stance.HeadUp, 11),
        Show(stance.EyeHalfShut, 8),
        Show(stance.EyeShut, 8),
        Show(stance.HeadUp, 4),
        Show(stance.EyeHalfShut, 4),
        Show(stance.EyeShut, 4),
        Show(stance.HeadUp, 10),
        Show(stance.EyeHalfShut, 5),
        Show(stance.EyeShut, 11),
    ];

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
