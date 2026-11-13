using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Bosses.Kraid.KraidHeadProgram;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's head animations (#641, #1600), and what collision and growth read at each step of each animation. The head shows
/// a pose for its duration, cries between poses and holds at each animation's end; the body's head timer gates every
/// advance. Checked against the game's head animations by the head-animation verification.
/// </summary>
internal static class KraidHeadAnimations
{
    /// <summary>How long the attack roar's entry pose lasts before its program first advances, in updates.</summary>
    internal const int RoarEntryDuration = 10;

    /// <summary>How long the eye glow's entry pose lasts before its program first advances, in updates.</summary>
    internal const int EyeGlowEntryDuration = 5;

    /// <summary>How long the death cry's entry pose lasts before its program first advances, in updates.</summary>
    internal const int DeathEntryDuration = 25;

    /// <summary>How long the attack roar's closing poses last, in updates; growth restarts the head with this delay.</summary>
    private const int RoarClosingDuration = 10;

    /// <summary>How long the attack roar holds its wide-open pose, in updates; growth restarts the head with this delay.</summary>
    private const int RoarOpenDuration = 64;

    /// <summary>
    /// The attack roar, past its entry pose. Each installer starts its animation here with the entry pose's duration and
    /// leaves the head image as it was.
    /// </summary>
    internal static AnimationPlayback<KraidHeadProgram> RoarAfterEntryFrame => At(P.Roar, 1);

    /// <summary>The eye glow, past its entry pose.</summary>
    internal static AnimationPlayback<KraidHeadProgram> EyeGlowAfterEntryFrame => At(P.EyeGlow, 1);

    /// <summary>The death cry, past its entry pose.</summary>
    internal static AnimationPlayback<KraidHeadProgram> DeathAfterEntryFrame => At(P.Death, 1);

    /// <summary>Every head segment's beats: its poses for their durations and the cries between them.</summary>
    internal static AnimationSegments<KraidHeadProgram, KraidHeadFrame, KraidHeadCue> Segments { get; } = new(
        new Dictionary<KraidHeadProgram, AnimationBeat<KraidHeadFrame, KraidHeadCue>[]>
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
    internal static IReadOnlyDictionary<KraidHeadProgram, Animation<KraidHeadProgram>> StartingAt { get; } = new Dictionary<KraidHeadProgram, Animation<KraidHeadProgram>>
    {
        [P.Roar] = new([new(P.Roar, 0, 8, 1)], AnimationEnd.Holds),
        [P.DyingRoar] = new([new(P.DyingRoar, 0, 8, 1)], AnimationEnd.Holds),
        [P.EyeGlow] = new([new(P.EyeGlow, 0, 3, 1)], AnimationEnd.Holds),
        [P.Death] = new([new(P.Death, 0, 5, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KraidHeadProgram> StartingWith(KraidHeadProgram segment) => StartingAt[segment];

    /// <summary>
    /// The head animation of <paramref name="program"/> standing before beat <paramref name="position"/> of its one phase;
    /// at the beat count, its play is done and the next play holds.
    /// </summary>
    internal static AnimationPlayback<KraidHeadProgram> At(KraidHeadProgram program, int position) =>
        new(StartingWith(program), new AnimationPlace(0, position));

    /// <summary>
    /// Where <paramref name="playback"/> stands in its head animation: the program its one phase plays, and the step its play
    /// reaches next, the animation's end standing for a play that is done or holds there.
    /// </summary>
    internal static KraidHeadListPosition PositionOf(AnimationPlayback<KraidHeadProgram> playback)
    {
        AnimationPhase<KraidHeadProgram> phase = playback.Animation.Phases[playback.Place.Phase];
        return new(phase.Segment, phase.FirstBeat + Math.Clamp(playback.Place.Beat, 0, phase.BeatCount));
    }

    /// <summary>
    /// What collision and growth read at each step of each animation, one entry per step, the last at the animation's end. The first entry of every
    /// program but the roar looks into the end of the program authored before it, whose last pose is the closed mouth.
    /// </summary>
    private static readonly Dictionary<KraidHeadProgram, KraidHeadCursorReads[]> Reads = new()
    {
        [P.Roar] =
        [
            new(KraidMouthShapeRead.OutsideHeadFrames, KraidMouthShapeRead.OutsideHeadFrames, KraidHeadGrowthResume.NextFrameClosed),
            After(KraidHeadFrame.Closed, KraidHeadGrowthResume.NextFrameOpening),
            After(KraidHeadFrame.Opening, KraidHeadGrowthResume.NextFrameWider),
            After(KraidHeadFrame.Wider, KraidHeadGrowthResume.NextStepOther),
            AfterCry(KraidHeadFrame.Wider, KraidHeadGrowthResume.NextStepOther),
            After(KraidHeadFrame.Open, KraidHeadGrowthResume.NextFrameWider),
            After(KraidHeadFrame.Wider, KraidHeadGrowthResume.NextFrameOpening),
            After(KraidHeadFrame.Opening, KraidHeadGrowthResume.NextFrameClosed),
            After(KraidHeadFrame.Closed, KraidHeadGrowthResume.NextStepOther),
        ],
        [P.DyingRoar] =
        [
            AfterPreviousProgram(KraidHeadFrame.Closed, KraidHeadGrowthResume.NextFrameClosed),
            After(KraidHeadFrame.Closed, KraidHeadGrowthResume.NextFrameOpening),
            After(KraidHeadFrame.Opening, KraidHeadGrowthResume.NextFrameWider),
            After(KraidHeadFrame.Wider, KraidHeadGrowthResume.NextStepOther),
            AfterCry(KraidHeadFrame.Wider, KraidHeadGrowthResume.NextStepOther),
            After(KraidHeadFrame.Open, KraidHeadGrowthResume.NextFrameWider),
            After(KraidHeadFrame.Wider, KraidHeadGrowthResume.NextFrameOpening),
            After(KraidHeadFrame.Opening, KraidHeadGrowthResume.NextFrameClosed),
            After(KraidHeadFrame.Closed, KraidHeadGrowthResume.NextStepOther),
        ],
        [P.EyeGlow] =
        [
            AfterPreviousProgram(KraidHeadFrame.Closed, KraidHeadGrowthResume.NextFrameClosed),
            After(KraidHeadFrame.Closed, KraidHeadGrowthResume.NextFrameOpening),
            After(KraidHeadFrame.Opening, KraidHeadGrowthResume.NextFrameClosed),
            After(KraidHeadFrame.Closed, KraidHeadGrowthResume.NextStepOther),
        ],
        [P.Death] =
        [
            AfterPreviousProgram(KraidHeadFrame.Closed, KraidHeadGrowthResume.NextFrameClosed),
            After(KraidHeadFrame.Closed, KraidHeadGrowthResume.NextFrameOpening),
            After(KraidHeadFrame.Opening, KraidHeadGrowthResume.NextFrameWider),
            After(KraidHeadFrame.Wider, KraidHeadGrowthResume.NextStepOther),
            AfterCry(KraidHeadFrame.Wider, KraidHeadGrowthResume.NextStepOther),
            After(KraidHeadFrame.Open, KraidHeadGrowthResume.NextStepOther),
        ],
    };

    /// <summary>What collision and growth read where <paramref name="playback"/> stands; a head that never started has no pose to read.</summary>
    internal static KraidHeadCursorReads ReadsAt(AnimationPlayback<KraidHeadProgram>? playback)
    {
        AnimationPlayback<KraidHeadProgram> started = playback ?? throw new InvalidDataException(
            "Kraid's head animation has not started, so its mouth and growth reads have no pose to look at.");
        KraidHeadListPosition position = PositionOf(started);
        KraidHeadCursorReads[] reads = Reads[position.Program];
        return (uint)position.Step < (uint)reads.Length
            ? reads[position.Step]
            : throw new InvalidOperationException($"Kraid head place {position} is outside its list.");
    }

    /// <summary>The outer mouth shape body hits are tested against where <paramref name="playback"/> stands.</summary>
    internal static KraidMouthHitbox OuterMouthAt(AnimationPlayback<KraidHeadProgram>? playback)
    {
        KraidMouthShapeRead read = ReadsAt(playback).Outer;
        return read.Kind == KraidMouthShapeReadKind.Shape
            ? read.Shape
            : throw new InvalidDataException($"Kraid's outer mouth read at {(playback is { } head ? PositionOf(head).ToString() : "no place")} finds {read.Kind}, not a mouth shape.");
    }

    /// <summary>The inner mouth shape mouth hits are tested against where <paramref name="playback"/> stands, or null when the pose has none.</summary>
    internal static KraidMouthHitbox? InnerMouthAt(AnimationPlayback<KraidHeadProgram>? playback)
    {
        KraidMouthShapeRead read = ReadsAt(playback).Inner;
        return read.Kind switch
        {
            KraidMouthShapeReadKind.Shape => read.Shape,
            KraidMouthShapeReadKind.NoShape => null,
            KraidMouthShapeReadKind.OutsideHeadFrames => throw new InvalidDataException(
                $"Kraid's inner mouth read at {(playback is { } head ? PositionOf(head).ToString() : "no place")} lands outside every head pose."),
            _ => throw new InvalidOperationException($"Undefined {nameof(KraidMouthShapeReadKind)} {read.Kind}."),
        };
    }

    /// <summary>The roar place growth restarts the head at.</summary>
    internal static AnimationPlayback<KraidHeadProgram> GrowthResumePlayback(KraidHeadGrowthResume resume) => resume switch
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

    /// <summary>The reads just past <paramref name="frame"/>: its outer and inner shapes.</summary>
    private static KraidHeadCursorReads After(KraidHeadFrame frame, KraidHeadGrowthResume growth) =>
        new(KraidMouthShapeRead.VulnerableOf(frame), KraidMouthShapeRead.InvulnerableOf(frame), growth);

    /// <summary>The reads just past a cry that follows <paramref name="frame"/>: the outer read finds the pose's inner shape.</summary>
    private static KraidHeadCursorReads AfterCry(KraidHeadFrame frame, KraidHeadGrowthResume growth) =>
        new(KraidMouthShapeRead.InvulnerableOf(frame), KraidMouthShapeRead.OutsideHeadFrames, growth);

    /// <summary>
    /// The reads at a program's start, past the end of the program authored before it, whose last pose is
    /// <paramref name="frame"/>: the outer read finds that pose's inner shape and the inner read finds no pose.
    /// </summary>
    private static KraidHeadCursorReads AfterPreviousProgram(KraidHeadFrame frame, KraidHeadGrowthResume growth) =>
        new(KraidMouthShapeRead.InvulnerableOf(frame), KraidMouthShapeRead.OutsideHeadFrames, growth);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<KraidHeadFrame, KraidHeadCue> Show(KraidHeadFrame frame, int duration) =>
        new AnimationBeat<KraidHeadFrame, KraidHeadCue>.Frame(frame, duration);

    /// <summary>Announces <paramref name="cue"/>.</summary>
    private static AnimationBeat<KraidHeadFrame, KraidHeadCue> Cue(KraidHeadCue cue) =>
        new AnimationBeat<KraidHeadFrame, KraidHeadCue>.Event(cue);
}
