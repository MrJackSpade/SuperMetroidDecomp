using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using K = SuperMetroid.Core.Areas.Tourian.TourianStatueProjectileCueKind;
using P = SuperMetroid.Core.Areas.Tourian.TourianStatueProjectileSegment;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian statue projectile animations (#641).</summary>
internal static class TourianStatueProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static HidingUnpausedAnimationSegments<TourianStatueProjectileSegment, EnemyProjectileFrameId, TourianStatueProjectileCue> Segments { get; } = new(
        new Dictionary<TourianStatueProjectileSegment, IUnpausedHidingAnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>[]>
    {
        [P.Splash] =
        [
            Show(EnemyProjectileFrameId.TourianStatueSplashLow, 8),
            Show(EnemyProjectileFrameId.TourianStatueSplashRising, 8),
            Show(EnemyProjectileFrameId.TourianStatueSplashScattering, 8),
            Show(EnemyProjectileFrameId.TourianStatueSplashFading, 8),
            Cue(new TourianStatueProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeGlow] =
        [
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowRingFaint, 8),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowRing, 8),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowRingCrossed, 8),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowStreak, 7),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowCrossRinged, 7),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowSparkle, 7),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowSparkleSmall, 6),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowStreakSmall, 6),
            Show(EnemyProjectileFrameId.TourianStatueEyeGlowDot, 5),
            Hide(48),
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
            Show(EnemyProjectileFrameId.TourianStatueParticleTurned0, 3),
            Show(EnemyProjectileFrameId.TourianStatueParticleTurned90, 3),
            Cue(new TourianStatueProjectileCue.Signal(K.SpawnTail)),
            Show(EnemyProjectileFrameId.TourianStatueParticleTurned180, 3),
            Show(EnemyProjectileFrameId.TourianStatueParticleTurned270, 3),
        ],
        [P.Tail] =
        [
            Show(EnemyProjectileFrameId.TourianStatueTailBright, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 8)),
            Show(EnemyProjectileFrameId.TourianStatueTailSparkles, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 4)),
            Show(EnemyProjectileFrameId.TourianStatueTailDim, 4),
            Cue(new TourianStatueProjectileCue.MoveDown(Pixels: 2)),
            Show(EnemyProjectileFrameId.TourianStatueTailFaint, 4),
            Cue(new TourianStatueProjectileCue.Signal(K.Delete)),
        ],
        [P.Soul] =
        [
            Show(EnemyProjectileFrameId.TourianStatueSoulMouthClosed, 8),
            Show(EnemyProjectileFrameId.TourianStatueSoulMouthOpen, 8),
        ],
        [P.BaseDecoration] =
        [
            Show(EnemyProjectileFrameId.TourianStatueBaseDecoration, 128),
            Cue(new TourianStatueProjectileCue.Signal(K.FollowReveal)),
            Hold(EnemyProjectileFrameId.TourianStatueBaseDecoration),
        ],
        [P.Ridley] =
        [
            Hold(EnemyProjectileFrameId.TourianStatueRidley),
        ],
        [P.Phantoon] =
        [
            Hold(EnemyProjectileFrameId.TourianStatuePhantoon),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at, each playing every beat of its segment
    /// (#2424). A particle plays its run again and again until it sinks out of the statues' screen, and a soul until it
    /// rises above the room's top, where its motion ends it.
    /// </summary>
    internal static IReadOnlyDictionary<TourianStatueProjectileSegment, Animation<TourianStatueProjectileSegment>> StartingAt { get; } = new Dictionary<TourianStatueProjectileSegment, Animation<TourianStatueProjectileSegment>>
    {
        [P.Splash] = new([Segments.Whole(P.Splash)], AnimationEnd.Holds),
        [P.EyeGlow] = new([Segments.Whole(P.EyeGlow)], AnimationEnd.Holds),
        [P.Particle] = Animation<TourianStatueProjectileSegment>.Looping([Segments.Whole(P.Particle)]),
        [P.Tail] = new([Segments.Whole(P.Tail)], AnimationEnd.Holds),
        [P.Soul] = Animation<TourianStatueProjectileSegment>.Looping([Segments.Whole(P.Soul)]),
        [P.BaseDecoration] = new([Segments.Whole(P.BaseDecoration)], AnimationEnd.Holds),
        [P.Ridley] = new([Segments.Whole(P.Ridley)], AnimationEnd.Holds),
        [P.Phantoon] = new([Segments.Whole(P.Phantoon)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<TourianStatueProjectileSegment> StartingWith(TourianStatueProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>.Frame(frame, duration);

    /// <summary>Shows nothing for <paramref name="duration"/> updates (#2558): the projectile is hidden until its next frame.</summary>
    private static AnimationHiddenBeat<EnemyProjectileFrameId, TourianStatueProjectileCue> Hide(int duration) =>
        new AnimationHiddenBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>(duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2068): a statue and the base's decoration rest on their picture.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>.Unpaused Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>.Unpaused Cue(TourianStatueProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, TourianStatueProjectileCue>.Event(cue);
}
