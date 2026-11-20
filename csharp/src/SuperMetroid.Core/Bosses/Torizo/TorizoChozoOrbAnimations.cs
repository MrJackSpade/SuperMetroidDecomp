using SuperMetroid.Core.Animation;
using C = SuperMetroid.Core.Bosses.Torizo.TorizoFamilyCue<SuperMetroid.Core.Bosses.Torizo.TorizoChozoOrbCueKind>;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileCueKind;
using O = SuperMetroid.Core.Bosses.Torizo.TorizoChozoOrbCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoChozoOrbSegment;
using SuperMetroid.Core.Enemies.Projectiles;
using static SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSide;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The animations of the Torizos' Chozo orbs (#2554): each animation's steps, each frame the installed
/// <see cref="EnemyProjectileFrameId"/> it shows and each cue a shared <see cref="TorizoProjectileCue"/> or one of
/// <see cref="TorizoChozoOrbCueKind"/>, which only the orb behaviour runs; the segments each animation plays whole, once or a number
/// of times (#2235); and whether it then loops or holds.
/// </summary>
internal static class TorizoChozoOrbAnimations
{
    /// <summary>Every animation's steps.</summary>
    internal static AnimationSegments<TorizoChozoOrbSegment, EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>> Segments { get; } = new(
        new Dictionary<TorizoChozoOrbSegment, AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>>[]>
    {
        [P.ChozoOrbMovingLeft] = ChozoOrbMoving(Left),
        [P.ChozoOrbMovingRight] = ChozoOrbMoving(Right),
        [P.ChozoOrbWallImpact] =
        [
            .. ChozoOrbBursting,
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.ChozoOrbFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.TurnHarmful)),
            Cue(new TorizoProjectileCue.Signal(K.PersistOnContactAndDrawInFront)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: RoomSound.ProjectileImpact, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlameBurst, 4),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlameDome, 5),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlameColumnShort, 6),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlameColumnTall, 7),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlameBurstAboveDome, 8),
            Show(EnemyProjectileFrameId.TorizoChozoOrbFlameEmbersAboveDome, 9),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.ChozoOrbShot] =
        [
            .. ChozoOrbBursting,
            Own(O.SpawnOrbDrops),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment a spawn, a reaction or a cue starts (#2235): the segments it plays whole,
    /// once or a number of times, and whether it then loops or holds. An animation that reaches a still picture holds it
    /// for good (#2353).
    /// </summary>
    internal static IReadOnlyDictionary<TorizoChozoOrbSegment, Animation<TorizoChozoOrbSegment>> StartingAt { get; } = new Dictionary<TorizoChozoOrbSegment, Animation<TorizoChozoOrbSegment>>
    {
        [P.ChozoOrbMovingLeft] = new([Segments.Whole(P.ChozoOrbMovingLeft)], AnimationEnd.Holds),
        [P.ChozoOrbMovingRight] = new([Segments.Whole(P.ChozoOrbMovingRight)], AnimationEnd.Holds),
        [P.ChozoOrbWallImpact] = new([Segments.Whole(P.ChozoOrbWallImpact)], AnimationEnd.Holds),
        [P.ChozoOrbFloorImpact] = new([Segments.Whole(P.ChozoOrbFloorImpact)], AnimationEnd.Holds),
        [P.ChozoOrbShot] = new([Segments.Whole(P.ChozoOrbShot)], AnimationEnd.Holds),
    };

    /// <summary>How <paramref name="animation"/> plays.</summary>
    internal static Animation<TorizoChozoOrbSegment> StartingWith(TorizoChozoOrbSegment animation) => StartingAt[animation];

    /// <summary>
    /// A Chozo orb bursting (#2411), on a wall or shot: it stops, survives touching Samus in front of her, and plays its five
    /// poses; a shot orb's burst then drops its pickup.
    /// </summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>>[] ChozoOrbBursting =>
    [
        Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
        Cue(new TorizoProjectileCue.Signal(K.PersistOnContactAndDrawInFront)),
        Show(EnemyProjectileFrameId.TorizoChozoOrbSpark, 4),
        Show(EnemyProjectileFrameId.TorizoChozoOrbFlareBall, 4),
        Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingCored, 4),
        Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRing, 4),
        Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingFading, 4),
    ];

    /// <summary>A Chozo orb flying toward <paramref name="side"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>>[] ChozoOrbMoving(TorizoProjectileSide side) =>
        [Hold(side.ChozoOrb)];

    /// <summary>Shows <paramref name="frame"/> for good: a still picture (#2235).</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>> Hold(EnemyProjectileFrameId frame) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>, which every Torizo projectile's behaviour runs alike.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>> Cue(TorizoProjectileCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>>.Event(new C.Shared(cue));

    /// <summary>Publishes <paramref name="kind"/>, which only the orb behaviour runs.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>> Own(TorizoChozoOrbCueKind kind) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoChozoOrbCueKind>>.Event(new C.Own(kind));
}
