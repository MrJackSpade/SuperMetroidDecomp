using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.Chozo.ChozoTourianDustProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Wrecked Ship Chozo's footstep and explosion dust and the Tourian statue's descent dust (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class ChozoTourianDustAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ChozoTourianDustProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<ChozoTourianDustProgram, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Footsteps] =
        [
            Cue(new EnemyProjectileCommonCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 4), Center: new(0, 3)))),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame00, 2),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame01, 2),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame02, 2),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame03, 2),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.SpikeClearingExplosions] =
        [
            Cue(new EnemyProjectileCommonCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 4), Center: new(0, 3)))),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame04, 5),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame05, 5),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame06, 5),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame07, 5),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame08, 5),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame09, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.TourianDescentDust] =
        [
        ],
        [P.TourianDescentDustLoop] =
        [
            Cue(new EnemyProjectileCommonCue.Command(CK.ReturnToOrigin)),
            Cue(new EnemyProjectileCommonCue.Scatter(Spread: new(XRange: new(0, 6), YRange: new(0, 2), Center: new(0, 0)))),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame10, 2),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame11, 2),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame12, 2),
            Show(EnemyProjectileFrameId.ChozoTourianDustFrame13, 2),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ChozoTourianDustProgram, Animation<ChozoTourianDustProgram>> StartingAt { get; } = new Dictionary<ChozoTourianDustProgram, Animation<ChozoTourianDustProgram>>
    {
        [P.Footsteps] = new([new(P.Footsteps, 0, 6, 1)], AnimationEnd.Holds),
        [P.SpikeClearingExplosions] = new([new(P.SpikeClearingExplosions, 0, 8, 1)], AnimationEnd.Holds),
        [P.TourianDescentDust] = new([new(P.TourianDescentDustLoop, 0, 6, 64), new(P.TourianDescentDustLoop, 6, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ChozoTourianDustProgram> StartingWith(ChozoTourianDustProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
