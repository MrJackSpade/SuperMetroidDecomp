using SuperMetroid.Core.Animation;
using C = SuperMetroid.Core.Bosses.Torizo.TorizoFamilyCue<SuperMetroid.Core.Bosses.Torizo.GoldenTorizoWeaponCueKind>;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileCueKind;
using O = SuperMetroid.Core.Bosses.Torizo.GoldenTorizoWeaponCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.GoldenTorizoWeaponSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The animations of Golden Torizo's caught super missile and its eye beam (#2554): each animation's steps, each frame the installed
/// <see cref="EnemyProjectileFrameId"/> it shows and each cue a shared <see cref="TorizoProjectileCue"/> or one of
/// <see cref="GoldenTorizoWeaponCueKind"/>, which only the behaviour keeping their Golden Torizo runs; the segments each animation plays whole, once or a number
/// of times (#2235); and whether it then loops or holds.
/// </summary>
internal static class GoldenTorizoWeaponAnimations
{
    /// <summary>Every animation's steps.</summary>
    internal static AnimationSegments<GoldenTorizoWeaponSegment, EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>> Segments { get; } = new(
        new Dictionary<GoldenTorizoWeaponSegment, AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>>[]>
    {
        [P.SuperMissileRight] = SuperMissileHeld(aim: O.AimSuperMissileTowardSamus),
        [P.SuperMissileSpinningRight] = SuperMissileSpinning(Right),
        [P.SuperMissileLeft] = SuperMissileHeld(aim: O.AimSuperMissileAwayFromSamus),
        [P.SuperMissileSpinningLeft] = SuperMissileSpinning(Left),
        [P.SuperMissileImpact] =
        [
            Cue(new TorizoProjectileCue.SetRadius(Radius: new(16, 16))),
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.PersistOnContactAndDrawInFront)),
            Cue(new TorizoProjectileCue.Signal(K.TurnHarmfulAndStopBlockingShots)),
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.SmallExplosion, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileFireballSmall, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileFireballLarge, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileFlamingRing, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileFireballHuge, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileWideRing, 5),
            Show(EnemyProjectileFrameId.GoldenTorizoSuperMissileWideRingFading, 5),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeamWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamSpark, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareBall, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingCored, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRing, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingFading, 4),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeamFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
        ],
        [P.EyeBeamFloorImpactWaiting] =
        [
            Show(EnemyProjectileFrameId.Blank, 8),
            Mark(O.FloorImpactWaited),
        ],
        [P.EyeBeamFloorFlames] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: RoomSound.ProjectileImpact, Priority: SoundRequestPriority.Normal)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameBurst, 4),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameDome, 5),
            Cue(new TorizoProjectileCue.Signal(K.TurnHarmful)),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameColumnShort, 6),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameColumnTall, 7),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameBurstAboveDome, 8),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlameEmbersAboveDome, 9),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.EyeBeam] =
        [
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingFading, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRing, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareRingCored, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamFlareBall, 1),
            Show(EnemyProjectileFrameId.GoldenTorizoEyeBeamSpark, 1),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment a spawn, a reaction or a cue starts (#2235): the segments it plays whole,
    /// once or a number of times, and whether it then loops or holds. An animation that reaches a still picture holds it
    /// for good (#2353).
    /// </summary>
    internal static IReadOnlyDictionary<GoldenTorizoWeaponSegment, Animation<GoldenTorizoWeaponSegment>> StartingAt { get; } = new Dictionary<GoldenTorizoWeaponSegment, Animation<GoldenTorizoWeaponSegment>>
    {
        [P.SuperMissileRight] = Animation<P>.LeadingInto([Segments.Whole(P.SuperMissileRight)], [Segments.Whole(P.SuperMissileSpinningRight)]),
        [P.SuperMissileLeft] = Animation<P>.LeadingInto([Segments.Whole(P.SuperMissileLeft)], [Segments.Whole(P.SuperMissileSpinningLeft)]),
        [P.SuperMissileImpact] = new([Segments.Whole(P.SuperMissileImpact)], AnimationEnd.Holds),
        [P.EyeBeamWallImpact] = new([Segments.Whole(P.EyeBeamWallImpact)], AnimationEnd.Holds),
        [P.EyeBeamFloorImpact] = Animation<P>.LeadingInto([Segments.Whole(P.EyeBeamFloorImpact)], [Segments.Whole(P.EyeBeamFloorImpactWaiting)]),
        [P.EyeBeamFloorFlames] = new([Segments.Whole(P.EyeBeamFloorFlames)], AnimationEnd.Holds),
        [P.EyeBeam] = Animation<P>.Looping([Segments.Whole(P.EyeBeam)]),
    };

    /// <summary>How <paramref name="animation"/> plays.</summary>
    internal static Animation<GoldenTorizoWeaponSegment> StartingWith(GoldenTorizoWeaponSegment animation) => StartingAt[animation];

    /// <summary>A cue's continuation into <paramref name="animation"/>, from its start.</summary>
    internal static AnimationContinuation<GoldenTorizoWeaponSegment> ContinueWith(GoldenTorizoWeaponSegment animation) =>
        new(StartingWith(animation), AnimationPlace.StartOfAnimation);

    /// <summary>A caught Super Missile held pointing down, then aimed as <paramref name="aim"/> says and thrown.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>>[] SuperMissileHeld(GoldenTorizoWeaponCueKind aim) =>
    [
        Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDown, 48),
        Own(aim),
        Own(O.ThrowSuperMissile),
    ];

    /// <summary>
    /// A thrown Super Missile spinning from pointing down toward <paramref name="side"/>: round through that side, up and the
    /// other side, back to pointing down.
    /// </summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>>[] SuperMissileSpinning(TorizoProjectileSide side) =>
    [
        Show(side.SuperMissileDownForward, 2),
        Show(side.SuperMissileForward, 2),
        Show(side.SuperMissileUpForward, 2),
        Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingUp, 2),
        Show(side.SuperMissileUpBack, 2),
        Show(side.SuperMissileBack, 2),
        Show(side.SuperMissileDownBack, 2),
        Show(EnemyProjectileFrameId.GoldenTorizoSuperMissilePointingDown, 2),
    ];

    /// <summary>Shows <paramref name="frame"/> for good: a still picture (#2235).</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>> Hold(EnemyProjectileFrameId frame) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>, which every Torizo projectile's behaviour runs alike.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>> Cue(TorizoProjectileCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>>.Event(new C.Shared(cue));

    /// <summary>Publishes <paramref name="kind"/>, which only the behaviour keeping their Golden Torizo runs.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>> Own(GoldenTorizoWeaponCueKind kind) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>>.Event(new C.Own(kind));

    /// <summary>Marks the moment <paramref name="kind"/> names (#2411), which the projectile's owner reacts to and which plays nothing.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>> Mark(GoldenTorizoWeaponCueKind kind) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoWeaponCueKind>>.Marker(new C.Own(kind));
}
