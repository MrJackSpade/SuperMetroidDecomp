using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Rios.GerutaSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Geruta's idle and swoop animations and its flames' animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="GerutaCue"/> naming what it does.
/// </summary>
internal static class GerutaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<GerutaSegment, EnemyFrameId, GerutaCue> Segments { get; } = new(
        new Dictionary<GerutaSegment, AnimationBeat<EnemyFrameId, GerutaCue>[]>
    {
        [P.Idle] =
        [
            Cue(new GerutaCue.PlaceFlames(8, GerutaFlames.Ascending)),
            Show(EnemyFrameId.GerutaWingsSpread, 13),
            Show(EnemyFrameId.GerutaWingsLevel, 18),
            Show(EnemyFrameId.GerutaWingsSpread, 13),
            Show(EnemyFrameId.GerutaWingsDrooping, 18),
        ],
        [P.StartDescending] =
        [
            Cue(new GerutaCue.PlaceFlames(8, GerutaFlames.Ascending)),
            Show(EnemyFrameId.GerutaWingsLevel, 1),
            Cue(new GerutaCue.PlaceFlames(4, GerutaFlames.Ascending)),
            Show(EnemyFrameId.GerutaWingsFlattening, 1),
            Cue(new GerutaCue.PlaceFlames(0, GerutaFlames.Ascending)),
            Show(EnemyFrameId.GerutaWingsTilting, 1),
            Cue(new GerutaCue.PlaceFlames(-4, GerutaFlames.Descending)),
            Show(EnemyFrameId.GerutaWingsLifting, 1),
            Cue(new GerutaCue.PlaceFlames(-12, GerutaFlames.Descending)),
            Show(EnemyFrameId.GerutaWingsRaising, 1),
            Cue(new GerutaCue.PlaceFlames(-16, GerutaFlames.Descending)),
            Show(EnemyFrameId.GerutaWingsSweptUp, 1),
            Cue(new GerutaCue.SwoopStartFinished()),
        ],
        [P.Descending] =
        [
            Cue(new GerutaCue.PlaceFlames(-12, GerutaFlames.Descending)),
            Hold(EnemyFrameId.GerutaDiving),
        ],
        [P.StartAscending] =
        [
            Cue(new GerutaCue.PlaceFlames(-16, GerutaFlames.Descending)),
            Show(EnemyFrameId.GerutaWingsSweptUp, 1),
            Cue(new GerutaCue.PlaceFlames(-12, GerutaFlames.Descending)),
            Show(EnemyFrameId.GerutaWingsRaising, 1),
            Cue(new GerutaCue.PlaceFlames(-4, GerutaFlames.Descending)),
            Show(EnemyFrameId.GerutaWingsLifting, 1),
            Cue(new GerutaCue.PlaceFlames(0, GerutaFlames.Ascending)),
            Show(EnemyFrameId.GerutaWingsTilting, 1),
            Cue(new GerutaCue.PlaceFlames(4, GerutaFlames.Ascending)),
            Show(EnemyFrameId.GerutaWingsFlattening, 1),
            Cue(new GerutaCue.PlaceFlames(8, GerutaFlames.Ascending)),
            Show(EnemyFrameId.GerutaWingsLevel, 1),
            Cue(new GerutaCue.PlaceFlames(8, GerutaFlames.Ascending)),
            Show(EnemyFrameId.GerutaWingsSpread, 1),
            Cue(new GerutaCue.PlaceFlames(12, GerutaFlames.Ascending)),
            Show(EnemyFrameId.GerutaWingsDrooping, 1),
            Cue(new GerutaCue.SwoopStartFinished()),
        ],
        [P.Ascending] =
        [
            Cue(new GerutaCue.PlaceFlames(12, GerutaFlames.Ascending)),
            Hold(EnemyFrameId.GerutaClimbing),
        ],
        [P.FlamesAscending] =
        [
            Show(EnemyFrameId.GerutaRisingFlameLarge, 6),
            Show(EnemyFrameId.GerutaRisingFlameMedium, 5),
            Show(EnemyFrameId.GerutaRisingFlameSmall, 8),
            Show(EnemyFrameId.GerutaRisingFlameMedium, 6),
        ],
        [P.FlamesDescending] =
        [
            Show(EnemyFrameId.GerutaFallingFlameLarge, 6),
            Show(EnemyFrameId.GerutaFallingFlameMedium, 5),
            Show(EnemyFrameId.GerutaFallingFlameSmall, 8),
            Show(EnemyFrameId.GerutaFallingFlameMedium, 6),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. The dive and the climb place their flames
    /// and hold their one pose (#2512): only those placements move the flames, so placing them again each time the pose
    /// showed again would change nothing.
    /// </summary>
    private static Dictionary<GerutaSegment, Animation<GerutaSegment>> StartingAt { get; } = new Dictionary<GerutaSegment, Animation<GerutaSegment>>
    {
        [P.Idle] = Animation<GerutaSegment>.Looping([Segments.Whole(P.Idle)]),
        [P.StartDescending] = new([Segments.Whole(P.StartDescending)], AnimationEnd.Holds),
        [P.Descending] = new([Segments.Whole(P.Descending)], AnimationEnd.Holds),
        [P.StartAscending] = new([Segments.Whole(P.StartAscending)], AnimationEnd.Holds),
        [P.Ascending] = new([Segments.Whole(P.Ascending)], AnimationEnd.Holds),
        [P.FlamesAscending] = Animation<GerutaSegment>.Looping([Segments.Whole(P.FlamesAscending)]),
        [P.FlamesDescending] = Animation<GerutaSegment>.Looping([Segments.Whole(P.FlamesDescending)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<GerutaSegment> StartingWith(GerutaSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, GerutaCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, GerutaCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, GerutaCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, GerutaCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, GerutaCue> Cue(GerutaCue cue) => new AnimationBeat<EnemyFrameId, GerutaCue>.Event(cue);
}
