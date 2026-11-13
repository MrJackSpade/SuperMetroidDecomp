using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using K = SuperMetroid.Core.Areas.Tourian.TourianStatueProjectileCueKind;
using P = SuperMetroid.Core.Areas.Tourian.TourianStatueProjectileSegment;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian statue projectile animations (#641).</summary>
internal static class TourianStatueProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<TourianStatueProjectileSegment, EnemyProjectileFrameId, TourianStatueProjectileCue> Segments { get; } = new(
        new Dictionary<TourianStatueProjectileSegment, AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>[]>
    {
        [P.Delete] =
        [
            Cue(new TourianStatueProjectileCue.Signal(K.Delete)),
        ],
        [P.Splash] =
        [
            Show(EnemyProjectileFrameId.TourianStatueSplashFirst, 8),
            Show(EnemyProjectileFrameId.TourianStatueSplashSecond, 8),
            Show(EnemyProjectileFrameId.TourianStatueSplashThird, 8),
            Show(EnemyProjectileFrameId.TourianStatueSplashFourth, 8),
            Cue(new TourianStatueProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeGlow] =
        [
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowFirst, 8),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowSecond, 8),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowThird, 8),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowFourth, 7),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowFifth, 7),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowSixth, 7),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowSeventh, 6),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowEighth, 6),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowNinth, 5),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowTenth, 48),
            Cue(new TourianStatueProjectileCue.Signal(K.PlayReleaseSound)),
            Cue(new TourianStatueProjectileCue.Signal(K.Earthquake)),
            Cue(new TourianStatueProjectileCue.Signal(K.SpawnParticle)),
            Cue(new TourianStatueProjectileCue.Signal(K.SpawnParticle)),
            Cue(new TourianStatueProjectileCue.Signal(K.SpawnParticle)),
            Cue(new TourianStatueProjectileCue.Signal(K.SpawnParticle)),
            Cue(new TourianStatueProjectileCue.Signal(K.Delete)),
        ],
        [P.Particle] =
        [
            Show(EnemyProjectileFrameId.TourianStatueParticleFirst, 3),
            Show(EnemyProjectileFrameId.TourianStatueParticleSecond, 3),
            Cue(new TourianStatueProjectileCue.Signal(K.SpawnTail)),
            Show(EnemyProjectileFrameId.TourianStatueParticleThird, 3),
            Show(EnemyProjectileFrameId.TourianStatueParticleFourth, 3),
        ],
        [P.Tail] =
        [
            Show(EnemyProjectileFrameId.TourianStatueTailFirst, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 8)),
            Show(EnemyProjectileFrameId.TourianStatueTailSecond, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 4)),
            Show(EnemyProjectileFrameId.TourianStatueTailThird, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 2)),
            Show(EnemyProjectileFrameId.TourianStatueTailFourth, 4),
            Cue(new TourianStatueProjectileCue.Signal(K.Delete)),
        ],
        [P.Soul] =
        [
            Show(EnemyProjectileFrameId.TourianStatueSoulFirst, 8),
            Show(EnemyProjectileFrameId.TourianStatueSoulSecond, 8),
        ],
        [P.BaseDecoration] =
        [
            Show(EnemyProjectileFrameId.TourianStatueBaseDecoration, 128),
            Cue(new TourianStatueProjectileCue.StartMoving(Movement: EnemyProjectileFrameBehavior.TourianStatueRevealFollow)),
        ],
        [P.BaseDecorationLoop] =
        [
            Show(EnemyProjectileFrameId.TourianStatueBaseDecorationLoop, 1911),
        ],
        [P.Ridley] =
        [
            Show(EnemyProjectileFrameId.TourianStatueRidley, 1911),
        ],
        [P.Phantoon] =
        [
            Show(EnemyProjectileFrameId.TourianStatuePhantoon, 1911),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. A particle starts its counted animation
    /// directly, with the counter its spawn cleared: its first play counts that zero down, wrapping it, so the animation
    /// replays the longest count more times before it holds.
    /// </summary>
    internal static IReadOnlyDictionary<TourianStatueProjectileSegment, Animation<TourianStatueProjectileSegment>> StartingAt { get; } = new Dictionary<TourianStatueProjectileSegment, Animation<TourianStatueProjectileSegment>>
    {
        [P.Delete] = new([new(P.Delete, 0, 1, 1)], AnimationEnd.Holds),
        [P.Splash] = new([new(P.Splash, 0, 5, 1)], AnimationEnd.Holds),
        [P.EyeGlow] = new([new(P.EyeGlow, 0, 17, 1)], AnimationEnd.Holds),
        [P.Particle] = new([new(P.Particle, 0, 5, 1), new(P.Particle, 0, 5, WrappingCountdown.LongestStart)], AnimationEnd.Holds),
        [P.Tail] = new([new(P.Tail, 0, 8, 1)], AnimationEnd.Holds),
        [P.Soul] = new([new(P.Soul, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BaseDecoration] = new([new(P.BaseDecoration, 0, 2, 1), new(P.BaseDecorationLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.BaseDecorationLoop] = new([new(P.BaseDecorationLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Ridley] = new([new(P.Ridley, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Phantoon] = new([new(P.Phantoon, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<TourianStatueProjectileSegment> StartingWith(TourianStatueProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue> Cue(TourianStatueProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>.Event(cue);
}
