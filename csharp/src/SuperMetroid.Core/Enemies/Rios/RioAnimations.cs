using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Rios.RioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.RioSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Rio's idle, swoop and cooldown animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="RioCue"/> naming what it does.
/// </summary>
internal static class RioAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RioSegment, EnemyFrameId, RioCue> Segments { get; } = new(
        new Dictionary<RioSegment, AnimationBeat<EnemyFrameId, RioCue>[]>
    {
        [P.Idle] = TuckedWingbeat,
        [P.PostSwoopIdle] = TuckedWingbeat,
        [P.HangingWingbeat] =
        [
            Show(EnemyFrameId.RioWingsUpLegsHanging, 4),
            Show(EnemyFrameId.RioWingsLevelLegsHanging, 4),
        ],
        [P.SwoopOpening] =
        [
            Show(EnemyFrameId.RioWingsUpLegsHanging, 3),
            Show(EnemyFrameId.RioWingsLevelLegsHanging, 3),
            Show(EnemyFrameId.RioWingsUpLegsSpread, 3),
            Show(EnemyFrameId.RioWingsLevelLegsSpread, 3),
            Show(EnemyFrameId.RioWingsUpClawsCurled, 3),
            Cue(new(K.AnimationFinished)),
        ],
        [P.SwoopLoop] =
        [
            Show(EnemyFrameId.RioWingsLevelClawsCurled, 3),
            Show(EnemyFrameId.RioWingsUpClawsCurled, 3),
        ],
        [P.SwoopCooldown] =
        [
            Show(EnemyFrameId.RioWingsUpClawsCurled, 3),
            Show(EnemyFrameId.RioWingsLevelLegsSpread, 3),
            Show(EnemyFrameId.RioWingsUpLegsSpread, 3),
            Show(EnemyFrameId.RioWingsLevelLegsHanging, 3),
            Show(EnemyFrameId.RioWingsUpLegsHanging, 3),
            Cue(new(K.AnimationFinished)),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. The idle's hover is its tucked
    /// wingbeat three times and its hanging wingbeat twice (#2451); no owner starts the hanging wingbeat itself.
    /// </summary>
    internal static IReadOnlyDictionary<RioSegment, Animation<RioSegment>> StartingAt { get; } = new Dictionary<RioSegment, Animation<RioSegment>>
    {
        [P.Idle] = Animation<RioSegment>.Looping([Segments.Whole(P.Idle), .. PostSwoopHover()]),
        [P.PostSwoopIdle] = Animation<RioSegment>.Looping([.. PostSwoopHover(), Segments.Whole(P.Idle)]),
        [P.SwoopOpening] = new([Segments.Whole(P.SwoopOpening)], AnimationEnd.Holds),
        [P.SwoopLoop] = Animation<RioSegment>.Looping([Segments.Whole(P.SwoopLoop)]),
        [P.SwoopCooldown] = new([Segments.Whole(P.SwoopCooldown)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RioSegment> StartingWith(RioSegment segment) => StartingAt[segment];

    /// <summary>The times the hover after a swoop beats its wings with its legs tucked before they hang.</summary>
    private const int TuckedWingbeats = 3;

    /// <summary>The times the hover beats its wings with its legs hanging before the idle starts again.</summary>
    private const int HangingWingbeats = 2;

    /// <summary>A wingbeat with the Rio's legs tucked, which the idle and the hover after a swoop play (#2451).</summary>
    private static AnimationBeat<EnemyFrameId, RioCue>[] TuckedWingbeat =>
    [
        Show(EnemyFrameId.RioWingsUpLegsTucked, 4),
        Show(EnemyFrameId.RioWingsLevelLegsTucked, 4),
    ];

    /// <summary>The hover after a swoop (#2451): its tucked wingbeats, then its hanging ones.</summary>
    private static AnimationPhase<RioSegment>[] PostSwoopHover() =>
        [Segments.Repeated(P.PostSwoopIdle, TuckedWingbeats), Segments.Repeated(P.HangingWingbeat, HangingWingbeats)];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, RioCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, RioCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, RioCue> Cue(RioCue cue) => new AnimationBeat<EnemyFrameId, RioCue>.Event(cue);
}
