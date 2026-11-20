using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Rios.HoltzCueKind;
using P = SuperMetroid.Core.Enemies.Rios.HoltzSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Holtz's (Holtz's) idle, swoop and cooldown animations and its flames' animation as typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="HoltzCue"/> naming what it does.
/// </summary>
internal static class HoltzAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<HoltzSegment, EnemyFrameId, HoltzCue> Segments { get; } = new(
        new Dictionary<HoltzSegment, AnimationBeat<EnemyFrameId, HoltzCue>[]>
    {
        [P.Idle] =
        [
            Cue(new(K.HideFlames)),
        ],
        [P.IdleWingbeat] =
        [
            Show(EnemyFrameId.HoltzWingsRaised, 11),
            Show(EnemyFrameId.HoltzWingsLevel, 11),
            Show(EnemyFrameId.HoltzWingsLevelDropped, 11),
            Show(EnemyFrameId.HoltzWingsLevel, 11),
        ],
        [P.PrepareToSwoop] =
        [
            Cue(new(K.HideFlames)),
            Show(EnemyFrameId.HoltzWingsLevelDropped, 3),
            Show(EnemyFrameId.HoltzWingsLifting, 3),
            Show(EnemyFrameId.HoltzWingsTilting, 3),
            Show(EnemyFrameId.HoltzWingsOpening, 3),
            Show(EnemyFrameId.HoltzWingsSpread, 2),
            Show(EnemyFrameId.HoltzWingsFullySpread, 1),
            Show(EnemyFrameId.HoltzWingsClosing, 2),
            Show(EnemyFrameId.HoltzWingsFolding, 3),
            Show(EnemyFrameId.HoltzWingsFolded, 3),
            Cue(new(K.AnimationFinished)),
        ],
        [P.Descending] =
        [
            Cue(new(K.HideFlames)),
            Hold(EnemyFrameId.HoltzWingsFolded),
        ],
        [P.AscentOpening] =
        [
            Cue(new(K.HideFlames)),
            Show(EnemyFrameId.HoltzWingsFolded, 3),
            Show(EnemyFrameId.HoltzWingsFolding, 3),
            Show(EnemyFrameId.HoltzWingsSwept, 3),
            Cue(new(K.AnimationFinished)),
        ],
        [P.Ascending] =
        [
            Cue(new(K.ShowFlames)),
            Hold(EnemyFrameId.HoltzWingsSwept),
        ],
        [P.Cooldown] =
        [
            Cue(new(K.ShowFlames)),
            Show(EnemyFrameId.HoltzWingsSwept, 3),
            Show(EnemyFrameId.HoltzWingsFolding, 3),
            Show(EnemyFrameId.HoltzWingsClosing, 2),
            Show(EnemyFrameId.HoltzWingsFullySpread, 1),
            Show(EnemyFrameId.HoltzWingsSpread, 2),
            Show(EnemyFrameId.HoltzWingsOpening, 3),
            Show(EnemyFrameId.HoltzWingsTilting, 1),
            Show(EnemyFrameId.HoltzWingsLifting, 1),
            Show(EnemyFrameId.HoltzWingsLevelDropped, 1),
            Cue(new(K.AnimationFinished)),
        ],
        [P.Flames] =
        [
            Show(EnemyFrameId.HoltzFlameLarge, 6),
            Show(EnemyFrameId.HoltzFlameMedium, 4),
            Show(EnemyFrameId.HoltzFlameSmall, 3),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at, each segment played whole; the idle's
    /// wingbeat loops on from the entry cue before it (#2061, #2276), and the climb shows its flames and holds its swept
    /// wings (#2512).
    /// </summary>
    private static Dictionary<HoltzSegment, Animation<HoltzSegment>> StartingAt { get; } = new Dictionary<HoltzSegment, Animation<HoltzSegment>>
    {
        [P.Idle] = Animation<HoltzSegment>.LeadingInto([Segments.Whole(P.Idle)], [Segments.Whole(P.IdleWingbeat)]),
        [P.PrepareToSwoop] = new([Segments.Whole(P.PrepareToSwoop)], AnimationEnd.Holds),
        [P.Descending] = new([Segments.Whole(P.Descending)], AnimationEnd.Holds),
        [P.AscentOpening] = new([Segments.Whole(P.AscentOpening)], AnimationEnd.Holds),
        [P.Ascending] = new([Segments.Whole(P.Ascending)], AnimationEnd.Holds),
        [P.Cooldown] = new([Segments.Whole(P.Cooldown)], AnimationEnd.Holds),
        [P.Flames] = Animation<HoltzSegment>.Looping([Segments.Whole(P.Flames)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<HoltzSegment> StartingWith(HoltzSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, HoltzCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, HoltzCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, HoltzCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, HoltzCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, HoltzCue> Cue(HoltzCue cue) => new AnimationBeat<EnemyFrameId, HoltzCue>.Event(cue);
}
