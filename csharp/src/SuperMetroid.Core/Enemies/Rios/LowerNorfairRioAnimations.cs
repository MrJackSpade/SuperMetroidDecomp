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
        [P.IdleLoop] =
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
        [P.AscendingLoop] =
        [
            Show(EnemyFrameId.LowerNorfairRioWingsSweptTwin, 2),
            Show(EnemyFrameId.LowerNorfairRioWingsSweptTwinB, 2),
            Show(EnemyFrameId.LowerNorfairRioWingsSweptTwinC, 2),
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
    /// The animation that starts at each segment an owner can start or restart at; the loops only run on from the
    /// animations that start them (#2061).
    /// </summary>
    internal static IReadOnlyDictionary<LowerNorfairRioSegment, Animation<LowerNorfairRioSegment>> StartingAt { get; } = new Dictionary<LowerNorfairRioSegment, Animation<LowerNorfairRioSegment>>
    {
        [P.Idle] = new([new(P.Idle, 0, 1, 1), new(P.IdleLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.PrepareToSwoop] = new([new(P.PrepareToSwoop, 0, 11, 1)], AnimationEnd.Holds),
        [P.Descending] = new([new(P.Descending, 0, 2, 1)], AnimationEnd.Holds),
        [P.AscentOpening] = new([new(P.AscentOpening, 0, 5, 1)], AnimationEnd.Holds),
        [P.Ascending] = new([new(P.Ascending, 0, 1, 1), new(P.AscendingLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(1)),
        [P.Cooldown] = new([new(P.Cooldown, 0, 11, 1)], AnimationEnd.Holds),
        [P.Flames] = new([new(P.Flames, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<LowerNorfairRioSegment> StartingWith(LowerNorfairRioSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, LowerNorfairRioCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, LowerNorfairRioCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, LowerNorfairRioCue> Cue(LowerNorfairRioCue cue) => new AnimationBeat<EnemyFrameId, LowerNorfairRioCue>.Event(cue);
}
