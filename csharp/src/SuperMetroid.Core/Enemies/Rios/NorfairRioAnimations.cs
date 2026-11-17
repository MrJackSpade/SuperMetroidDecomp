using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Rios.NorfairRioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.NorfairRioSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Norfair Rio's idle and swoop animations and its flames' animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="NorfairRioCue"/> naming what it does.
/// </summary>
internal static class NorfairRioAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NorfairRioSegment, EnemyFrameId, NorfairRioCue> Segments { get; } = new(
        new Dictionary<NorfairRioSegment, AnimationBeat<EnemyFrameId, NorfairRioCue>[]>
    {
        [P.Idle] =
        [
            Cue(NorfairRioCue.PlaceFlames(8)),
            Show(EnemyFrameId.NorfairRioWingsSpread, 13),
            Show(EnemyFrameId.NorfairRioWingsLevel, 18),
            Show(EnemyFrameId.NorfairRioWingsSpread, 13),
            Show(EnemyFrameId.NorfairRioWingsDrooping, 18),
        ],
        [P.StartDescending] =
        [
            Cue(NorfairRioCue.PlaceFlames(8)),
            Show(EnemyFrameId.NorfairRioWingsLevel, 1),
            Cue(NorfairRioCue.PlaceFlames(4)),
            Show(EnemyFrameId.NorfairRioWingsFlattening, 1),
            Cue(NorfairRioCue.PlaceFlames(0)),
            Show(EnemyFrameId.NorfairRioWingsTilting, 1),
            Cue(NorfairRioCue.PlaceFlames(-4)),
            Show(EnemyFrameId.NorfairRioWingsLifting, 1),
            Cue(NorfairRioCue.PlaceFlames(-12)),
            Show(EnemyFrameId.NorfairRioWingsRaising, 1),
            Cue(NorfairRioCue.PlaceFlames(-16)),
            Show(EnemyFrameId.NorfairRioWingsSweptUp, 1),
            Cue(new(K.SwoopStartFinished)),
        ],
        [P.Descending] =
        [
            Cue(NorfairRioCue.PlaceFlames(-12)),
            Show(EnemyFrameId.NorfairRioDiving, 6),
            Show(EnemyFrameId.NorfairRioDiving, 5),
            Show(EnemyFrameId.NorfairRioDiving, 8),
            Show(EnemyFrameId.NorfairRioDiving, 6),
        ],
        [P.StartAscending] =
        [
            Cue(NorfairRioCue.PlaceFlames(-16)),
            Show(EnemyFrameId.NorfairRioWingsSweptUp, 1),
            Cue(NorfairRioCue.PlaceFlames(-12)),
            Show(EnemyFrameId.NorfairRioWingsRaising, 1),
            Cue(NorfairRioCue.PlaceFlames(-4)),
            Show(EnemyFrameId.NorfairRioWingsLifting, 1),
            Cue(NorfairRioCue.PlaceFlames(0)),
            Show(EnemyFrameId.NorfairRioWingsTilting, 1),
            Cue(NorfairRioCue.PlaceFlames(4)),
            Show(EnemyFrameId.NorfairRioWingsFlattening, 1),
            Cue(NorfairRioCue.PlaceFlames(8)),
            Show(EnemyFrameId.NorfairRioWingsLevel, 1),
            Cue(NorfairRioCue.PlaceFlames(8)),
            Show(EnemyFrameId.NorfairRioWingsSpread, 1),
            Cue(NorfairRioCue.PlaceFlames(12)),
            Show(EnemyFrameId.NorfairRioWingsDrooping, 1),
            Cue(new(K.SwoopStartFinished)),
        ],
        [P.Ascending] =
        [
            Cue(NorfairRioCue.PlaceFlames(12)),
            Show(EnemyFrameId.NorfairRioClimbing, 6),
            Show(EnemyFrameId.NorfairRioClimbing, 5),
            Show(EnemyFrameId.NorfairRioClimbing, 8),
            Show(EnemyFrameId.NorfairRioClimbing, 6),
        ],
        [P.FlamesAscending] =
        [
            Show(EnemyFrameId.NorfairRioRisingFlameLarge, 6),
            Show(EnemyFrameId.NorfairRioRisingFlameMedium, 5),
            Show(EnemyFrameId.NorfairRioRisingFlameSmall, 8),
            Show(EnemyFrameId.NorfairRioRisingFlameMedium, 6),
        ],
        [P.FlamesDescending] =
        [
            Show(EnemyFrameId.NorfairRioFallingFlameLarge, 6),
            Show(EnemyFrameId.NorfairRioFallingFlameMedium, 5),
            Show(EnemyFrameId.NorfairRioFallingFlameSmall, 8),
            Show(EnemyFrameId.NorfairRioFallingFlameMedium, 6),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NorfairRioSegment, Animation<NorfairRioSegment>> StartingAt { get; } = new Dictionary<NorfairRioSegment, Animation<NorfairRioSegment>>
    {
        [P.Idle] = new([new(P.Idle, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StartDescending] = new([new(P.StartDescending, 0, 13, 1)], AnimationEnd.Holds),
        [P.Descending] = new([new(P.Descending, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StartAscending] = new([new(P.StartAscending, 0, 17, 1)], AnimationEnd.Holds),
        [P.Ascending] = new([new(P.Ascending, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlamesAscending] = new([new(P.FlamesAscending, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlamesDescending] = new([new(P.FlamesDescending, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NorfairRioSegment> StartingWith(NorfairRioSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairRioCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NorfairRioCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairRioCue> Cue(NorfairRioCue cue) => new AnimationBeat<EnemyFrameId, NorfairRioCue>.Event(cue);
}
