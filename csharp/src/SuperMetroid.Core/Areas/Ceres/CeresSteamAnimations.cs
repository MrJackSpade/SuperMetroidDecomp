using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Areas.Ceres.CeresSteamCueKind;
using P = SuperMetroid.Core.Areas.Ceres.CeresSteamSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres steam vents' typed animations (#641). Each frame shows a <see cref="EnemyFrameId"/> and each cue is a
/// <see cref="CeresSteamCueKind"/>. A dormant vent's animation only hides it and holds; its owner keeps it hidden for its
/// drawn wait and then goes on from where a rest ends (<see cref="ActivationOf"/>, #2480). Every wait the vent spends
/// hidden is a pause of its length, naming no picture. Every way a vent blows plays these over its own puff
/// (<see cref="CeresSteamWay"/>).
/// </summary>
internal static partial class CeresSteamAnimations
{
    /// <summary>How long a vent stays hidden between puffs.</summary>
    internal const int RestUpdates = 64;

    /// <summary>How long each picture of a puff shows.</summary>
    internal const int PuffPictureUpdates = 3;

    /// <summary>
    /// The segments of a vent whose puff shows <paramref name="puff"/>: its dormant wait hides the vent, its rest hides it
    /// for <see cref="RestUpdates"/> and shows it again, and its puff shows the pictures.
    /// </summary>
    internal static AnimationSegments<CeresSteamSegment, EnemyFrameId, CeresSteamCueKind> Blowing(IReadOnlyList<EnemyFrameId> puff) => new(
        new Dictionary<CeresSteamSegment, AnimationBeat<EnemyFrameId, CeresSteamCueKind>[]>
        {
            [P.Dormant] = [Cue(K.Vanish)],
            [P.Resting] = [Cue(K.Vanish), Wait(RestUpdates), Cue(K.Appear)],
            [P.Puffing] = [.. puff.Select(Show)],
        });

    /// <summary>
    /// The animation each segment a vent starts at starts, over its way's <paramref name="segments"/>: a dormant vent hides
    /// and holds until its owner's wait runs out; a puffing vent goes on puffing and resting in turn for good. A vent only
    /// rests on the way from a puff, so no animation starts at a rest.
    /// </summary>
    internal static IReadOnlyDictionary<CeresSteamSegment, Animation<CeresSteamSegment>> StartingAt(
        AnimationSegments<CeresSteamSegment, EnemyFrameId, CeresSteamCueKind> segments) =>
        new Dictionary<CeresSteamSegment, Animation<CeresSteamSegment>>
        {
            [P.Dormant] = new([segments.Whole(P.Dormant)], AnimationEnd.Holds),
            [P.Puffing] = Animation<CeresSteamSegment>.Looping([segments.Whole(P.Puffing), segments.Whole(P.Resting)]),
        };

    /// <summary>
    /// Where a dormant vent's puffs go on from once its wait runs out (#2480): the puffs <paramref name="startingAt"/> starts,
    /// at the last beat of their rest, where the vent appears. It appears and shows its puff's first picture on the update
    /// its wait runs out, as the game's vent appears and puffs on the update its wait runs out.
    /// </summary>
    internal static AnimationPlayback<CeresSteamSegment> ActivationOf(IReadOnlyDictionary<CeresSteamSegment, Animation<CeresSteamSegment>> startingAt)
    {
        Animation<CeresSteamSegment> puffs = startingAt[P.Puffing];
        AnimationPhase<CeresSteamSegment> rest = puffs.Phases.Single(phase => phase.Segment == P.Resting);
        return new(puffs, AnimationSegments<CeresSteamSegment, EnemyFrameId, CeresSteamCueKind>.PlaceOf(puffs, P.Resting, rest.BeatCount - 1));
    }

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
