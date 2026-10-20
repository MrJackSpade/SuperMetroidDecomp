using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Areas.Ceres.CeresSteamCueKind;
using P = SuperMetroid.Core.Areas.Ceres.CeresSteamProgram;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres steam vents' typed programs (#641), in place of their bank-$A6 lists. Generated from the verified native lists: each
/// frame shows a <see cref="CeresSteamFrame"/> pose and each callback becomes a <see cref="CeresSteamCue"/> with its typed operands.
/// </summary>
internal static class CeresSteamAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<CeresSteamProgram, CeresSteamFrame, CeresSteamCue> Programs { get; } = new(
        new Dictionary<CeresSteamProgram, AnimationProgramStep<CeresSteamProgram, CeresSteamFrame, CeresSteamCue>[]>
    {
        [P.Up] =
        [
            Cue(new(K.Vanish)),
            Show(0, 1),
            Cue(new(K.CountDownToActivation, Target: P.Up, Alternate: P.UpLoop2)),
            FallInto(P.UpLoop),
        ],
        [P.UpLoop] =
        [
            Cue(new(K.Vanish)),
            Show(0, 64),
            Cue(new(K.Appear)),
            FallInto(P.UpLoop2),
        ],
        [P.UpLoop2] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 3),
            GoTo(P.UpLoop),
        ],
        [P.Left] =
        [
            Cue(new(K.Vanish)),
            Show(7, 1),
            Cue(new(K.CountDownToActivation, Target: P.Left, Alternate: P.LeftLoop2)),
            FallInto(P.LeftLoop),
        ],
        [P.LeftLoop] =
        [
            Cue(new(K.Vanish)),
            Show(7, 64),
            Cue(new(K.Appear)),
            FallInto(P.LeftLoop2),
        ],
        [P.LeftLoop2] =
        [
            Show(7, 3),
            Show(8, 3),
            Show(9, 3),
            Show(10, 3),
            Show(11, 3),
            Show(12, 3),
            Show(13, 3),
            GoTo(P.LeftLoop),
        ],
        [P.Down] =
        [
            Cue(new(K.Vanish)),
            Show(14, 1),
            Cue(new(K.CountDownToActivation, Target: P.Down, Alternate: P.DownLoop2)),
            FallInto(P.DownLoop),
        ],
        [P.DownLoop] =
        [
            Cue(new(K.Vanish)),
            Show(14, 64),
            Cue(new(K.Appear)),
            FallInto(P.DownLoop2),
        ],
        [P.DownLoop2] =
        [
            Show(14, 3),
            Show(15, 3),
            Show(16, 3),
            Show(17, 3),
            Show(18, 3),
            Show(19, 3),
            Show(20, 3),
            GoTo(P.DownLoop),
        ],
        [P.Right] =
        [
            Cue(new(K.Vanish)),
            Show(21, 1),
            Cue(new(K.CountDownToActivation, Target: P.Right, Alternate: P.RightLoop2)),
            FallInto(P.RightLoop),
        ],
        [P.RightLoop] =
        [
            Cue(new(K.Vanish)),
            Show(21, 64),
            Cue(new(K.Appear)),
            FallInto(P.RightLoop2),
        ],
        [P.RightLoop2] =
        [
            Show(21, 3),
            Show(22, 3),
            Show(23, 3),
            Show(24, 3),
            Show(25, 3),
            Show(26, 3),
            Show(27, 3),
            GoTo(P.RightLoop),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<CeresSteamProgram, CeresSteamFrame, CeresSteamCue> Show(int pose, int duration) => AnimationProgramStep<CeresSteamProgram, CeresSteamFrame, CeresSteamCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<CeresSteamProgram, CeresSteamFrame, CeresSteamCue> Cue(CeresSteamCue cue) => AnimationProgramStep<CeresSteamProgram, CeresSteamFrame, CeresSteamCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<CeresSteamProgram, CeresSteamFrame, CeresSteamCue> GoTo(CeresSteamProgram program) => AnimationProgramStep<CeresSteamProgram, CeresSteamFrame, CeresSteamCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<CeresSteamProgram, CeresSteamFrame, CeresSteamCue> FallInto(CeresSteamProgram program) => AnimationProgramStep<CeresSteamProgram, CeresSteamFrame, CeresSteamCue>.FallInto(program);

}
