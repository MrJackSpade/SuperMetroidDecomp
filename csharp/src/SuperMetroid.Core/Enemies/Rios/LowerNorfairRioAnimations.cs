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
            Hold(EnemyFrameId.LowerNorfairRioWingsFolded),
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
            Hold(EnemyFrameId.LowerNorfairRioWingsSwept),
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
    /// The animation that starts at each segment an owner can start or restart at, each segment played whole; the idle's
    /// wingbeat loops on from the entry cue before it (#2061, #2276), and the climb shows its flames and holds its swept
    /// wings (#2512).
    /// </summary>
    internal static IReadOnlyDictionary<LowerNorfairRioSegment, Animation<LowerNorfairRioSegment>> StartingAt { get; } = new Dictionary<LowerNorfairRioSegment, Animation<LowerNorfairRioSegment>>
    {
        [P.Idle] = Animation<LowerNorfairRioSegment>.LeadingInto([Segments.Whole(P.Idle)], [Segments.Whole(P.IdleWingbeat)]),
        [P.PrepareToSwoop] = new([Segments.Whole(P.PrepareToSwoop)], AnimationEnd.Holds),
        [P.Descending] = new([Segments.Whole(P.Descending)], AnimationEnd.Holds),
        [P.AscentOpening] = new([Segments.Whole(P.AscentOpening)], AnimationEnd.Holds),
        [P.Ascending] = new([Segments.Whole(P.Ascending)], AnimationEnd.Holds),
        [P.Cooldown] = new([Segments.Whole(P.Cooldown)], AnimationEnd.Holds),
        [P.Flames] = Animation<LowerNorfairRioSegment>.Looping([Segments.Whole(P.Flames)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<LowerNorfairRioSegment> StartingWith(LowerNorfairRioSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, LowerNorfairRioCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, LowerNorfairRioCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, LowerNorfairRioCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, LowerNorfairRioCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, LowerNorfairRioCue> Cue(LowerNorfairRioCue cue) => new AnimationBeat<EnemyFrameId, LowerNorfairRioCue>.Event(cue);
}
