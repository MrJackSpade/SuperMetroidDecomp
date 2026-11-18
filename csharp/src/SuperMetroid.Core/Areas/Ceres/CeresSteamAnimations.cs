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
            Show(EnemyFrameId.CeresSteamUpJetBursting, 3),
            Show(EnemyFrameId.CeresSteamUpJetLengthening, 3),
            Show(EnemyFrameId.CeresSteamUpJetFull, 3),
            Show(EnemyFrameId.CeresSteamUpPlumeDrifting, 3),
            Show(EnemyFrameId.CeresSteamUpPlumeScattering, 3),
            Show(EnemyFrameId.CeresSteamUpPlumeFading, 3),
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
            Show(EnemyFrameId.CeresSteamLeftJetBursting, 3),
            Show(EnemyFrameId.CeresSteamLeftJetLengthening, 3),
            Show(EnemyFrameId.CeresSteamLeftJetFull, 3),
            Show(EnemyFrameId.CeresSteamLeftPlumeDrifting, 3),
            Show(EnemyFrameId.CeresSteamLeftPlumeScattering, 3),
            Show(EnemyFrameId.CeresSteamLeftPlumeFading, 3),
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
            Show(EnemyFrameId.CeresSteamDownJetBursting, 3),
            Show(EnemyFrameId.CeresSteamDownJetLengthening, 3),
            Show(EnemyFrameId.CeresSteamDownJetFull, 3),
            Show(EnemyFrameId.CeresSteamDownPlumeDrifting, 3),
            Show(EnemyFrameId.CeresSteamDownPlumeScattering, 3),
            Show(EnemyFrameId.CeresSteamDownPlumeFading, 3),
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
            Show(EnemyFrameId.CeresSteamRightJetBursting, 3),
            Show(EnemyFrameId.CeresSteamRightJetLengthening, 3),
            Show(EnemyFrameId.CeresSteamRightJetFull, 3),
            Show(EnemyFrameId.CeresSteamRightPlumeDrifting, 3),
            Show(EnemyFrameId.CeresSteamRightPlumeScattering, 3),
            Show(EnemyFrameId.CeresSteamRightPlumeFading, 3),
        ],
    });

    /// <summary>
    /// The animation each segment a vent starts or its count-down branches to starts: a dormant vent waits, then rests and
    /// puffs in turn; a puffing vent goes on puffing and resting in turn for good. A vent only rests on the way from its
    /// dormant wait or a puff, so no animation starts at a rest.
    /// </summary>
    internal static IReadOnlyDictionary<CeresSteamSegment, Animation<CeresSteamSegment>> StartingAt { get; } = new Dictionary<CeresSteamSegment, Animation<CeresSteamSegment>>
    {
        [P.UpDormant] = Animation<CeresSteamSegment>.LeadingInto([new(P.UpDormant, 0, 3, 1)], [new(P.UpResting, 0, 3, 1), new(P.UpPuffing, 0, 7, 1)]),
        [P.UpPuffing] = Animation<CeresSteamSegment>.Looping([new(P.UpPuffing, 0, 7, 1), new(P.UpResting, 0, 3, 1)]),
        [P.LeftDormant] = Animation<CeresSteamSegment>.LeadingInto([new(P.LeftDormant, 0, 3, 1)], [new(P.LeftResting, 0, 3, 1), new(P.LeftPuffing, 0, 7, 1)]),
        [P.LeftPuffing] = Animation<CeresSteamSegment>.Looping([new(P.LeftPuffing, 0, 7, 1), new(P.LeftResting, 0, 3, 1)]),
        [P.DownDormant] = Animation<CeresSteamSegment>.LeadingInto([new(P.DownDormant, 0, 3, 1)], [new(P.DownResting, 0, 3, 1), new(P.DownPuffing, 0, 7, 1)]),
        [P.DownPuffing] = Animation<CeresSteamSegment>.Looping([new(P.DownPuffing, 0, 7, 1), new(P.DownResting, 0, 3, 1)]),
        [P.RightDormant] = Animation<CeresSteamSegment>.LeadingInto([new(P.RightDormant, 0, 3, 1)], [new(P.RightResting, 0, 3, 1), new(P.RightPuffing, 0, 7, 1)]),
        [P.RightPuffing] = Animation<CeresSteamSegment>.Looping([new(P.RightPuffing, 0, 7, 1), new(P.RightResting, 0, 3, 1)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresSteamSegment> StartingWith(CeresSteamSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CeresSteamCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCue> Cue(CeresSteamCue cue) => new AnimationBeat<EnemyFrameId, CeresSteamCue>.Event(cue);
}
