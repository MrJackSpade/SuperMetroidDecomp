using SuperMetroid.Core.Animation;
using C = SuperMetroid.Core.Bosses.Torizo.TorizoFamilyCue<SuperMetroid.Core.Bosses.Torizo.TorizoExplosionCueKind>;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileCueKind;
using O = SuperMetroid.Core.Bosses.Torizo.TorizoExplosionCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoExplosionSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The animations of the Torizos' explosions and the bursts of a sonic boom that has struck a wall, which return to where they burst (#2554): each animation's steps, each frame the installed
/// <see cref="EnemyProjectileFrameId"/> it shows and each cue a shared <see cref="TorizoProjectileCue"/> or one of
/// <see cref="TorizoExplosionCueKind"/>, which only the explosion behaviour runs; the segments each animation plays whole, once or a number
/// of times (#2235); and whether it then loops or holds.
/// </summary>
internal static class TorizoExplosionAnimations
{
    /// <summary>Every animation's steps.</summary>
    internal static AnimationSegments<TorizoExplosionSegment, EnemyProjectileFrameId, TorizoFamilyCue<TorizoExplosionCueKind>> Segments { get; } = new(
        new Dictionary<TorizoExplosionSegment, AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoExplosionCueKind>>[]>
    {
        [P.LowHealthExplosion] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.StopHarmingSamusAndDrawInFront)),
        ],
        [P.LowHealthExplosionBurst] =
        [
            Own(O.ReturnToOrigin),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 4), Center: new(0, 0)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoExplosionSpark, 2),
            Show(EnemyProjectileFrameId.TorizoExplosionFlareBall, 2),
            Show(EnemyProjectileFrameId.TorizoExplosionFlareRingCored, 3),
            Show(EnemyProjectileFrameId.TorizoExplosionFlareRing, 3),
            Show(EnemyProjectileFrameId.TorizoExplosionFlareRingFading, 2),
        ],
        [P.DeathExplosion] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.StopHarmingSamusAndDrawInFront)),
            Own(O.MayTurnToSmoke),
        ],
        [P.DeathExplosionBurst] =
        [
            Own(O.ReturnToOrigin),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 5), YRange: new(0, 6), Center: new(0, 16)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoDeathExplosionSpark, 4),
            Show(EnemyProjectileFrameId.TorizoExplosionFireballSmall, 6),
            Show(EnemyProjectileFrameId.TorizoExplosionFireballLarge, 5),
            Show(EnemyProjectileFrameId.TorizoExplosionFlamingRing, 5),
            Show(EnemyProjectileFrameId.TorizoExplosionStarburst, 5),
            Show(EnemyProjectileFrameId.TorizoExplosionEmbers, 6),
        ],
        [P.DeathSmoke] =
        [
            Own(O.ReturnToOrigin),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 5), YRange: new(0, 6), Center: new(0, 4)))),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoExplosionFireCloud, 8),
            Show(EnemyProjectileFrameId.TorizoExplosionFireCloudSpreading, 8),
            Show(EnemyProjectileFrameId.TorizoExplosionFireCloudBreaking, 8),
            Show(EnemyProjectileFrameId.TorizoExplosionFireCloudEmbers, 8),
        ],
        [P.SonicBoomWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.StopBlockingShots)),
            Cue(new TorizoProjectileCue.Signal(K.StopHarmingSamus)),
            Cue(new TorizoProjectileCue.Signal(K.DrawInFront)),
        ],
        [P.SonicBoomWallImpactBurst] =
        [
            Own(O.ReturnToOrigin),
            Cue(new TorizoProjectileCue.Scatter(Spread: new(XRange: new(0, 4), YRange: new(0, 5), Center: new(0, 0)))),
            Show(EnemyProjectileFrameId.TorizoSonicBoomSpark, 2),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFlareBall, 2),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFlareRingCored, 3),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFlareRing, 3),
            Show(EnemyProjectileFrameId.TorizoSonicBoomFlareRingFading, 2),
        ],
        [P.Deletion] = [Cue(new TorizoProjectileCue.Signal(K.Delete))],
    });

    /// <summary>
    /// The animation that starts at each segment a spawn, a reaction or a cue starts (#2235): the segments it plays whole,
    /// once or a number of times, and whether it then loops or holds. An animation that reaches a still picture holds it
    /// for good (#2353).
    /// </summary>
    internal static IReadOnlyDictionary<TorizoExplosionSegment, Animation<TorizoExplosionSegment>> StartingAt { get; } = new Dictionary<TorizoExplosionSegment, Animation<TorizoExplosionSegment>>
    {
        [P.LowHealthExplosion] = new([Segments.Whole(P.LowHealthExplosion), Segments.Repeated(P.LowHealthExplosionBurst, 3), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [P.DeathExplosion] = new([Segments.Whole(P.DeathExplosion), Segments.Repeated(P.DeathExplosionBurst, 2), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [P.DeathSmoke] = new([Segments.Repeated(P.DeathSmoke, 2), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [P.SonicBoomWallImpact] = new([Segments.Whole(P.SonicBoomWallImpact), Segments.Repeated(P.SonicBoomWallImpactBurst, 5), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
    };

    /// <summary>How <paramref name="animation"/> plays.</summary>
    internal static Animation<TorizoExplosionSegment> StartingWith(TorizoExplosionSegment animation) => StartingAt[animation];

    /// <summary>A cue's continuation into <paramref name="animation"/>, from its start.</summary>
    internal static AnimationContinuation<TorizoExplosionSegment> ContinueWith(TorizoExplosionSegment animation) =>
        new(StartingWith(animation), AnimationPlace.StartOfAnimation);

    /// <summary>Shows <paramref name="frame"/> for good: a still picture (#2235).</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoExplosionCueKind>> Hold(EnemyProjectileFrameId frame) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoExplosionCueKind>>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoExplosionCueKind>> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoExplosionCueKind>>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>, which every Torizo projectile's behaviour runs alike.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoExplosionCueKind>> Cue(TorizoProjectileCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoExplosionCueKind>>.Event(new C.Shared(cue));

    /// <summary>Publishes <paramref name="kind"/>, which only the explosion behaviour runs.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoExplosionCueKind>> Own(TorizoExplosionCueKind kind) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoExplosionCueKind>>.Event(new C.Own(kind));
}
