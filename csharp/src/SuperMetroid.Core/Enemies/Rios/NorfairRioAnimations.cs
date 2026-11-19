using SuperMetroid.Core.Animation;
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
            Cue(new NorfairRioCue.PlaceFlames(8, NorfairRioFlames.Ascending)),
            Show(EnemyFrameId.NorfairRioWingsSpread, 13),
            Show(EnemyFrameId.NorfairRioWingsLevel, 18),
            Show(EnemyFrameId.NorfairRioWingsSpread, 13),
            Show(EnemyFrameId.NorfairRioWingsDrooping, 18),
        ],
        [P.StartDescending] =
        [
            Cue(new NorfairRioCue.PlaceFlames(8, NorfairRioFlames.Ascending)),
            Show(EnemyFrameId.NorfairRioWingsLevel, 1),
            Cue(new NorfairRioCue.PlaceFlames(4, NorfairRioFlames.Ascending)),
            Show(EnemyFrameId.NorfairRioWingsFlattening, 1),
            Cue(new NorfairRioCue.PlaceFlames(0, NorfairRioFlames.Ascending)),
            Show(EnemyFrameId.NorfairRioWingsTilting, 1),
            Cue(new NorfairRioCue.PlaceFlames(-4, NorfairRioFlames.Descending)),
            Show(EnemyFrameId.NorfairRioWingsLifting, 1),
            Cue(new NorfairRioCue.PlaceFlames(-12, NorfairRioFlames.Descending)),
            Show(EnemyFrameId.NorfairRioWingsRaising, 1),
            Cue(new NorfairRioCue.PlaceFlames(-16, NorfairRioFlames.Descending)),
            Show(EnemyFrameId.NorfairRioWingsSweptUp, 1),
            Cue(new NorfairRioCue.SwoopStartFinished()),
        ],
        [P.Descending] =
        [
            Cue(new NorfairRioCue.PlaceFlames(-12, NorfairRioFlames.Descending)),
            Hold(EnemyFrameId.NorfairRioDiving),
        ],
        [P.StartAscending] =
        [
            Cue(new NorfairRioCue.PlaceFlames(-16, NorfairRioFlames.Descending)),
            Show(EnemyFrameId.NorfairRioWingsSweptUp, 1),
            Cue(new NorfairRioCue.PlaceFlames(-12, NorfairRioFlames.Descending)),
            Show(EnemyFrameId.NorfairRioWingsRaising, 1),
            Cue(new NorfairRioCue.PlaceFlames(-4, NorfairRioFlames.Descending)),
            Show(EnemyFrameId.NorfairRioWingsLifting, 1),
            Cue(new NorfairRioCue.PlaceFlames(0, NorfairRioFlames.Ascending)),
            Show(EnemyFrameId.NorfairRioWingsTilting, 1),
            Cue(new NorfairRioCue.PlaceFlames(4, NorfairRioFlames.Ascending)),
            Show(EnemyFrameId.NorfairRioWingsFlattening, 1),
            Cue(new NorfairRioCue.PlaceFlames(8, NorfairRioFlames.Ascending)),
            Show(EnemyFrameId.NorfairRioWingsLevel, 1),
            Cue(new NorfairRioCue.PlaceFlames(8, NorfairRioFlames.Ascending)),
            Show(EnemyFrameId.NorfairRioWingsSpread, 1),
            Cue(new NorfairRioCue.PlaceFlames(12, NorfairRioFlames.Ascending)),
            Show(EnemyFrameId.NorfairRioWingsDrooping, 1),
            Cue(new NorfairRioCue.SwoopStartFinished()),
        ],
        [P.Ascending] =
        [
            Cue(new NorfairRioCue.PlaceFlames(12, NorfairRioFlames.Ascending)),
            Hold(EnemyFrameId.NorfairRioClimbing),
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

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. The dive and the climb place their flames
    /// and hold their one pose (#2512): only those placements move the flames, so placing them again each time the pose
    /// showed again would change nothing.
    /// </summary>
    private static Dictionary<NorfairRioSegment, Animation<NorfairRioSegment>> StartingAt { get; } = new Dictionary<NorfairRioSegment, Animation<NorfairRioSegment>>
    {
        [P.Idle] = Animation<NorfairRioSegment>.Looping([Segments.Whole(P.Idle)]),
        [P.StartDescending] = new([Segments.Whole(P.StartDescending)], AnimationEnd.Holds),
        [P.Descending] = new([Segments.Whole(P.Descending)], AnimationEnd.Holds),
        [P.StartAscending] = new([Segments.Whole(P.StartAscending)], AnimationEnd.Holds),
        [P.Ascending] = new([Segments.Whole(P.Ascending)], AnimationEnd.Holds),
        [P.FlamesAscending] = Animation<NorfairRioSegment>.Looping([Segments.Whole(P.FlamesAscending)]),
        [P.FlamesDescending] = Animation<NorfairRioSegment>.Looping([Segments.Whole(P.FlamesDescending)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NorfairRioSegment> StartingWith(NorfairRioSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairRioCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NorfairRioCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairRioCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NorfairRioCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairRioCue> Cue(NorfairRioCue cue) => new AnimationBeat<EnemyFrameId, NorfairRioCue>.Event(cue);
}
