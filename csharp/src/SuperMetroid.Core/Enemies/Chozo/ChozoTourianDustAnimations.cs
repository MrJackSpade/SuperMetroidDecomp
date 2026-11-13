using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.Chozo.ChozoTourianDustSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Wrecked Ship Chozo's footstep and explosion dust and the Tourian statue's descent dust (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class ChozoTourianDustAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ChozoTourianDustSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<ChozoTourianDustSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Footsteps] =
        [
            Cue(new EnemyProjectileCommonCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 4), Center: new(0, 3)))),
            Show(EnemyProjectileFrameId.ChozoTourianDustFootstepsFirst, 2),
            Show(EnemyProjectileFrameId.ChozoTourianDustFootstepsSecond, 2),
            Show(EnemyProjectileFrameId.ChozoTourianDustFootstepsThird, 2),
            Show(EnemyProjectileFrameId.ChozoTourianDustFootstepsFourth, 2),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.SpikeClearingExplosions] =
        [
            Cue(new EnemyProjectileCommonCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 4), Center: new(0, 3)))),
            Show(EnemyProjectileFrameId.ChozoTourianDustSpikeClearingExplosionsFirst, 5),
            Show(EnemyProjectileFrameId.ChozoTourianDustSpikeClearingExplosionsSecond, 5),
            Show(EnemyProjectileFrameId.ChozoTourianDustSpikeClearingExplosionsThird, 5),
            Show(EnemyProjectileFrameId.ChozoTourianDustSpikeClearingExplosionsFourth, 5),
            Show(EnemyProjectileFrameId.ChozoTourianDustSpikeClearingExplosionsFifth, 5),
            Show(EnemyProjectileFrameId.ChozoTourianDustSpikeClearingExplosionsSixth, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.TourianDescentDust] =
        [
        ],
        [P.TourianDescentDustLoop] =
        [
            Cue(new EnemyProjectileCommonCue.Signal(CK.ReturnToOrigin)),
            Cue(new EnemyProjectileCommonCue.Scatter(Spread: new(XRange: new(0, 6), YRange: new(0, 2), Center: new(0, 0)))),
            Show(EnemyProjectileFrameId.ChozoTourianDustTourianDescentDustFirst, 2),
            Show(EnemyProjectileFrameId.ChozoTourianDustTourianDescentDustSecond, 2),
            Show(EnemyProjectileFrameId.ChozoTourianDustTourianDescentDustThird, 2),
            Show(EnemyProjectileFrameId.ChozoTourianDustTourianDescentDustFourth, 2),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ChozoTourianDustSegment, Animation<ChozoTourianDustSegment>> StartingAt { get; } = new Dictionary<ChozoTourianDustSegment, Animation<ChozoTourianDustSegment>>
    {
        [P.Footsteps] = new([new(P.Footsteps, 0, 6, 1)], AnimationEnd.Holds),
        [P.SpikeClearingExplosions] = new([new(P.SpikeClearingExplosions, 0, 8, 1)], AnimationEnd.Holds),
        [P.TourianDescentDust] = new([new(P.TourianDescentDustLoop, 0, 6, 64), new(P.TourianDescentDustLoop, 6, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ChozoTourianDustSegment> StartingWith(ChozoTourianDustSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
