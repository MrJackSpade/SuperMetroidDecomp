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
    /// <summary>
    /// The wing poses of a swoop's start from level to swept up, each with where its flames sit and which way they burn: a
    /// descent sweeps the wings up through them, and an ascent sweeps them back down.
    /// </summary>
    private static readonly (EnemyFrameId Wings, int FlameOffset, GerutaFlames Flames)[] SwoopWings =
    [
        (EnemyFrameId.GerutaWingsLevel, 8, GerutaFlames.Ascending),
        (EnemyFrameId.GerutaWingsFlattening, 4, GerutaFlames.Ascending),
        (EnemyFrameId.GerutaWingsTilting, 0, GerutaFlames.Ascending),
        (EnemyFrameId.GerutaWingsLifting, -4, GerutaFlames.Descending),
        (EnemyFrameId.GerutaWingsRaising, -12, GerutaFlames.Descending),
        (EnemyFrameId.GerutaWingsSweptUp, -16, GerutaFlames.Descending),
    ];

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
        [P.StartDescending] = [.. SwoopWings.SelectMany(Swept), Cue(new GerutaCue.SwoopStartFinished())],
        [P.Descending] =
        [
            Cue(new GerutaCue.PlaceFlames(-12, GerutaFlames.Descending)),
            Hold(EnemyFrameId.GerutaDiving),
        ],
        [P.StartAscending] =
        [
            .. Enumerable.Reverse(SwoopWings).SelectMany(Swept),
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
    /// The animation that starts at each segment an owner can start or restart at. The dive and the climb place their
    /// flames and hold their one pose (#2512).
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

    /// <summary>One wing pose of a swoop's start, for one update, its flames placed as the pose places them.</summary>
    private static AnimationBeat<EnemyFrameId, GerutaCue>[] Swept((EnemyFrameId Wings, int FlameOffset, GerutaFlames Flames) pose) =>
        [Cue(new GerutaCue.PlaceFlames(pose.FlameOffset, pose.Flames)), Show(pose.Wings, 1)];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, GerutaCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, GerutaCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, GerutaCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, GerutaCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, GerutaCue> Cue(GerutaCue cue) => new AnimationBeat<EnemyFrameId, GerutaCue>.Event(cue);
}
