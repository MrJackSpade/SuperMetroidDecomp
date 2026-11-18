using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Rios.LowerNorfairRioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.LowerNorfairRioSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Lower Norfair Rio's (Holtz's) idle, swoop and cooldown animations and its flames' animation as typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="LowerNorfairRioCue"/> naming what it does.
/// </summary>
internal static class LowerNorfairRioAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<LowerNorfairRioSegment, EnemyFrameId, LowerNorfairRioCue> Segments { get; } = new(
        new Dictionary<LowerNorfairRioSegment, AnimationBeat<EnemyFrameId, LowerNorfairRioCue>[]>
    {
        [P.Idle] =
        [
            Cue(new(K.HideFlames)),
        ],
        [P.IdleWingbeat] =
        [
            Show(EnemyFrameId.LowerNorfairRioWingsRaised, 11),
            Show(EnemyFrameId.LowerNorfairRioWingsLevel, 11),
            Show(EnemyFrameId.LowerNorfairRioWingsLevelDropped, 11),
            Show(EnemyFrameId.LowerNorfairRioWingsLevel, 11),
        ],
        [P.PrepareToSwoop] =
        [
            Cue(new(K.HideFlames)),
            Show(EnemyFrameId.LowerNorfairRioWingsLevelDropped, 3),
            Show(EnemyFrameId.LowerNorfairRioWingsLifting, 3),
            Show(EnemyFrameId.LowerNorfairRioWingsTilting, 3),
            Show(EnemyFrameId.LowerNorfairRioWingsOpening, 3),
            Show(EnemyFrameId.LowerNorfairRioWingsSpread, 2),
            Show(EnemyFrameId.LowerNorfairRioWingsFullySpread, 1),
            Show(EnemyFrameId.LowerNorfairRioWingsClosing, 2),
            Show(EnemyFrameId.LowerNorfairRioWingsFolding, 3),
            Show(EnemyFrameId.LowerNorfairRioWingsFolded, 3),
            Cue(new(K.AnimationFinished)),
        ],
        [P.Descending] =
        [
            Cue(new(K.HideFlames)),
            Show(EnemyFrameId.LowerNorfairRioWingsFolded, 1),
        ],
        [P.AscentOpening] =
        [
            Cue(new(K.HideFlames)),
            Show(EnemyFrameId.LowerNorfairRioWingsFolded, 3),
            Show(EnemyFrameId.LowerNorfairRioWingsFolding, 3),
            Show(EnemyFrameId.LowerNorfairRioWingsSwept, 3),
            Cue(new(K.AnimationFinished)),
        ],
        [P.Ascending] =
        [
            Cue(new(K.ShowFlames)),
        ],
        [P.AscendingWingbeat] =
        [
            Show(EnemyFrameId.LowerNorfairRioWingsSwept, 2),
            Show(EnemyFrameId.LowerNorfairRioWingsSwept, 2),
            Show(EnemyFrameId.LowerNorfairRioWingsSwept, 2),
        ],
        [P.Cooldown] =
        [
            Cue(new(K.ShowFlames)),
            Show(EnemyFrameId.LowerNorfairRioWingsSwept, 3),
            Show(EnemyFrameId.LowerNorfairRioWingsFolding, 3),
            Show(EnemyFrameId.LowerNorfairRioWingsClosing, 2),
            Show(EnemyFrameId.LowerNorfairRioWingsFullySpread, 1),
            Show(EnemyFrameId.LowerNorfairRioWingsSpread, 2),
            Show(EnemyFrameId.LowerNorfairRioWingsOpening, 3),
            Show(EnemyFrameId.LowerNorfairRioWingsTilting, 1),
            Show(EnemyFrameId.LowerNorfairRioWingsLifting, 1),
            Show(EnemyFrameId.LowerNorfairRioWingsLevelDropped, 1),
            Cue(new(K.AnimationFinished)),
        ],
        [P.Flames] =
        [
            Show(EnemyFrameId.LowerNorfairRioFlameLarge, 6),
            Show(EnemyFrameId.LowerNorfairRioFlameMedium, 4),
            Show(EnemyFrameId.LowerNorfairRioFlameSmall, 3),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at, each segment played whole; the
    /// wingbeats loop on from the entry cues before them (#2061, #2276).
    /// </summary>
    internal static IReadOnlyDictionary<LowerNorfairRioSegment, Animation<LowerNorfairRioSegment>> StartingAt { get; } = new Dictionary<LowerNorfairRioSegment, Animation<LowerNorfairRioSegment>>
    {
        [P.Idle] = new([Segments.Whole(P.Idle), Segments.Whole(P.IdleWingbeat)], AnimationEnd.LoopsFrom(1)),
        [P.PrepareToSwoop] = new([Segments.Whole(P.PrepareToSwoop)], AnimationEnd.Holds),
        [P.Descending] = new([Segments.Whole(P.Descending)], AnimationEnd.Holds),
        [P.AscentOpening] = new([Segments.Whole(P.AscentOpening)], AnimationEnd.Holds),
        [P.Ascending] = new([Segments.Whole(P.Ascending), Segments.Whole(P.AscendingWingbeat)], AnimationEnd.LoopsFrom(1)),
        [P.Cooldown] = new([Segments.Whole(P.Cooldown)], AnimationEnd.Holds),
        [P.Flames] = new([Segments.Whole(P.Flames)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<LowerNorfairRioSegment> StartingWith(LowerNorfairRioSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, LowerNorfairRioCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, LowerNorfairRioCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, LowerNorfairRioCue> Cue(LowerNorfairRioCue cue) => new AnimationBeat<EnemyFrameId, LowerNorfairRioCue>.Event(cue);
}
