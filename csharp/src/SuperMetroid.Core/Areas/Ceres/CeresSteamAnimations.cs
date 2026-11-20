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

    /// <summary>
    /// The four ways a vent blows, each with its dormant wait, rest and puff segments and its puff's seven pictures in
    /// order; every way's segments are built from these by the one vent animation below (#2557).
    /// </summary>
    private static readonly CeresSteamWay[] Ways =
    [
        new(P.UpDormant, P.UpResting, P.UpPuffing,
        [
            EnemyFrameId.CeresSteamUp, EnemyFrameId.CeresSteamUpJetBursting, EnemyFrameId.CeresSteamUpJetLengthening, EnemyFrameId.CeresSteamUpJetFull,
            EnemyFrameId.CeresSteamUpPlumeDrifting, EnemyFrameId.CeresSteamUpPlumeScattering, EnemyFrameId.CeresSteamUpPlumeFading,
        ]),
        new(P.LeftDormant, P.LeftResting, P.LeftPuffing,
        [
            EnemyFrameId.CeresSteamLeft, EnemyFrameId.CeresSteamLeftJetBursting, EnemyFrameId.CeresSteamLeftJetLengthening, EnemyFrameId.CeresSteamLeftJetFull,
            EnemyFrameId.CeresSteamLeftPlumeDrifting, EnemyFrameId.CeresSteamLeftPlumeScattering, EnemyFrameId.CeresSteamLeftPlumeFading,
        ]),
        new(P.DownDormant, P.DownResting, P.DownPuffing,
        [
            EnemyFrameId.CeresSteamDown, EnemyFrameId.CeresSteamDownJetBursting, EnemyFrameId.CeresSteamDownJetLengthening, EnemyFrameId.CeresSteamDownJetFull,
            EnemyFrameId.CeresSteamDownPlumeDrifting, EnemyFrameId.CeresSteamDownPlumeScattering, EnemyFrameId.CeresSteamDownPlumeFading,
        ]),
        new(P.RightDormant, P.RightResting, P.RightPuffing,
        [
            EnemyFrameId.CeresSteamRight, EnemyFrameId.CeresSteamRightJetBursting, EnemyFrameId.CeresSteamRightJetLengthening, EnemyFrameId.CeresSteamRightJetFull,
            EnemyFrameId.CeresSteamRightPlumeDrifting, EnemyFrameId.CeresSteamRightPlumeScattering, EnemyFrameId.CeresSteamRightPlumeFading,
        ]),
    ];

    /// <summary>Every segment's beats: each way's dormant wait hides the vent, its rest hides it for <see cref="RestUpdates"/> and shows it again, and its puff shows its pictures.</summary>
    internal static AnimationSegments<CeresSteamSegment, EnemyFrameId, CeresSteamCueKind> Segments { get; } = new(
        Ways.SelectMany(way => new KeyValuePair<CeresSteamSegment, AnimationBeat<EnemyFrameId, CeresSteamCueKind>[]>[]
        {
            new(way.Dormant, [Cue(K.Vanish)]),
            new(way.Resting, [Cue(K.Vanish), Wait(RestUpdates), Cue(K.Appear)]),
            new(way.Puffing, [.. way.Puff.Select(Show)]),
        }).ToDictionary());

    /// <summary>
    /// The animation each segment a vent starts at starts: a dormant vent hides and holds until its owner's wait runs out;
    /// a puffing vent goes on puffing and resting in turn for good. A vent only rests on the way from a puff, so no
    /// animation starts at a rest.
    /// </summary>
    private static Dictionary<CeresSteamSegment, Animation<CeresSteamSegment>> StartingAt { get; } =
        Ways.SelectMany(way => new KeyValuePair<CeresSteamSegment, Animation<CeresSteamSegment>>[]
        {
            new(way.Dormant, new([Segments.Whole(way.Dormant)], AnimationEnd.Holds)),
            new(way.Puffing, Animation<CeresSteamSegment>.Looping([Segments.Whole(way.Puffing), Segments.Whole(way.Resting)])),
        }).ToDictionary();

    /// <summary>
    /// Where each dormant vent's puffs go on from once its wait runs out (#2480): its way's puffs, at the last beat of their
    /// rest, where the vent appears. It appears and shows its puff's first picture on the update its wait runs out, as the
    /// game's vent appears and puffs on the update its wait runs out.
    /// </summary>
    internal static IReadOnlyDictionary<CeresSteamSegment, AnimationPlayback<CeresSteamSegment>> ActivationAfter { get; } =
        Ways.ToDictionary(way => way.Dormant, way => WhereRestEnds(way.Puffing, way.Resting));

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
