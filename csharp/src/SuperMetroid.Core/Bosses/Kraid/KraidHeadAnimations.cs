using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Bosses.Kraid.KraidHeadSegment;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's head animations (#641, #1600), and what collision and growth read at each step of each animation. The head shows
/// a pose for its duration, cries between poses and holds at each animation's end; the head's hold on its pose
/// (<see cref="KraidEnemyState.HeadHold"/>) paces every advance. His fight and death start each animation on its opening pose, which the head enters without showing it, and his
/// growth resumes the roar on one of its poses so (#2399). Collision takes its mouth shapes from the step the head has just
/// passed (<see cref="KraidMouthTrailsHead"/>), growth chooses where the roar resumes by the step the head shows next.
/// </summary>
internal static class KraidHeadAnimations
{
    /// <summary>How long each of the attack roar's opening and closing poses lasts, in updates.</summary>
    private const int RoarPoseDuration = 10;

    /// <summary>How long the attack roar holds its wide-open pose, in updates.</summary>
    private const int RoarOpenDuration = 64;

    /// <summary>How long the eye glow shows the closed mouth before and after it opens, in updates.</summary>
    private const int EyeGlowClosedDuration = 5;

    /// <summary>How long the eye glow shows the mouth open one stage, in updates.</summary>
    private const int EyeGlowOpeningDuration = 10;

    /// <summary>How long each of the death cry's opening poses lasts, in updates.</summary>
    private const int DeathOpeningDuration = 25;

    /// <summary>How long the death cry shows the open mouth before it holds, in updates.</summary>
    private const int DeathOpenDuration = 64;

    /// <summary>Every head segment's beats: its poses for their durations and the cries between them.</summary>
    internal static AnimationSegments<KraidHeadSegment, KraidHeadFrame, KraidHeadCue> Segments { get; } = new(
        new Dictionary<KraidHeadSegment, AnimationBeat<KraidHeadFrame, KraidHeadCue>[]>
    {
        [P.Roar] =
        [
            Show(KraidHeadFrame.Closed, RoarPoseDuration),
            Show(KraidHeadFrame.Opening, RoarPoseDuration),
            Show(KraidHeadFrame.Wider, RoarPoseDuration),
            Cue(KraidHeadCue.Roar),
        ],
        [P.RoarOpenHold] =
        [
            Show(KraidHeadFrame.Open, RoarOpenDuration),
        ],
        [P.RoarClosing] =
        [
            Show(KraidHeadFrame.Wider, RoarPoseDuration),
            Show(KraidHeadFrame.Opening, RoarPoseDuration),
            Show(KraidHeadFrame.Closed, RoarPoseDuration),
        ],
        [P.EyeGlow] =
        [
            Show(KraidHeadFrame.Closed, EyeGlowClosedDuration),
            Show(KraidHeadFrame.Opening, EyeGlowOpeningDuration),
            Show(KraidHeadFrame.Closed, EyeGlowClosedDuration),
        ],
        [P.Death] =
        [
            Show(KraidHeadFrame.Closed, DeathOpeningDuration),
            Show(KraidHeadFrame.Opening, DeathOpeningDuration),
            Show(KraidHeadFrame.Wider, DeathOpeningDuration),
            Cue(KraidHeadCue.DyingCry),
            Show(KraidHeadFrame.Open, DeathOpenDuration),
        ],
    });

    /// <summary>Each head animation, by the segment it starts at: its segments played once each, then a hold on its last pose.</summary>
    private static Dictionary<KraidHeadSegment, Animation<KraidHeadSegment>> StartingAt { get; } = new Dictionary<KraidHeadSegment, Animation<KraidHeadSegment>>
    {
        [P.Roar] = new([Segments.Whole(P.Roar), Segments.Whole(P.RoarOpenHold), Segments.Whole(P.RoarClosing)], AnimationEnd.Holds),
        [P.EyeGlow] = new([Segments.Whole(P.EyeGlow)], AnimationEnd.Holds),
        [P.Death] = new([Segments.Whole(P.Death)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    private static Animation<KraidHeadSegment> StartingWith(KraidHeadSegment segment) => StartingAt[segment];

    /// <summary>
    /// The head starting the animation that starts at <paramref name="animation"/>, as his fight and death install it: it
    /// enters the animation's opening pose without showing it.
    /// </summary>
    internal static KraidHeadEntry Starting(KraidHeadSegment animation) =>
        Entering(new(StartingWith(animation), AnimationPlace.StartOfAnimation));

    /// <summary>
    /// The roar as growth resumes it (#2399): entering the closing pose matching the one the head showed next, or, when it
    /// showed none of those next, the wide-open hold, so that the closing part plays on after it.
    /// </summary>
    internal static KraidHeadEntry GrowthResume(KraidHeadGrowthResume resume)
    {
        Animation<KraidHeadSegment> roar = StartingWith(P.Roar);
        return Entering(new(roar, Segments.PlaceOfFrame(roar, resume.Segment, resume.Frame)));
    }

    /// <summary>
    /// The head entering the pose <paramref name="before"/> stands before, without showing it: the play stands just past
    /// the pose, which holds for its duration.
    /// </summary>
    /// <remarks>A play at rest enters no pose, so the head holds what it shows for no update.</remarks>
    /// <exception cref="InvalidOperationException">The play reaches a cry before a pose.</exception>
    private static KraidHeadEntry Entering(AnimationPlayback<KraidHeadSegment> before)
    {
        AnimationPlace place = before.Place;
        AnimationShown<KraidHeadSegment, KraidHeadFrame> entered = Segments.PlayToNextFrame(before.Animation, ref place,
            cue => throw new InvalidOperationException($"Kraid's head enters a pose with no {cue} before it."));
        return new(before with { Place = place }, entered.Match(EnteredDelays, before));
    }

    /// <summary>
    /// How long the head holds the pose it enters, for each outcome of the play (#2553): a frame or a pause holds for its
    /// duration, and a play that shows a frame for good, rests or ends holds what it shows for no update. Built once.
    /// </summary>
    private static AnimationShownCases<KraidHeadSegment, KraidHeadFrame, AnimationPlayback<KraidHeadSegment>, int> EnteredDelays { get; } = new(
        Frame: static (pose, _) => pose.Duration,
        HeldFrame: static (_, _) => 0,
        Paused: static (pause, _) => pause.Duration,
        Held: static (_, _) => 0,
        Ended: static (_, _) => 0);

    /// <summary>
    /// Where <paramref name="playback"/> stands in its head animation: the animation, named by the segment it starts at, and
    /// the step its play reaches next, counted through its segments, the animation's end standing for a play that is done or
    /// holds there.
    /// </summary>
    private static KraidHeadPlace PlaceOf(AnimationPlayback<KraidHeadSegment> playback)
    {
        IReadOnlyList<AnimationPhase<KraidHeadSegment>> phases = playback.Animation.Phases;
        AnimationPhase<KraidHeadSegment> phase = phases[playback.Place.Phase];
        return new(phases[0].Segment,
            phases.Take(playback.Place.Phase).Sum(before => before.BeatCount) + Math.Clamp(playback.Place.Beat, 0, phase.BeatCount));
    }

    /// <summary>
    /// The pose the mouth tests find at each animation's start, before its first step (<see cref="KraidMouthTrailsHead"/>):
    /// none before the roar, and the closed mouth before the eye glow and the death cry.
    /// </summary>
    private static readonly Dictionary<KraidHeadSegment, KraidHeadFrame?> PoseBeforeStart = new()
    {
        [P.Roar] = null,
        [P.EyeGlow] = KraidHeadFrame.Closed,
        [P.Death] = KraidHeadFrame.Closed,
    };

    /// <summary>What collision and growth read where <paramref name="started"/> stands.</summary>
    internal static KraidHeadAnimationReads ReadsAt(AnimationPlayback<KraidHeadSegment> started)
    {
        KraidHeadPlace place = PlaceOf(started);
        KraidHeadFrame?[] shown = [.. started.Animation.Phases
            .SelectMany(phase => Segments.Beats(phase.Segment).Skip(phase.FirstBeat).Take(phase.BeatCount))
            .Select(beat => beat.ShownFrame)];
        // A step that shows no frame is the roar's or the dying cry's.
        KraidMouthTrailsHead.HeadStep<KraidHeadFrame>[] steps = [.. shown.Select(frame => frame is { } pose
            ? new KraidMouthTrailsHead.HeadStep<KraidHeadFrame>.Posed(pose)
            : KraidMouthTrailsHead.HeadStep<KraidHeadFrame>.Cry)];
        KraidMouthTrailsHead.MouthShapes<KraidHeadFrame> shapes =
            KraidMouthTrailsHead.ShapesAt(steps, place.Step, PoseBeforeStart[place.Animation]);
        return new(Read(shapes.Outer), Read(shapes.Inner), GrowthResumeBefore(place.Step < shown.Length ? shown[place.Step] : null));
    }

    /// <summary>What a mouth test finds: the shape it takes, the closed mouth's missing inner shape, or no pose.</summary>
    private static KraidMouthShapeRead Read(KraidMouthTrailsHead.MouthShape<KraidHeadFrame> shape) => shape switch
    {
        KraidMouthTrailsHead.MouthShape<KraidHeadFrame>.OfPose { Shape: KraidMouthTrailsHead.PoseShape.Inner, Pose: var pose } =>
            KraidMouthShapeRead.InvulnerableOf(pose),
        KraidMouthTrailsHead.MouthShape<KraidHeadFrame>.OfPose { Pose: var pose } => KraidMouthShapeRead.VulnerableOf(pose),
        _ when shape == KraidMouthTrailsHead.MouthShape<KraidHeadFrame>.NoPose => KraidMouthShapeRead.NoPose,
        _ => throw new InvalidOperationException($"Undefined mouth shape {shape}."),
    };

    /// <summary>
    /// Where growth resumes the roar when the head shows <paramref name="next"/> next: a pose the roar's closing part shows
    /// (the closed, opening or wider pose) resumes that part at the pose; the open pose, a cry or an animation's end resume
    /// the roar at the wide-open hold's end.
    /// </summary>
    private static KraidHeadGrowthResume GrowthResumeBefore(KraidHeadFrame? next) =>
        next is { } pose && Segments.Beats(P.RoarClosing).Any(beat => beat.ShownFrame == pose)
            ? new(P.RoarClosing, pose)
            : OpenHoldResume;

    /// <summary>The roar's wide-open hold, where growth resumes the head when it shows none of the closing poses next.</summary>
    private static KraidHeadGrowthResume OpenHoldResume { get; } = new(P.RoarOpenHold, KraidHeadFrame.Open);

    /// <summary>The outer mouth shape body hits are tested against where <paramref name="playback"/> stands.</summary>
    internal static KraidMouthHitbox OuterMouthAt(AnimationPlayback<KraidHeadSegment> playback)
    {
        KraidMouthShapeRead read = ReadsAt(playback).Outer;
        return read is KraidMouthShapeRead.Found found
            ? found.Shape
            : throw new InvalidDataException($"Kraid's outer mouth read at {PlaceOf(playback)} finds {read.Kind}, not a mouth shape.");
    }

    /// <summary>The inner mouth shape mouth hits are tested against where <paramref name="playback"/> stands, or null when the pose has none.</summary>
    internal static KraidMouthHitbox? InnerMouthAt(AnimationPlayback<KraidHeadSegment> playback)
    {
        KraidMouthShapeRead read = ReadsAt(playback).Inner;
        return read.Kind switch
        {
            KraidMouthShapeReadKind.Shape => ((KraidMouthShapeRead.Found)read).Shape,
            KraidMouthShapeReadKind.NoShape => null,
            KraidMouthShapeReadKind.NoPose => throw new InvalidDataException(
                $"Kraid's inner mouth read at {(playback is { } head ? PlaceOf(head).ToString() : "no place")} finds no head pose."),
            _ => throw new InvalidOperationException($"Undefined {nameof(KraidMouthShapeReadKind)} {read.Kind}."),
        };
    }

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<KraidHeadFrame, KraidHeadCue> Show(KraidHeadFrame frame, int duration) =>
        new AnimationBeat<KraidHeadFrame, KraidHeadCue>.Frame(frame, duration);

    /// <summary>Announces <paramref name="cue"/>.</summary>
    private static AnimationBeat<KraidHeadFrame, KraidHeadCue> Cue(KraidHeadCue cue) =>
        new AnimationBeat<KraidHeadFrame, KraidHeadCue>.Event(cue);
}
