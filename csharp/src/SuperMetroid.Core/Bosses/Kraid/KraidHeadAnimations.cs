using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Compatibility;
using P = SuperMetroid.Core.Bosses.Kraid.KraidHeadSegment;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's head animations (#641, #1600), and what collision and growth read at each step of each animation. The head shows
/// a pose for its duration, cries between poses and holds at each animation's end; the body's head timer gates every
/// advance. Collision takes its mouth shapes from the step the head has just passed (<see cref="KraidMouthTrailsHead"/>),
/// growth chooses where the roar resumes by the step the head shows next. Checked against the game's head animations by
/// the head-animation verification.
/// </summary>
internal static class KraidHeadAnimations
{
    /// <summary>How long the attack roar's entry pose lasts before its animation first advances, in updates.</summary>
    internal const int RoarEntryDuration = 10;

    /// <summary>How long the eye glow's entry pose lasts before its animation first advances, in updates.</summary>
    internal const int EyeGlowEntryDuration = 5;

    /// <summary>How long the death cry's entry pose lasts before its animation first advances, in updates.</summary>
    internal const int DeathEntryDuration = 25;

    /// <summary>How long the attack roar's closing poses last, in updates; growth restarts the head with this delay.</summary>
    private const int RoarClosingDuration = 10;

    /// <summary>How long the attack roar holds its wide-open pose, in updates; growth restarts the head with this delay.</summary>
    private const int RoarOpenDuration = 64;

    /// <summary>
    /// The attack roar, past its entry pose. Each installer starts its animation here with the entry pose's duration and
    /// leaves the head image as it was.
    /// </summary>
    internal static AnimationPlayback<KraidHeadSegment> RoarAfterEntryFrame => At(P.Roar, 1);

    /// <summary>The eye glow, past its entry pose.</summary>
    internal static AnimationPlayback<KraidHeadSegment> EyeGlowAfterEntryFrame => At(P.EyeGlow, 1);

    /// <summary>The death cry, past its entry pose.</summary>
    internal static AnimationPlayback<KraidHeadSegment> DeathAfterEntryFrame => At(P.Death, 1);

    /// <summary>Every head segment's beats: its poses for their durations and the cries between them.</summary>
    internal static AnimationSegments<KraidHeadSegment, KraidHeadFrame, KraidHeadCue> Segments { get; } = new(
        new Dictionary<KraidHeadSegment, AnimationBeat<KraidHeadFrame, KraidHeadCue>[]>
    {
        [P.Roar] =
        [
            Show(KraidHeadFrame.Closed, RoarClosingDuration),
            Show(KraidHeadFrame.Opening, RoarClosingDuration),
            Show(KraidHeadFrame.Wider, RoarClosingDuration),
            Cue(KraidHeadCue.Roar),
            Show(KraidHeadFrame.Open, RoarOpenDuration),
            Show(KraidHeadFrame.Wider, RoarClosingDuration),
            Show(KraidHeadFrame.Opening, RoarClosingDuration),
            Show(KraidHeadFrame.Closed, RoarClosingDuration),
        ],
        [P.DyingRoar] =
        [
            Show(KraidHeadFrame.Closed, 20),
            Show(KraidHeadFrame.Opening, 20),
            Show(KraidHeadFrame.Wider, 20),
            Cue(KraidHeadCue.Roar),
            Show(KraidHeadFrame.Open, 192),
            Show(KraidHeadFrame.Wider, 20),
            Show(KraidHeadFrame.Opening, 20),
            Show(KraidHeadFrame.Closed, 20),
        ],
        [P.EyeGlow] =
        [
            Show(KraidHeadFrame.Closed, EyeGlowEntryDuration),
            Show(KraidHeadFrame.Opening, 10),
            Show(KraidHeadFrame.Closed, EyeGlowEntryDuration),
        ],
        [P.Death] =
        [
            Show(KraidHeadFrame.Closed, DeathEntryDuration),
            Show(KraidHeadFrame.Opening, DeathEntryDuration),
            Show(KraidHeadFrame.Wider, DeathEntryDuration),
            Cue(KraidHeadCue.DyingCry),
            Show(KraidHeadFrame.Open, 64),
        ],
    });

    /// <summary>Each head animation: its segment played once, then a hold on its last pose.</summary>
    internal static IReadOnlyDictionary<KraidHeadSegment, Animation<KraidHeadSegment>> StartingAt { get; } = new Dictionary<KraidHeadSegment, Animation<KraidHeadSegment>>
    {
        [P.Roar] = new([new(P.Roar, 0, 8, 1)], AnimationEnd.Holds),
        [P.DyingRoar] = new([new(P.DyingRoar, 0, 8, 1)], AnimationEnd.Holds),
        [P.EyeGlow] = new([new(P.EyeGlow, 0, 3, 1)], AnimationEnd.Holds),
        [P.Death] = new([new(P.Death, 0, 5, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KraidHeadSegment> StartingWith(KraidHeadSegment segment) => StartingAt[segment];

    /// <summary>
    /// The head animation of <paramref name="segment"/> standing before beat <paramref name="position"/> of its one phase;
    /// at the beat count, its play is done and the next play holds.
    /// </summary>
    internal static AnimationPlayback<KraidHeadSegment> At(KraidHeadSegment segment, int position) =>
        new(StartingWith(segment), new AnimationPlace(0, position));

    /// <summary>
    /// Where <paramref name="playback"/> stands in its head animation: the segment its one phase plays, and the step its play
    /// reaches next, the animation's end standing for a play that is done or holds there.
    /// </summary>
    internal static KraidHeadSegmentPlace PlaceOf(AnimationPlayback<KraidHeadSegment> playback)
    {
        AnimationPhase<KraidHeadSegment> phase = playback.Animation.Phases[playback.Place.Phase];
        return new(phase.Segment, phase.FirstBeat + Math.Clamp(playback.Place.Beat, 0, phase.BeatCount));
    }

    /// <summary>
    /// The last pose of the head animation laid out before each animation, which the mouth tests find at its start
    /// (<see cref="KraidMouthTrailsHead"/>): every animation but the roar, laid out first, follows one that ends with the
    /// mouth closed.
    /// </summary>
    private static readonly Dictionary<KraidHeadSegment, KraidHeadFrame?> PoseLaidOutBefore = new()
    {
        [P.Roar] = null,
        [P.DyingRoar] = KraidHeadFrame.Closed,
        [P.EyeGlow] = KraidHeadFrame.Closed,
        [P.Death] = KraidHeadFrame.Closed,
    };

    /// <summary>What collision and growth read where <paramref name="playback"/> stands; a head that never started has no pose to read.</summary>
    internal static KraidHeadCursorReads ReadsAt(AnimationPlayback<KraidHeadSegment>? playback)
    {
        AnimationPlayback<KraidHeadSegment> started = playback ?? throw new InvalidDataException(
            "Kraid's head animation has not started, so its mouth and growth reads have no pose to look at.");
        KraidHeadSegmentPlace place = PlaceOf(started);
        IReadOnlyList<AnimationBeat<KraidHeadFrame, KraidHeadCue>> beats = Segments.Beats(place.Segment);
        if ((uint)place.Step > (uint)beats.Count)
            throw new InvalidOperationException($"Kraid head place {place} is outside its segment.");
        KraidHeadFrame?[] steps = [.. beats.Select(beat => beat.ShownFrame)];
        KraidMouthTrailsHead.MouthShapes<KraidHeadFrame> shapes =
            KraidMouthTrailsHead.ShapesAt(steps, place.Step, PoseLaidOutBefore[place.Segment]);
        return new(Read(shapes.Outer), Read(shapes.Inner), GrowthResumeBefore(place.Step < steps.Length ? steps[place.Step] : null));
    }

    /// <summary>What a mouth test finds: the shape it takes, the closed mouth's missing inner shape, or no pose.</summary>
    private static KraidMouthShapeRead Read(KraidMouthTrailsHead.MouthShape<KraidHeadFrame> shape) => shape.Pose switch
    {
        null => KraidMouthShapeRead.NoPose,
        { } pose when shape.Shape == KraidMouthTrailsHead.PoseShape.Inner => KraidMouthShapeRead.InvulnerableOf(pose),
        { } pose => KraidMouthShapeRead.VulnerableOf(pose),
    };

    /// <summary>
    /// Where growth resumes the roar when the head shows <paramref name="next"/> next: the closed, opening or wider pose
    /// resume the roar's closing part at that pose; the open pose, a cry or an animation's end resume it at the wide-open
    /// hold's end.
    /// </summary>
    private static KraidHeadGrowthResume GrowthResumeBefore(KraidHeadFrame? next) =>
        next == KraidHeadFrame.Closed ? KraidHeadGrowthResume.NextFrameClosed
        : next == KraidHeadFrame.Opening ? KraidHeadGrowthResume.NextFrameOpening
        : next == KraidHeadFrame.Wider ? KraidHeadGrowthResume.NextFrameWider
        : KraidHeadGrowthResume.NextStepOther;

    /// <summary>The outer mouth shape body hits are tested against where <paramref name="playback"/> stands.</summary>
    internal static KraidMouthHitbox OuterMouthAt(AnimationPlayback<KraidHeadSegment>? playback)
    {
        KraidMouthShapeRead read = ReadsAt(playback).Outer;
        return read.Kind == KraidMouthShapeReadKind.Shape
            ? read.Shape
            : throw new InvalidDataException($"Kraid's outer mouth read at {(playback is { } head ? PlaceOf(head).ToString() : "no place")} finds {read.Kind}, not a mouth shape.");
    }

    /// <summary>The inner mouth shape mouth hits are tested against where <paramref name="playback"/> stands, or null when the pose has none.</summary>
    internal static KraidMouthHitbox? InnerMouthAt(AnimationPlayback<KraidHeadSegment>? playback)
    {
        KraidMouthShapeRead read = ReadsAt(playback).Inner;
        return read.Kind switch
        {
            KraidMouthShapeReadKind.Shape => read.Shape,
            KraidMouthShapeReadKind.NoShape => null,
            KraidMouthShapeReadKind.NoPose => throw new InvalidDataException(
                $"Kraid's inner mouth read at {(playback is { } head ? PlaceOf(head).ToString() : "no place")} finds no head pose."),
            _ => throw new InvalidOperationException($"Undefined {nameof(KraidMouthShapeReadKind)} {read.Kind}."),
        };
    }

    /// <summary>The roar place growth restarts the head at.</summary>
    internal static AnimationPlayback<KraidHeadSegment> GrowthResumePlayback(KraidHeadGrowthResume resume) => resume switch
    {
        KraidHeadGrowthResume.NextFrameClosed => At(P.Roar, 8),
        KraidHeadGrowthResume.NextFrameOpening => At(P.Roar, 7),
        KraidHeadGrowthResume.NextFrameWider => At(P.Roar, 6),
        KraidHeadGrowthResume.NextStepOther => At(P.Roar, 5),
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidHeadGrowthResume)} {resume}."),
    };

    /// <summary>The head delay, in updates, growth restarts the head with.</summary>
    internal static int GrowthResumeDelay(KraidHeadGrowthResume resume) => resume switch
    {
        KraidHeadGrowthResume.NextFrameClosed or KraidHeadGrowthResume.NextFrameOpening or
            KraidHeadGrowthResume.NextFrameWider => RoarClosingDuration,
        KraidHeadGrowthResume.NextStepOther => RoarOpenDuration,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidHeadGrowthResume)} {resume}."),
    };

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<KraidHeadFrame, KraidHeadCue> Show(KraidHeadFrame frame, int duration) =>
        new AnimationBeat<KraidHeadFrame, KraidHeadCue>.Frame(frame, duration);

    /// <summary>Announces <paramref name="cue"/>.</summary>
    private static AnimationBeat<KraidHeadFrame, KraidHeadCue> Cue(KraidHeadCue cue) =>
        new AnimationBeat<KraidHeadFrame, KraidHeadCue>.Event(cue);
}
