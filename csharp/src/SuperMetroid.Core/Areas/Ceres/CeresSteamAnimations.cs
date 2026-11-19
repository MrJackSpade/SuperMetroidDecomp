using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Areas.Ceres.CeresSteamCueKind;
using P = SuperMetroid.Core.Areas.Ceres.CeresSteamSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres steam vents' typed animations (#641). Each frame shows a <see cref="EnemyFrameId"/> and each cue is a
/// <see cref="CeresSteamCueKind"/>. A dormant vent's animation only hides it and holds; its owner keeps it hidden for its
/// drawn wait and then goes on from where a rest ends (<see cref="ActivationAfter"/>, #2480). Every wait the vent spends
/// hidden is a pause of its length, naming no picture.
/// </summary>
internal static class CeresSteamAnimations
{
    /// <summary>How long a vent stays hidden between puffs.</summary>
    private const int RestUpdates = 64;

    /// <summary>How long each picture of a puff shows.</summary>
    private const int PuffPictureUpdates = 3;

    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CeresSteamSegment, EnemyFrameId, CeresSteamCueKind> Segments { get; } = new(
        new Dictionary<CeresSteamSegment, AnimationBeat<EnemyFrameId, CeresSteamCueKind>[]>
    {
        [P.UpDormant] = [Cue(K.Vanish)],
        [P.UpResting] = [Cue(K.Vanish), Wait(RestUpdates), Cue(K.Appear)],
        [P.UpPuffing] =
        [
            Show(EnemyFrameId.CeresSteamUp),
            Show(EnemyFrameId.CeresSteamUpJetBursting),
            Show(EnemyFrameId.CeresSteamUpJetLengthening),
            Show(EnemyFrameId.CeresSteamUpJetFull),
            Show(EnemyFrameId.CeresSteamUpPlumeDrifting),
            Show(EnemyFrameId.CeresSteamUpPlumeScattering),
            Show(EnemyFrameId.CeresSteamUpPlumeFading),
        ],
        [P.LeftDormant] = [Cue(K.Vanish)],
        [P.LeftResting] = [Cue(K.Vanish), Wait(RestUpdates), Cue(K.Appear)],
        [P.LeftPuffing] =
        [
            Show(EnemyFrameId.CeresSteamLeft),
            Show(EnemyFrameId.CeresSteamLeftJetBursting),
            Show(EnemyFrameId.CeresSteamLeftJetLengthening),
            Show(EnemyFrameId.CeresSteamLeftJetFull),
            Show(EnemyFrameId.CeresSteamLeftPlumeDrifting),
            Show(EnemyFrameId.CeresSteamLeftPlumeScattering),
            Show(EnemyFrameId.CeresSteamLeftPlumeFading),
        ],
        [P.DownDormant] = [Cue(K.Vanish)],
        [P.DownResting] = [Cue(K.Vanish), Wait(RestUpdates), Cue(K.Appear)],
        [P.DownPuffing] =
        [
            Show(EnemyFrameId.CeresSteamDown),
            Show(EnemyFrameId.CeresSteamDownJetBursting),
            Show(EnemyFrameId.CeresSteamDownJetLengthening),
            Show(EnemyFrameId.CeresSteamDownJetFull),
            Show(EnemyFrameId.CeresSteamDownPlumeDrifting),
            Show(EnemyFrameId.CeresSteamDownPlumeScattering),
            Show(EnemyFrameId.CeresSteamDownPlumeFading),
        ],
        [P.RightDormant] = [Cue(K.Vanish)],
        [P.RightResting] = [Cue(K.Vanish), Wait(RestUpdates), Cue(K.Appear)],
        [P.RightPuffing] =
        [
            Show(EnemyFrameId.CeresSteamRight),
            Show(EnemyFrameId.CeresSteamRightJetBursting),
            Show(EnemyFrameId.CeresSteamRightJetLengthening),
            Show(EnemyFrameId.CeresSteamRightJetFull),
            Show(EnemyFrameId.CeresSteamRightPlumeDrifting),
            Show(EnemyFrameId.CeresSteamRightPlumeScattering),
            Show(EnemyFrameId.CeresSteamRightPlumeFading),
        ],
    });

    /// <summary>
    /// The animation each segment a vent starts at starts: a dormant vent hides and holds until its owner's wait runs out;
    /// a puffing vent goes on puffing and resting in turn for good. A vent only rests on the way from a puff, so no
    /// animation starts at a rest.
    /// </summary>
    private static IReadOnlyDictionary<CeresSteamSegment, Animation<CeresSteamSegment>> StartingAt { get; } = new Dictionary<CeresSteamSegment, Animation<CeresSteamSegment>>
    {
        [P.UpDormant] = new([Segments.Whole(P.UpDormant)], AnimationEnd.Holds),
        [P.UpPuffing] = Animation<CeresSteamSegment>.Looping([Segments.Whole(P.UpPuffing), Segments.Whole(P.UpResting)]),
        [P.LeftDormant] = new([Segments.Whole(P.LeftDormant)], AnimationEnd.Holds),
        [P.LeftPuffing] = Animation<CeresSteamSegment>.Looping([Segments.Whole(P.LeftPuffing), Segments.Whole(P.LeftResting)]),
        [P.DownDormant] = new([Segments.Whole(P.DownDormant)], AnimationEnd.Holds),
        [P.DownPuffing] = Animation<CeresSteamSegment>.Looping([Segments.Whole(P.DownPuffing), Segments.Whole(P.DownResting)]),
        [P.RightDormant] = new([Segments.Whole(P.RightDormant)], AnimationEnd.Holds),
        [P.RightPuffing] = Animation<CeresSteamSegment>.Looping([Segments.Whole(P.RightPuffing), Segments.Whole(P.RightResting)]),
    };

    /// <summary>
    /// Where each dormant vent's puffs go on from once its wait runs out (#2480): its way's puffs, at the last beat of their
    /// rest, where the vent appears. It appears and shows its puff's first picture on the update its wait runs out, as the
    /// game's vent appears and puffs on the update its wait runs out.
    /// </summary>
    internal static IReadOnlyDictionary<CeresSteamSegment, AnimationPlayback<CeresSteamSegment>> ActivationAfter { get; } =
        new Dictionary<CeresSteamSegment, AnimationPlayback<CeresSteamSegment>>
    {
        [P.UpDormant] = WhereRestEnds(P.UpPuffing, P.UpResting),
        [P.LeftDormant] = WhereRestEnds(P.LeftPuffing, P.LeftResting),
        [P.DownDormant] = WhereRestEnds(P.DownPuffing, P.DownResting),
        [P.RightDormant] = WhereRestEnds(P.RightPuffing, P.RightResting),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresSteamSegment> StartingWith(CeresSteamSegment segment) => StartingAt[segment];

    /// <summary>The puffs that start at <paramref name="puffing"/>, standing at the appearance that ends <paramref name="resting"/>.</summary>
    private static AnimationPlayback<CeresSteamSegment> WhereRestEnds(CeresSteamSegment puffing, CeresSteamSegment resting) =>
        new(StartingAt[puffing], AnimationSegments<CeresSteamSegment, EnemyFrameId, CeresSteamCueKind>.PlaceOf(
            StartingAt[puffing], resting, Segments.Beats(resting).Count - 1));

    /// <summary>Shows <paramref name="frame"/> for one picture of a puff.</summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCueKind> Show(EnemyFrameId frame) =>
        new AnimationBeat<EnemyFrameId, CeresSteamCueKind>.Frame(frame, PuffPictureUpdates);

    /// <summary>
    /// Waits <paramref name="updates"/> updates with the vent hidden, naming no picture: a hidden vent is neither drawn nor
    /// touched, so what it would show there is never seen.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCueKind> Wait(int updates) =>
        new AnimationBeat<EnemyFrameId, CeresSteamCueKind>.Pause(updates);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CeresSteamCueKind> Cue(CeresSteamCueKind cue) =>
        new AnimationBeat<EnemyFrameId, CeresSteamCueKind>.Event(cue);
}
