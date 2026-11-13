using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Kraid.KraidPartCue;
using P = SuperMetroid.Core.Bosses.Kraid.KraidPartSegment;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's typed part animations (#641, #1600): each foot, arm, lint and fingernail segment's beats, and the animation starting at each.</summary>
internal static class KraidAnimations
{
    /// <summary>Every foot, arm, lint and fingernail segment's beats.</summary>
    internal static AnimationSegments<KraidPartSegment, KraidPartFrame, KraidPartCue> Segments { get; } = new(
        new Dictionary<KraidPartSegment, AnimationBeat<KraidPartFrame, KraidPartCue>[]>
    {
        [P.FootInitial] =
        [
            Hold(FootStanding()),
        ],
        [P.FootNeutral] =
        [
            Hold(Foot(0)),
        ],
        [P.FootWalkingForward] =
        [
            Show(Foot(0), 4),
            Show(Foot(1), 4),
            Show(Foot(2), 4),
            Show(Foot(3), 4),
            Show(Foot(4), 4),
            Show(Foot(5), 4),
            Show(Foot(6), 4),
            Show(Foot(7), 4),
            Show(Foot(8), 4),
            Show(Foot(9), 4),
            Show(Foot(10), 4),
            Show(Foot(11), 3),
            Show(Foot(12), 1),
            Show(Foot(13), 1),
            Show(Foot(14), 1),
            Show(Foot(15), 1),
            Show(Foot(16), 1),
            Show(Foot(17), 16),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(18), 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(19), 1),
            Show(Foot(19), 3),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(19), 1),
            Show(Foot(19), 3),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(21), 1),
            Show(Foot(21), 3),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(Foot(22), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(Foot(23), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(Foot(24), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Cue(K.StepSound),
            Show(Foot(25), 1),
            Cue(K.StepLeft),
            Show(Foot(26), 1),
            Cue(K.StepLeft),
            Show(Foot(27), 1),
            Cue(K.StepLeft),
            Show(Foot(28), 1),
            Cue(K.StepLeft),
            Show(Foot(29), 1),
            Cue(K.StepLeft),
            Show(Foot(30), 1),
            Cue(K.StepLeft),
            Show(Foot(31), 1),
            Show(Foot(0), 1),
        ],
        [P.FootWalkingForwardFinished] =
        [
        ],
        [P.FootLungeForward] =
        [
            Show(Foot(0), 1),
            Show(Foot(1), 1),
            Show(Foot(2), 1),
            Show(Foot(3), 1),
            Show(Foot(4), 1),
            Show(Foot(5), 1),
            Show(Foot(6), 1),
            Show(Foot(7), 1),
            Show(Foot(8), 1),
            Show(Foot(9), 1),
            Show(Foot(10), 1),
            Show(Foot(11), 1),
            Show(Foot(12), 1),
            Show(Foot(13), 1),
            Show(Foot(14), 1),
            Show(Foot(15), 1),
            Show(Foot(16), 1),
            Show(Foot(17), 4),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(18), 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(19), 1),
            Show(Foot(19), 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(19), 1),
            Show(Foot(19), 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(21), 1),
            Show(Foot(21), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(Foot(22), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(Foot(23), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(Foot(24), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Cue(K.StepSound),
            Show(Foot(25), 1),
            Cue(K.StepLeft),
            Show(Foot(26), 1),
            Cue(K.StepLeft),
            Show(Foot(27), 1),
            Cue(K.StepLeft),
            Show(Foot(28), 1),
            Cue(K.StepLeft),
            Show(Foot(29), 1),
            Cue(K.StepLeft),
            Show(Foot(30), 1),
            Show(Foot(31), 1),
            Cue(K.StepLeft),
            Show(Foot(0), 1),
        ],
        [P.FootLungeForwardFinished] =
        [
        ],
        [P.FootWalkingBackward] =
        [
            Cue(K.StepRight),
            Show(Foot(33), 4),
            Cue(K.StepRight),
            Show(Foot(32), 1),
            Cue(K.StepRight),
            Show(Foot(30), 1),
            Cue(K.StepRight),
            Show(Foot(29), 1),
            Cue(K.StepRight),
            Show(Foot(28), 1),
            Cue(K.StepRight),
            Show(Foot(27), 1),
            Cue(K.StepRight),
            Show(Foot(26), 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(Foot(25), 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(Foot(24), 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(Foot(23), 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(Foot(22), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Show(Foot(21), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Show(Foot(20), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Show(Foot(19), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepSound),
            Show(Foot(18), 1),
            Show(Foot(17), 20),
            Show(Foot(16), 4),
            Show(Foot(15), 4),
            Show(Foot(14), 4),
            Show(Foot(13), 4),
            Show(Foot(12), 4),
            Show(Foot(11), 4),
            Show(Foot(10), 4),
            Show(Foot(9), 4),
            Show(Foot(8), 1),
            Show(Foot(7), 1),
            Show(Foot(6), 1),
            Show(Foot(5), 1),
            Show(Foot(4), 1),
            Show(Foot(3), 1),
            Show(Foot(2), 1),
            Show(Foot(1), 1),
        ],
        [P.FootWalkingBackwardLoop] =
        [
        ],
        [P.ArmNormal] =
        [
            Show(Arm(0), 6),
            Show(Arm(1), 6),
            Show(Arm(2), 6),
            Show(Arm(3), 6),
            Show(Arm(4), 6),
            Show(Arm(5), 6),
            Show(Arm(6), 6),
            Show(Arm(7), 6),
            Show(Arm(8), 6),
            Show(Arm(9), 6),
            Show(Arm(7), 6),
            Show(Arm(6), 6),
            Show(Arm(5), 6),
            Show(Arm(4), 6),
            Show(Arm(3), 6),
            Show(Arm(2), 6),
            Show(Arm(1), 6),
        ],
        [P.ArmNormalPause] =
        [
            Show(Arm(0), 32),
            Cue(K.SlowArmBelowHalfHealth),
        ],
        [P.ArmSlow] =
        [
            Show(Arm(0), 8),
            Show(Arm(1), 8),
            Show(Arm(2), 8),
            Show(Arm(3), 8),
            Show(Arm(4), 8),
            Show(Arm(5), 8),
            Show(Arm(6), 8),
            Show(Arm(7), 8),
            Show(Arm(8), 8),
            Show(Arm(9), 8),
            Show(Arm(7), 8),
            Show(Arm(6), 8),
            Show(Arm(5), 8),
            Show(Arm(4), 8),
            Show(Arm(3), 8),
            Show(Arm(2), 8),
            Show(Arm(1), 8),
            Show(Arm(0), 48),
            Cue(K.SlowArmBelowHalfHealth),
        ],
        [P.ArmRisingOrSinking] =
        [
            Show(Arm(10), 6),
            Show(Arm(11), 6),
            Show(Arm(12), 6),
            Show(Arm(13), 6),
            Show(Arm(14), 6),
            Show(Arm(15), 6),
            Show(Arm(16), 6),
            Show(Arm(17), 6),
            Show(Arm(18), 6),
            Show(Arm(19), 6),
            Show(Arm(17), 6),
            Show(Arm(16), 6),
            Show(Arm(15), 6),
            Show(Arm(14), 6),
            Show(Arm(13), 6),
            Show(Arm(12), 6),
            Show(Arm(11), 6),
            Show(Arm(10), 32),
        ],
        [P.ArmDyingOrPreparingToLunge] =
        [
            Show(Arm(0), 6),
            Show(Arm(20), 6),
            Show(Arm(21), ReadyToLungeArmDuration),
        ],
        [P.LintInitial] =
        [
            Hold(Lint(0)),
        ],
        [P.LintPostGrowth] =
        [
            Hold(Lint(1)),
        ],
        [P.NailLoop] =
        [
            Show(Nail(0), 3),
            Show(Nail(1), 3),
            Show(Nail(2), 3),
            Show(Nail(3), 3),
            Show(Nail(4), 3),
            Show(Nail(5), 3),
            Show(Nail(6), 3),
            Show(Nail(7), 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KraidPartSegment, Animation<KraidPartSegment>> StartingAt { get; } = new Dictionary<KraidPartSegment, Animation<KraidPartSegment>>
    {
        [P.FootInitial] = new([new(P.FootInitial, 0, 1, 1)], AnimationEnd.Holds),
        [P.FootNeutral] = new([new(P.FootNeutral, 0, 1, 1)], AnimationEnd.Holds),
        [P.FootWalkingForward] = new([new(P.FootWalkingForward, 0, 59, 1)], AnimationEnd.Holds),
        [P.FootWalkingForwardFinished] = new([], AnimationEnd.Holds),
        [P.FootLungeForward] = new([new(P.FootLungeForward, 0, 59, 1)], AnimationEnd.Holds),
        [P.FootLungeForwardFinished] = new([], AnimationEnd.Holds),
        [P.FootWalkingBackward] = new([new(P.FootWalkingBackward, 0, 55, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FootWalkingBackwardLoop] = new([new(P.FootWalkingBackward, 0, 55, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmNormal] = new([new(P.ArmNormal, 0, 17, 1), new(P.ArmNormalPause, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmNormalPause] = new([new(P.ArmNormalPause, 0, 2, 1), new(P.ArmNormal, 0, 17, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmSlow] = new([new(P.ArmSlow, 0, 19, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmRisingOrSinking] = new([new(P.ArmRisingOrSinking, 0, 18, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmDyingOrPreparingToLunge] = new([new(P.ArmDyingOrPreparingToLunge, 0, 3, 1)], AnimationEnd.Holds),
        [P.LintInitial] = new([new(P.LintInitial, 0, 1, 1)], AnimationEnd.Holds),
        [P.LintPostGrowth] = new([new(P.LintPostGrowth, 0, 1, 1)], AnimationEnd.Holds),
        [P.NailLoop] = new([new(P.NailLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KraidPartSegment> StartingWith(KraidPartSegment segment) => StartingAt[segment];

    /// <summary>
    /// The segment each list that runs on into the list authored next, directly after its last beat, continues into: a
    /// part resting after that beat already stands in the next, which Kraid's arm and foot routines compare.
    /// </summary>
    internal static IReadOnlyDictionary<KraidPartSegment, KraidPartSegment> RunsOnInto { get; } = new Dictionary<KraidPartSegment, KraidPartSegment>
    {
        [P.FootWalkingForward] = P.FootWalkingForwardFinished,
        [P.FootLungeForward] = P.FootLungeForwardFinished,
        [P.FootWalkingBackward] = P.FootWalkingBackwardLoop,
        [P.ArmNormal] = P.ArmNormalPause,
    };

    /// <summary>The foot's standing pose.</summary>
    private static KraidPartFrame FootStanding() => new(KraidPartFrameKind.FootStanding, 0);

    /// <summary>A foot pose.</summary>
    private static KraidPartFrame Foot(int index) => new(KraidPartFrameKind.Foot, index);

    /// <summary>An arm pose.</summary>
    private static KraidPartFrame Arm(int index) => new(KraidPartFrameKind.Arm, index);

    /// <summary>A belly-lint pose.</summary>
    private static KraidPartFrame Lint(int index) => new(KraidPartFrameKind.Lint, index);

    /// <summary>A fingernail pose.</summary>
    private static KraidPartFrame Nail(int index) => new(KraidPartFrameKind.Nail, index);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<KraidPartFrame, KraidPartCue> Show(KraidPartFrame frame, int duration) =>
        new AnimationBeat<KraidPartFrame, KraidPartCue>.Frame(frame, duration);

    /// <summary>
    /// Shows <paramref name="frame"/> for good (#1627): the part rests on it until its owner starts another program. The
    /// feet's standing poses and the belly lints' poses are held: nothing reads a foot's frame timer, and a lint's routine
    /// reloads its timer every update, so the original's longest frame never runs out where gameplay sees it.
    /// </summary>
    private static AnimationBeat<KraidPartFrame, KraidPartCue> Hold(KraidPartFrame frame) =>
        new AnimationBeat<KraidPartFrame, KraidPartCue>.HeldFrame(frame);

    /// <summary>
    /// The updates the arm's last lunge-ready pose shows, the longest frame a list gives, kept exact (#1627): while mouth
    /// reopens are pending, Kraid's arm routine adds an update to the arm's frame timer every update, which keeps this
    /// count standing and the arm's new-frame flag clear; a held frame's timer would run out every update instead.
    /// </summary>
    private const int ReadyToLungeArmDuration = 32767;

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<KraidPartFrame, KraidPartCue> Cue(KraidPartCue cue) =>
        new AnimationBeat<KraidPartFrame, KraidPartCue>.Event(cue);
}
