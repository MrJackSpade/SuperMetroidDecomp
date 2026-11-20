using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.EffectActors;
using K = SuperMetroid.Core.Enemies.Deaths.EnemyDeathAndDropCueKind;
using P = SuperMetroid.Core.Enemies.Deaths.EnemyDeathAndDropSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The generic enemy-death explosions, the drops they become and the respawn pause (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyDeathAndDropCue"/> with the values it acts on.
/// </summary>
internal static class EnemyDeathAndDropAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static HidingUnpausedAnimationSegments<EnemyDeathAndDropSegment, EnemyProjectileFrameId, EnemyDeathAndDropCue> Segments { get; } = new(
        new Dictionary<EnemyDeathAndDropSegment, IUnpausedHidingAnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>[]>
    {
        [P.RespawnPause] =
        [
            Hide(64),
            Cue(new EnemyDeathAndDropCue.Signal(K.RespawnEnemy)),
            Cue(new EnemyDeathAndDropCue.Signal(K.Delete)),
        ],
        [P.BigExplosion] =
        [
            Cue(new EnemyDeathAndDropCue.SpawnEffectActor(RoomEffectActorKind.SmallExplosion, EnemyDeathExplosionDefinitions.NarrowSpriteSpread)),
            Cue(new EnemyDeathAndDropCue.SpawnEffectActor(RoomEffectActorKind.Smoke, EnemyDeathExplosionDefinitions.NarrowSpriteSpread)),
            Hide(8),
            Cue(new EnemyDeathAndDropCue.Signal(K.PlaySmallExplosionSound)),
        ],
        [P.BigExplosionEnd] =
        [
            Cue(new EnemyDeathAndDropCue.Signal(K.BecomePickup)),
        ],
        [P.FakeKraidExplosion] =
        [
            Cue(new EnemyDeathAndDropCue.SpawnEffectActor(RoomEffectActorKind.SmallExplosion, EnemyDeathExplosionDefinitions.WideSpriteSpread)),
            Cue(new EnemyDeathAndDropCue.SpawnEffectActor(RoomEffectActorKind.Smoke, EnemyDeathExplosionDefinitions.WideSpriteSpread)),
            Cue(new EnemyDeathAndDropCue.SpawnEffectActor(RoomEffectActorKind.BigDustCloud, EnemyDeathExplosionDefinitions.WideSpriteSpread)),
            Hide(8),
            Cue(new EnemyDeathAndDropCue.Signal(K.PlaySmallExplosionSound)),
        ],
        [P.FakeKraidExplosionEnd] =
        [
            Cue(new EnemyDeathAndDropCue.Signal(K.BecomePickup)),
        ],
        [P.NormalExplosion] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathFireballSmall, 5),
            Show(EnemyProjectileFrameId.EnemyDeathFireballLarge, 5),
            Show(EnemyProjectileFrameId.EnemyDeathFlamingRing, 5),
            Cue(new EnemyDeathAndDropCue.Signal(K.PlayEnemyKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFireballHuge, 5),
            Show(EnemyProjectileFrameId.EnemyDeathWideRing, 5),
            Show(EnemyProjectileFrameId.EnemyDeathWideRingFading, 5),
            Cue(new EnemyDeathAndDropCue.Signal(K.BecomePickup)),
        ],
        [P.SmallExplosion] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathSpark, 4),
            Show(EnemyProjectileFrameId.EnemyDeathFireballSmall, 6),
            Show(EnemyProjectileFrameId.EnemyDeathFireballLarge, 5),
            Cue(new EnemyDeathAndDropCue.Signal(K.PlayEnemyKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFlamingRing, 5),
            Show(EnemyProjectileFrameId.EnemyDeathStarburst, 5),
            Show(EnemyProjectileFrameId.EnemyDeathEmbers, 6),
            Cue(new EnemyDeathAndDropCue.Signal(K.BecomePickup)),
        ],
        [P.SmallEnergy] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupEnergySmallDark, 8),
            Show(EnemyProjectileFrameId.EnemyPickupEnergySmallRinged, 8),
            Show(EnemyProjectileFrameId.EnemyPickupEnergySmallBright, 8),
            Show(EnemyProjectileFrameId.EnemyPickupEnergyScattered, 8),
        ],
        [P.BigEnergy] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupEnergyBigBright, 8),
            Show(EnemyProjectileFrameId.EnemyPickupEnergyBigRinged, 8),
            Show(EnemyProjectileFrameId.EnemyPickupEnergyBigDark, 8),
            Show(EnemyProjectileFrameId.EnemyPickupEnergyScattered, 8),
        ],
        [P.Missiles] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupMissile, 5),
            Show(EnemyProjectileFrameId.EnemyPickupMissileFlashing, 5),
        ],
        [P.SuperMissiles] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupSuperMissile, 5),
            Show(EnemyProjectileFrameId.EnemyPickupSuperMissileFlashing, 5),
        ],
        [P.PowerBombs] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupPowerBombBright, 5),
            Show(EnemyProjectileFrameId.EnemyPickupPowerBombFading, 5),
            Show(EnemyProjectileFrameId.EnemyPickupPowerBombDim, 5),
            Show(EnemyProjectileFrameId.EnemyPickupPowerBombFlashing, 5),
        ],
        [P.KilledBySamusContact] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathFireballLarge, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFireballCluster, 2),
            Show(EnemyProjectileFrameId.EnemyDeathStarburstCluster, 2),
            Show(EnemyProjectileFrameId.EnemyDeathEmbersCluster, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersFlyingApart, 2),
            Cue(new EnemyDeathAndDropCue.Signal(K.PlayContactKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersCooling, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersDimming, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshDrifting, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshSpreading, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshSinking, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshFalling, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshSettling, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshDispersing, 2),
            Cue(new EnemyDeathAndDropCue.Signal(K.BecomePickup)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EnemyDeathAndDropSegment, Animation<EnemyDeathAndDropSegment>> StartingAt { get; } = new Dictionary<EnemyDeathAndDropSegment, Animation<EnemyDeathAndDropSegment>>
    {
        [P.RespawnPause] = new([Segments.Whole(P.RespawnPause)], AnimationEnd.Holds),
        [P.NormalExplosion] = new([Segments.Whole(P.NormalExplosion)], AnimationEnd.Holds),
        [P.SmallExplosion] = new([Segments.Whole(P.SmallExplosion)], AnimationEnd.Holds),
        [P.SmallEnergy] = Animation<EnemyDeathAndDropSegment>.Looping([Segments.Whole(P.SmallEnergy)]),
        [P.BigEnergy] = Animation<EnemyDeathAndDropSegment>.Looping([Segments.Whole(P.BigEnergy)]),
        [P.Missiles] = Animation<EnemyDeathAndDropSegment>.Looping([Segments.Whole(P.Missiles)]),
        [P.SuperMissiles] = Animation<EnemyDeathAndDropSegment>.Looping([Segments.Whole(P.SuperMissiles)]),
        [P.PowerBombs] = Animation<EnemyDeathAndDropSegment>.Looping([Segments.Whole(P.PowerBombs)]),
        [P.KilledBySamusContact] = new([Segments.Whole(P.KilledBySamusContact)], AnimationEnd.Holds),
    };

    /// <summary>How many bursts the big explosion makes before it becomes the drop.</summary>
    private const int BigExplosionBursts = 5;

    /// <summary>How many bursts Fake Kraid's explosion makes before it becomes the drop.</summary>
    private const int FakeKraidExplosionBursts = 16;

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EnemyDeathAndDropSegment> StartingWith(EnemyDeathAndDropSegment segment) => StartingAt[segment];

    /// <summary>
    /// The animation each death explosion starts (#2061): the big explosion's bursts and Fake Kraid's (<see cref="BigExplosionBursts"/>, <see cref="FakeKraidExplosionBursts"/>), each
    /// scattering its sprites, before it becomes the drop; the other explosions' animations start at their segments.
    /// </summary>
    internal static IdentityTable<EnemyDeathExplosion, Animation<EnemyDeathAndDropSegment>> Explosions { get; } = new(new Dictionary<EnemyDeathExplosion, Animation<EnemyDeathAndDropSegment>>
        {
            [EnemyDeathExplosion.Small] = StartingAt[P.SmallExplosion],
            [EnemyDeathExplosion.SamusContact] = StartingAt[P.KilledBySamusContact],
            [EnemyDeathExplosion.Normal] = StartingAt[P.NormalExplosion],
            [EnemyDeathExplosion.FakeKraid] = new([Segments.Repeated(P.FakeKraidExplosion, FakeKraidExplosionBursts), Segments.Whole(P.FakeKraidExplosionEnd)], AnimationEnd.Holds),
            [EnemyDeathExplosion.Big] = new([Segments.Repeated(P.BigExplosion, BigExplosionBursts), Segments.Whole(P.BigExplosionEnd)], AnimationEnd.Holds),
        });

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>.Frame(frame, duration);

    /// <summary>Shows nothing for <paramref name="duration"/> updates (#2558): the projectile is hidden until its next frame.</summary>
    private static AnimationHiddenBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue> Hide(int duration) =>
        new AnimationHiddenBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>(duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>.Unpaused Cue(EnemyDeathAndDropCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>.Event(cue);
}
