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
            Show(EnemyFrameId.CeresSteamUpJet22, 3),
            Show(EnemyFrameId.CeresSteamUpJet31, 3),
            Show(EnemyFrameId.CeresSteamUpJet40, 3),
            Show(EnemyFrameId.CeresSteamUpDrifting33, 3),
            Show(EnemyFrameId.CeresSteamUpDrifting27, 3),
            Show(EnemyFrameId.CeresSteamUpDrifting16, 3),
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
            Show(EnemyFrameId.CeresSteamLeftJet24, 3),
            Show(EnemyFrameId.CeresSteamLeftJet32, 3),
            Show(EnemyFrameId.CeresSteamLeftJet40, 3),
            Show(EnemyFrameId.CeresSteamLeftDrifting32, 3),
            Show(EnemyFrameId.CeresSteamLeftDrifting26, 3),
            Show(EnemyFrameId.CeresSteamLeftDrifting16, 3),
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
            Show(EnemyFrameId.CeresSteamDownJet22, 3),
            Show(EnemyFrameId.CeresSteamDownJet31, 3),
            Show(EnemyFrameId.CeresSteamDownJet39, 3),
            Show(EnemyFrameId.CeresSteamDownDrifting31, 3),
            Show(EnemyFrameId.CeresSteamDownDrifting24, 3),
            Show(EnemyFrameId.CeresSteamDownDrifting16, 3),
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
            Show(EnemyFrameId.CeresSteamRightJet24, 3),
            Show(EnemyFrameId.CeresSteamRightJet32, 3),
            Show(EnemyFrameId.CeresSteamRightJet40, 3),
            Show(EnemyFrameId.CeresSteamRightDrifting32, 3),
            Show(EnemyFrameId.CeresSteamRightDrifting26, 3),
            Show(EnemyFrameId.CeresSteamRightDrifting16, 3),
        ],
    });

    /// <summary>
    /// The animation each segment a vent starts or its count-down branches to starts: a dormant vent waits, then rests and
    /// puffs in turn; a puffing vent goes on puffing and resting in turn for good. A vent only rests on the way from its
    /// dormant wait or a puff, so no animation starts at a rest.
    /// </summary>
    internal static IReadOnlyDictionary<CeresSteamSegment, Animation<CeresSteamSegment>> StartingAt { get; } = new Dictionary<CeresSteamSegment, Animation<CeresSteamSegment>>
    {
        [P.UpDormant] = new([new(P.UpDormant, 0, 3, 1), new(P.UpResting, 0, 3, 1), new(P.UpPuffing, 0, 7, 1)], AnimationEnd.LoopsFrom(1)),
        [P.UpPuffing] = new([new(P.UpPuffing, 0, 7, 1), new(P.UpResting, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LeftDormant] = new([new(P.LeftDormant, 0, 3, 1), new(P.LeftResting, 0, 3, 1), new(P.LeftPuffing, 0, 7, 1)], AnimationEnd.LoopsFrom(1)),
        [P.LeftPuffing] = new([new(P.LeftPuffing, 0, 7, 1), new(P.LeftResting, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownDormant] = new([new(P.DownDormant, 0, 3, 1), new(P.DownResting, 0, 3, 1), new(P.DownPuffing, 0, 7, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DownPuffing] = new([new(P.DownPuffing, 0, 7, 1), new(P.DownResting, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RightDormant] = new([new(P.RightDormant, 0, 3, 1), new(P.RightResting, 0, 3, 1), new(P.RightPuffing, 0, 7, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RightPuffing] = new([new(P.RightPuffing, 0, 7, 1), new(P.RightResting, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresSteamSegment> StartingWith(CeresSteamSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CeresSteamCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCue> Cue(CeresSteamCue cue) => new AnimationBeat<EnemyFrameId, CeresSteamCue>.Event(cue);
}
