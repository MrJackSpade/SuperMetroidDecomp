using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Areas.Ceres.CeresSteamCueKind;
using P = SuperMetroid.Core.Areas.Ceres.CeresSteamSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres steam vents' typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="CeresSteamCue"/> with the values it acts on.
/// </summary>
internal static class CeresSteamAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CeresSteamSegment, EnemyFrameId, CeresSteamCue> Segments { get; } = new(
        new Dictionary<CeresSteamSegment, AnimationBeat<EnemyFrameId, CeresSteamCue>[]>
    {
        [P.UpDormant] =
        [
            Cue(new CeresSteamCue.Signal(K.Vanish)),
            Show(EnemyFrameId.CeresSteamUp, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.UpDormant, P.UpPuffing)),
        ],
        [P.UpResting] =
        [
            Cue(new CeresSteamCue.Signal(K.Vanish)),
            Show(EnemyFrameId.CeresSteamUp, 64),
            Cue(new CeresSteamCue.Signal(K.Appear)),
        ],
        [P.UpPuffing] =
        [
            Show(EnemyFrameId.CeresSteamUp, 3),
            Show(EnemyFrameId.CeresSteamUpSecond, 3),
            Show(EnemyFrameId.CeresSteamUpThird, 3),
            Show(EnemyFrameId.CeresSteamUpFourth, 3),
            Show(EnemyFrameId.CeresSteamUpFifth, 3),
            Show(EnemyFrameId.CeresSteamUpSixth, 3),
            Show(EnemyFrameId.CeresSteamUpSeventh, 3),
        ],
        [P.LeftDormant] =
        [
            Cue(new CeresSteamCue.Signal(K.Vanish)),
            Show(EnemyFrameId.CeresSteamLeft, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.LeftDormant, P.LeftPuffing)),
        ],
        [P.LeftResting] =
        [
            Cue(new CeresSteamCue.Signal(K.Vanish)),
            Show(EnemyFrameId.CeresSteamLeft, 64),
            Cue(new CeresSteamCue.Signal(K.Appear)),
        ],
        [P.LeftPuffing] =
        [
            Show(EnemyFrameId.CeresSteamLeft, 3),
            Show(EnemyFrameId.CeresSteamLeftSecond, 3),
            Show(EnemyFrameId.CeresSteamLeftThird, 3),
            Show(EnemyFrameId.CeresSteamLeftFourth, 3),
            Show(EnemyFrameId.CeresSteamLeftFifth, 3),
            Show(EnemyFrameId.CeresSteamLeftSixth, 3),
            Show(EnemyFrameId.CeresSteamLeftSeventh, 3),
        ],
        [P.DownDormant] =
        [
            Cue(new CeresSteamCue.Signal(K.Vanish)),
            Show(EnemyFrameId.CeresSteamDown, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.DownDormant, P.DownPuffing)),
        ],
        [P.DownResting] =
        [
            Cue(new CeresSteamCue.Signal(K.Vanish)),
            Show(EnemyFrameId.CeresSteamDown, 64),
            Cue(new CeresSteamCue.Signal(K.Appear)),
        ],
        [P.DownPuffing] =
        [
            Show(EnemyFrameId.CeresSteamDown, 3),
            Show(EnemyFrameId.CeresSteamDownSecond, 3),
            Show(EnemyFrameId.CeresSteamDownThird, 3),
            Show(EnemyFrameId.CeresSteamDownFourth, 3),
            Show(EnemyFrameId.CeresSteamDownFifth, 3),
            Show(EnemyFrameId.CeresSteamDownSixth, 3),
            Show(EnemyFrameId.CeresSteamDownSeventh, 3),
        ],
        [P.RightDormant] =
        [
            Cue(new CeresSteamCue.Signal(K.Vanish)),
            Show(EnemyFrameId.CeresSteamRight, 1),
            Cue(new CeresSteamCue.CountDownToActivation(P.RightDormant, P.RightPuffing)),
        ],
        [P.RightResting] =
        [
            Cue(new CeresSteamCue.Signal(K.Vanish)),
            Show(EnemyFrameId.CeresSteamRight, 64),
            Cue(new CeresSteamCue.Signal(K.Appear)),
        ],
        [P.RightPuffing] =
        [
            Show(EnemyFrameId.CeresSteamRight, 3),
            Show(EnemyFrameId.CeresSteamRightSecond, 3),
            Show(EnemyFrameId.CeresSteamRightThird, 3),
            Show(EnemyFrameId.CeresSteamRightFourth, 3),
            Show(EnemyFrameId.CeresSteamRightFifth, 3),
            Show(EnemyFrameId.CeresSteamRightSixth, 3),
            Show(EnemyFrameId.CeresSteamRightSeventh, 3),
        ],
    });

    /// <summary>
    /// The animation each segment starts: a dormant vent waits, then rests and puffs in turn; a resting or puffing vent
    /// goes on resting and puffing in turn for good.
    /// </summary>
    internal static IReadOnlyDictionary<CeresSteamSegment, Animation<CeresSteamSegment>> StartingAt { get; } = new Dictionary<CeresSteamSegment, Animation<CeresSteamSegment>>
    {
        [P.UpDormant] = new([new(P.UpDormant, 0, 3, 1), new(P.UpResting, 0, 3, 1), new(P.UpPuffing, 0, 7, 1)], AnimationEnd.LoopsFrom(1)),
        [P.UpResting] = new([new(P.UpResting, 0, 3, 1), new(P.UpPuffing, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpPuffing] = new([new(P.UpPuffing, 0, 7, 1), new(P.UpResting, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LeftDormant] = new([new(P.LeftDormant, 0, 3, 1), new(P.LeftResting, 0, 3, 1), new(P.LeftPuffing, 0, 7, 1)], AnimationEnd.LoopsFrom(1)),
        [P.LeftResting] = new([new(P.LeftResting, 0, 3, 1), new(P.LeftPuffing, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LeftPuffing] = new([new(P.LeftPuffing, 0, 7, 1), new(P.LeftResting, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownDormant] = new([new(P.DownDormant, 0, 3, 1), new(P.DownResting, 0, 3, 1), new(P.DownPuffing, 0, 7, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DownResting] = new([new(P.DownResting, 0, 3, 1), new(P.DownPuffing, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownPuffing] = new([new(P.DownPuffing, 0, 7, 1), new(P.DownResting, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RightDormant] = new([new(P.RightDormant, 0, 3, 1), new(P.RightResting, 0, 3, 1), new(P.RightPuffing, 0, 7, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RightResting] = new([new(P.RightResting, 0, 3, 1), new(P.RightPuffing, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RightPuffing] = new([new(P.RightPuffing, 0, 7, 1), new(P.RightResting, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresSteamSegment> StartingWith(CeresSteamSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CeresSteamCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCue> Cue(CeresSteamCue cue) => new AnimationBeat<EnemyFrameId, CeresSteamCue>.Event(cue);
}
