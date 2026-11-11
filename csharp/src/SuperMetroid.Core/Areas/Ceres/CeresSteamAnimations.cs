using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Areas.Ceres.CeresSteamCueKind;
using P = SuperMetroid.Core.Areas.Ceres.CeresSteamProgram;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres steam vents' typed programs (#641). Each
/// frame shows a <see cref="CeresSteamFrame"/> pose and each callback becomes a <see cref="CeresSteamCue"/> with its typed operands.
/// </summary>
internal static class CeresSteamAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CeresSteamProgram, CeresSteamFrame, CeresSteamCue> Segments { get; } = new(
        new Dictionary<CeresSteamProgram, AnimationBeat<CeresSteamFrame, CeresSteamCue>[]>
    {
        [P.Up] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(0, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.Up, P.UpLoop2)),
        ],
        [P.UpLoop] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(0, 64),
            Cue(new CeresSteamCue.Command(K.Appear)),
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
        ],
        [P.Left] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(7, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.Left, P.LeftLoop2)),
        ],
        [P.LeftLoop] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(7, 64),
            Cue(new CeresSteamCue.Command(K.Appear)),
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
        ],
        [P.Down] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(14, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.Down, P.DownLoop2)),
        ],
        [P.DownLoop] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(14, 64),
            Cue(new CeresSteamCue.Command(K.Appear)),
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
        ],
        [P.Right] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(21, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.Right, P.RightLoop2)),
        ],
        [P.RightLoop] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(21, 64),
            Cue(new CeresSteamCue.Command(K.Appear)),
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
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CeresSteamProgram, Animation<CeresSteamProgram>> StartingAt { get; } = new Dictionary<CeresSteamProgram, Animation<CeresSteamProgram>>
    {
        [P.Up] = new([new(P.Up, 0, 3, 1), new(P.UpLoop, 0, 3, 1), new(P.UpLoop2, 0, 7, 1)], AnimationEnd.LoopsFrom(1)),
        [P.UpLoop] = new([new(P.UpLoop, 0, 3, 1), new(P.UpLoop2, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpLoop2] = new([new(P.UpLoop2, 0, 7, 1), new(P.UpLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Left] = new([new(P.Left, 0, 3, 1), new(P.LeftLoop, 0, 3, 1), new(P.LeftLoop2, 0, 7, 1)], AnimationEnd.LoopsFrom(1)),
        [P.LeftLoop] = new([new(P.LeftLoop, 0, 3, 1), new(P.LeftLoop2, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LeftLoop2] = new([new(P.LeftLoop2, 0, 7, 1), new(P.LeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Down] = new([new(P.Down, 0, 3, 1), new(P.DownLoop, 0, 3, 1), new(P.DownLoop2, 0, 7, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DownLoop] = new([new(P.DownLoop, 0, 3, 1), new(P.DownLoop2, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownLoop2] = new([new(P.DownLoop2, 0, 7, 1), new(P.DownLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Right] = new([new(P.Right, 0, 3, 1), new(P.RightLoop, 0, 3, 1), new(P.RightLoop2, 0, 7, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RightLoop] = new([new(P.RightLoop, 0, 3, 1), new(P.RightLoop2, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RightLoop2] = new([new(P.RightLoop2, 0, 7, 1), new(P.RightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresSteamProgram> StartingWith(CeresSteamProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CeresSteamFrame, CeresSteamCue> Show(int pose, int duration) => new AnimationBeat<CeresSteamFrame, CeresSteamCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<CeresSteamFrame, CeresSteamCue> Cue(CeresSteamCue cue) => new AnimationBeat<CeresSteamFrame, CeresSteamCue>.Event(cue);
}
