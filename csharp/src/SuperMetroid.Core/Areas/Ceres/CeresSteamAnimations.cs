using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Areas.Ceres.CeresSteamCueKind;
using P = SuperMetroid.Core.Areas.Ceres.CeresSteamSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres steam vents' typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="CeresSteamCue"/> with its typed operands.
/// </summary>
internal static class CeresSteamAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CeresSteamSegment, EnemyFrameId, CeresSteamCue> Segments { get; } = new(
        new Dictionary<CeresSteamSegment, AnimationBeat<EnemyFrameId, CeresSteamCue>[]>
    {
        [P.Up] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamUp, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.Up, P.UpLoop2)),
        ],
        [P.UpLoop] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamUp, 64),
            Cue(new CeresSteamCue.Command(K.Appear)),
        ],
        [P.UpLoop2] =
        [
            Show(EnemyFrameId.CeresSteamUp, 3),
            Show(EnemyFrameId.CeresSteamUpLoopTwoSecond, 3),
            Show(EnemyFrameId.CeresSteamUpLoopTwoThird, 3),
            Show(EnemyFrameId.CeresSteamUpLoopTwoFourth, 3),
            Show(EnemyFrameId.CeresSteamUpLoopTwoFifth, 3),
            Show(EnemyFrameId.CeresSteamUpLoopTwoSixth, 3),
            Show(EnemyFrameId.CeresSteamUpLoopTwoSeventh, 3),
        ],
        [P.Left] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamLeft, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.Left, P.LeftLoop2)),
        ],
        [P.LeftLoop] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamLeft, 64),
            Cue(new CeresSteamCue.Command(K.Appear)),
        ],
        [P.LeftLoop2] =
        [
            Show(EnemyFrameId.CeresSteamLeft, 3),
            Show(EnemyFrameId.CeresSteamLeftLoopTwoSecond, 3),
            Show(EnemyFrameId.CeresSteamLeftLoopTwoThird, 3),
            Show(EnemyFrameId.CeresSteamLeftLoopTwoFourth, 3),
            Show(EnemyFrameId.CeresSteamLeftLoopTwoFifth, 3),
            Show(EnemyFrameId.CeresSteamLeftLoopTwoSixth, 3),
            Show(EnemyFrameId.CeresSteamLeftLoopTwoSeventh, 3),
        ],
        [P.Down] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamDown, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.Down, P.DownLoop2)),
        ],
        [P.DownLoop] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamDown, 64),
            Cue(new CeresSteamCue.Command(K.Appear)),
        ],
        [P.DownLoop2] =
        [
            Show(EnemyFrameId.CeresSteamDown, 3),
            Show(EnemyFrameId.CeresSteamDownLoopTwoSecond, 3),
            Show(EnemyFrameId.CeresSteamDownLoopTwoThird, 3),
            Show(EnemyFrameId.CeresSteamDownLoopTwoFourth, 3),
            Show(EnemyFrameId.CeresSteamDownLoopTwoFifth, 3),
            Show(EnemyFrameId.CeresSteamDownLoopTwoSixth, 3),
            Show(EnemyFrameId.CeresSteamDownLoopTwoSeventh, 3),
        ],
        [P.Right] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamRight, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.Right, P.RightLoop2)),
        ],
        [P.RightLoop] =
        [
            Cue(new CeresSteamCue.Command(K.Vanish)),
            Show(EnemyFrameId.CeresSteamRight, 64),
            Cue(new CeresSteamCue.Command(K.Appear)),
        ],
        [P.RightLoop2] =
        [
            Show(EnemyFrameId.CeresSteamRight, 3),
            Show(EnemyFrameId.CeresSteamRightLoopTwoSecond, 3),
            Show(EnemyFrameId.CeresSteamRightLoopTwoThird, 3),
            Show(EnemyFrameId.CeresSteamRightLoopTwoFourth, 3),
            Show(EnemyFrameId.CeresSteamRightLoopTwoFifth, 3),
            Show(EnemyFrameId.CeresSteamRightLoopTwoSixth, 3),
            Show(EnemyFrameId.CeresSteamRightLoopTwoSeventh, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CeresSteamSegment, Animation<CeresSteamSegment>> StartingAt { get; } = new Dictionary<CeresSteamSegment, Animation<CeresSteamSegment>>
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
    internal static Animation<CeresSteamSegment> StartingWith(CeresSteamSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CeresSteamCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCue> Cue(CeresSteamCue cue) => new AnimationBeat<EnemyFrameId, CeresSteamCue>.Event(cue);
}
