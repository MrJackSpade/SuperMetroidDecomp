using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Kraid.KraidPartCue;
using P = SuperMetroid.Core.Bosses.Kraid.KraidPartProgram;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's typed part programs (#641), in place of their bank-$A7 instruction lists.</summary>
internal static class KraidAnimations
{
    /// <summary>Every foot, arm, lint and fingernail program.</summary>
    internal static AnimationProgramSet<KraidPartProgram, KraidPartFrame, KraidPartCue> Programs { get; } = new(
        new Dictionary<KraidPartProgram, AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>[]>
    {
        [P.FootInitial] =
        [
            Show(FootStanding(), 32767),
            Hold,
        ],
        [P.FootNeutral] =
        [
            Show(Foot(0), 32767),
            Hold,
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
            FallInto(P.FootWalkingForwardFinished),
        ],
        [P.FootWalkingForwardFinished] =
        [
            Hold,
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
            FallInto(P.FootLungeForwardFinished),
        ],
        [P.FootLungeForwardFinished] =
        [
            Hold,
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
            FallInto(P.FootWalkingBackwardLoop),
        ],
        [P.FootWalkingBackwardLoop] =
        [
            GoTo(P.FootWalkingBackward),
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
            FallInto(P.ArmNormalPause),
        ],
        [P.ArmNormalPause] =
        [
            Show(Arm(0), 32),
            Cue(K.SlowArmBelowHalfHealth),
            GoTo(P.ArmNormal),
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
            GoTo(P.ArmSlow),
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
            GoTo(P.ArmRisingOrSinking),
        ],
        [P.ArmDyingOrPreparingToLunge] =
        [
            Show(Arm(0), 6),
            Show(Arm(20), 6),
            Show(Arm(21), 32767),
            Hold,
        ],
        [P.LintInitial] =
        [
            Show(Lint(0), 32767),
            Hold,
        ],
        [P.LintPostGrowth] =
        [
            Show(Lint(1), 32767),
            Hold,
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
            GoTo(P.NailLoop),
        ],
    });

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
    private static AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue> Show(KraidPartFrame frame, int duration) =>
        AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.Show(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue> Cue(KraidPartCue cue) =>
        AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.Publish(cue);

    /// <summary>Continues at the start of <paramref name="program"/>.</summary>
    private static AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue> GoTo(KraidPartProgram program) =>
        AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue> FallInto(KraidPartProgram program) =>
        AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.FallInto(program);

    /// <summary>Holds the last frame: the native sleep.</summary>
    private static AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue> Hold =>
        AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.Hold;
}
