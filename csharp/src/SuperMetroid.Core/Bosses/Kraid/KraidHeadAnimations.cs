using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Bosses.Kraid.KraidHeadProgram;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's head programs (#641) as typed steps, and what collision and growth read at each step. The head shows a
/// pose for its duration, cries between poses and holds at each program's end; the body's head timer gates every
/// advance. Checked against the cartridge's head lists by <c>KraidHeadAnimationsVerification</c>.
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

    /// <summary>The attack roar, past its entry pose. Each installer starts its program here with the entry pose's duration and leaves the head image as it was.</summary>
    internal static AnimationProgramCursor<KraidHeadProgram> RoarAfterEntryFrame { get; } = new(P.Roar, 1);

    /// <summary>The eye glow, past its entry pose.</summary>
    internal static AnimationProgramCursor<KraidHeadProgram> EyeGlowAfterEntryFrame { get; } = new(P.EyeGlow, 1);

    /// <summary>The death cry, past its entry pose.</summary>
    internal static AnimationProgramCursor<KraidHeadProgram> DeathAfterEntryFrame { get; } = new(P.Death, 1);

    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<KraidHeadProgram, KraidHeadFrame, KraidHeadCue> Programs { get; } = new(
        new Dictionary<KraidHeadProgram, AnimationProgramStep<KraidHeadProgram, KraidHeadFrame, KraidHeadCue>[]>
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
            Hold,
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
            Hold,
        ],
        [P.EyeGlow] =
        [
            Show(KraidHeadFrame.Closed, EyeGlowEntryDuration),
            Show(KraidHeadFrame.Opening, 10),
            Show(KraidHeadFrame.Closed, EyeGlowEntryDuration),
            Hold,
        ],
        [P.Death] =
        [
            Show(KraidHeadFrame.Closed, DeathEntryDuration),
            Show(KraidHeadFrame.Opening, DeathEntryDuration),
            Show(KraidHeadFrame.Wider, DeathEntryDuration),
            Cue(KraidHeadCue.DyingCry),
            Show(KraidHeadFrame.Open, 64),
            Hold,
        ],
    });

    /// <summary>
    /// What collision and growth read at each cursor of each program, one entry per step. The first entry of every
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

    /// <summary>What collision and growth read at <paramref name="cursor"/>; a head that never started has no pose to read.</summary>
    internal static KraidHeadCursorReads ReadsAt(AnimationProgramCursor<KraidHeadProgram>? cursor)
    {
        AnimationProgramCursor<KraidHeadProgram> started = cursor ?? throw new InvalidDataException(
            "Kraid's head program has not started, so its mouth and growth reads have no pose to look at.");
        KraidHeadCursorReads[] reads = Reads[started.Program];
        return (uint)started.Position < (uint)reads.Length
            ? reads[started.Position]
            : throw new InvalidOperationException($"Kraid head cursor {started} is outside its program.");
    }

    /// <summary>The outer mouth shape body hits are tested against at <paramref name="cursor"/>.</summary>
    internal static KraidMouthHitbox OuterMouthAt(AnimationProgramCursor<KraidHeadProgram>? cursor)
    {
        KraidMouthShapeRead read = ReadsAt(cursor).Outer;
        return read.Kind == KraidMouthShapeReadKind.Shape
            ? read.Shape
            : throw new InvalidDataException($"Kraid's outer mouth read at {cursor} finds {read.Kind}, not a mouth shape.");
    }

    /// <summary>The inner mouth shape mouth hits are tested against at <paramref name="cursor"/>, or null when the pose has none.</summary>
    internal static KraidMouthHitbox? InnerMouthAt(AnimationProgramCursor<KraidHeadProgram>? cursor)
    {
        KraidMouthShapeRead read = ReadsAt(cursor).Inner;
        return read.Kind switch
        {
            KraidMouthShapeReadKind.Shape => read.Shape,
            KraidMouthShapeReadKind.NoShape => null,
            KraidMouthShapeReadKind.OutsideHeadFrames => throw new InvalidDataException(
                $"Kraid's inner mouth read at {cursor} lands outside every head pose."),
            _ => throw new InvalidOperationException($"Undefined {nameof(KraidMouthShapeReadKind)} {read.Kind}."),
        };
    }

    /// <summary>The roar cursor growth restarts the head at.</summary>
    internal static AnimationProgramCursor<KraidHeadProgram> GrowthResumeCursor(KraidHeadGrowthResume resume) => resume switch
    {
        KraidHeadGrowthResume.NextFrameClosed => new(P.Roar, 8),
        KraidHeadGrowthResume.NextFrameOpening => new(P.Roar, 7),
        KraidHeadGrowthResume.NextFrameWider => new(P.Roar, 6),
        KraidHeadGrowthResume.NextStepOther => new(P.Roar, 5),
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
    private static AnimationProgramStep<KraidHeadProgram, KraidHeadFrame, KraidHeadCue> Show(KraidHeadFrame frame, int duration) =>
        AnimationProgramStep<KraidHeadProgram, KraidHeadFrame, KraidHeadCue>.Show(frame, duration);

    /// <summary>Announces <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<KraidHeadProgram, KraidHeadFrame, KraidHeadCue> Cue(KraidHeadCue cue) =>
        AnimationProgramStep<KraidHeadProgram, KraidHeadFrame, KraidHeadCue>.Publish(cue);

    /// <summary>Holds the last pose, as a program's end does.</summary>
    private static AnimationProgramStep<KraidHeadProgram, KraidHeadFrame, KraidHeadCue> Hold =>
        AnimationProgramStep<KraidHeadProgram, KraidHeadFrame, KraidHeadCue>.Hold;
}
