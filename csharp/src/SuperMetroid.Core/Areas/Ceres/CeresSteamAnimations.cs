using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Areas.Ceres.CeresSteamCueKind;
using P = SuperMetroid.Core.Areas.Ceres.CeresSteamProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres steam vents' typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="CeresSteamCue"/> with its typed operands.
/// </summary>
internal static class CeresSteamAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CeresSteamProgram, EnemyFrameId, CeresSteamCue> Segments { get; } = new(
        new Dictionary<CeresSteamProgram, AnimationBeat<EnemyFrameId, CeresSteamCue>[]>
    {
        [P.Up] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamPose0, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.Up, P.UpLoop2)),
        ],
        [P.UpLoop] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamPose0, 64),
            Cue(new CeresSteamCue.Command(K.Appear)),
        ],
        [P.UpLoop2] =
        [
            Show(EnemyFrameId.CeresSteamPose0, 3),
            Show(EnemyFrameId.CeresSteamPose1, 3),
            Show(EnemyFrameId.CeresSteamPose2, 3),
            Show(EnemyFrameId.CeresSteamPose3, 3),
            Show(EnemyFrameId.CeresSteamPose4, 3),
            Show(EnemyFrameId.CeresSteamPose5, 3),
            Show(EnemyFrameId.CeresSteamPose6, 3),
        ],
        [P.Left] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamPose7, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.Left, P.LeftLoop2)),
        ],
        [P.LeftLoop] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamPose7, 64),
            Cue(new CeresSteamCue.Command(K.Appear)),
        ],
        [P.LeftLoop2] =
        [
            Show(EnemyFrameId.CeresSteamPose7, 3),
            Show(EnemyFrameId.CeresSteamPose8, 3),
            Show(EnemyFrameId.CeresSteamPose9, 3),
            Show(EnemyFrameId.CeresSteamPose10, 3),
            Show(EnemyFrameId.CeresSteamPose11, 3),
            Show(EnemyFrameId.CeresSteamPose12, 3),
            Show(EnemyFrameId.CeresSteamPose13, 3),
        ],
        [P.Down] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamPose14, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.Down, P.DownLoop2)),
        ],
        [P.DownLoop] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamPose14, 64),
            Cue(new CeresSteamCue.Command(K.Appear)),
        ],
        [P.DownLoop2] =
        [
            Show(EnemyFrameId.CeresSteamPose14, 3),
            Show(EnemyFrameId.CeresSteamPose15, 3),
            Show(EnemyFrameId.CeresSteamPose16, 3),
            Show(EnemyFrameId.CeresSteamPose17, 3),
            Show(EnemyFrameId.CeresSteamPose18, 3),
            Show(EnemyFrameId.CeresSteamPose19, 3),
            Show(EnemyFrameId.CeresSteamPose20, 3),
        ],
        [P.Right] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamPose21, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.Right, P.RightLoop2)),
        ],
        [P.RightLoop] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamPose21, 64),
            Cue(new CeresSteamCue.Command(K.Appear)),
        ],
        [P.RightLoop2] =
        [
            Show(EnemyFrameId.CeresSteamPose21, 3),
            Show(EnemyFrameId.CeresSteamPose22, 3),
            Show(EnemyFrameId.CeresSteamPose23, 3),
            Show(EnemyFrameId.CeresSteamPose24, 3),
            Show(EnemyFrameId.CeresSteamPose25, 3),
            Show(EnemyFrameId.CeresSteamPose26, 3),
            Show(EnemyFrameId.CeresSteamPose27, 3),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CeresSteamCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCue> Cue(CeresSteamCue cue) => new AnimationBeat<EnemyFrameId, CeresSteamCue>.Event(cue);
}
