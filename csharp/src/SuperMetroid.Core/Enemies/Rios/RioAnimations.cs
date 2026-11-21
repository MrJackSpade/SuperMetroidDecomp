using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Rios.RioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.RioSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Rio's idle, swoop and cooldown animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="RioCue"/> naming what it does.
/// </summary>
internal static partial class RioAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RioSegment, EnemyFrameId, RioCue> Segments { get; } = new(
        new Dictionary<RioSegment, AnimationBeat<EnemyFrameId, RioCue>[]>
    {
        [P.TuckedWingbeat] =
        [
            Show(EnemyFrameId.RioWingsUpLegsTucked, 4),
            Show(EnemyFrameId.RioWingsLevelLegsTucked, 4),
        ],
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
    /// The animation that starts at each segment an owner can start or restart at: at the tucked wingbeat the idle, one
    /// tucked wingbeat before the hover (#2451, #2512). No owner starts the hanging wingbeat itself.
    /// </summary>
    internal static Dictionary<RioSegment, Animation<RioSegment>> StartingAt { get; } = new Dictionary<RioSegment, Animation<RioSegment>>
    {
        [P.TuckedWingbeat] = Animation<RioSegment>.Looping([Segments.Whole(P.TuckedWingbeat), .. Hover()]),
        [P.SwoopOpening] = new([Segments.Whole(P.SwoopOpening)], AnimationEnd.Holds),
        [P.SwoopLoop] = Animation<RioSegment>.Looping([Segments.Whole(P.SwoopLoop)]),
        [P.SwoopCooldown] = new([Segments.Whole(P.SwoopCooldown)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RioSegment> StartingWith(RioSegment segment) => StartingAt[segment];

    /// <summary>
    /// The hover a swoop ends in (#2451, #2512): the idle from its hover on, its tucked wingbeats and its hanging ones, then
    /// the idle's own tucked wingbeat before the hover again.
    /// </summary>
    internal static Animation<RioSegment> PostSwoopHover { get; } =
        Animation<RioSegment>.Looping([.. Hover(), Segments.Whole(P.TuckedWingbeat)]);

    /// <summary>The times the hover after a swoop beats its wings with its legs tucked before they hang.</summary>
    private const int TuckedWingbeats = 3;

    /// <summary>The times the hover beats its wings with its legs hanging before the idle starts again.</summary>
    private const int HangingWingbeats = 2;

    /// <summary>The hover (#2451): its tucked wingbeats, then its hanging ones.</summary>
    private static AnimationPhase<RioSegment>[] Hover() =>
        [Segments.Repeated(P.TuckedWingbeat, TuckedWingbeats), Segments.Repeated(P.HangingWingbeat, HangingWingbeats)];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, RioCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, RioCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, RioCue> Cue(RioCue cue) => new AnimationBeat<EnemyFrameId, RioCue>.Event(cue);
}
