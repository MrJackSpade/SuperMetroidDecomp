using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.EffectActors;
using K = SuperMetroid.Core.Enemies.Deaths.EnemyDeathProjectileCueKind;
using P = SuperMetroid.Core.Enemies.Deaths.EnemyDeathProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The generic enemy-death explosions, the drops they become and the respawn tail (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyDeathProjectileCue"/> with the values it acts on.
/// </summary>
internal static class EnemyDeathProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EnemyDeathProjectileSegment, EnemyProjectileFrameId, EnemyDeathProjectileCue> Segments { get; } = new(
        new Dictionary<EnemyDeathProjectileSegment, AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue>[]>
    {
        [P.RespawnTail] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathBlank, 64),
            Cue(new EnemyDeathProjectileCue.Signal(K.HandleRespawningEnemy)),
            Cue(new EnemyDeathProjectileCue.Signal(K.Delete)),
        ],
        [P.BigExplosion] =
        [
            Cue(new EnemyDeathProjectileCue.SpawnEffectActor(RoomEffectActorKind.SmallExplosion, EnemyDeathExplosionDefinitions.NarrowSpriteSpread)),
            Cue(new EnemyDeathProjectileCue.SpawnEffectActor(RoomEffectActorKind.Smoke, EnemyDeathExplosionDefinitions.NarrowSpriteSpread)),
            Show(EnemyProjectileFrameId.EnemyDeathBlankTwin, 8),
            Cue(new EnemyDeathProjectileCue.Signal(K.PlaySmallExplosionSound)),
            Cue(new EnemyDeathProjectileCue.Signal(K.BecomePickup)),
        ],
        [P.MiniKraidExplosion] =
        [
            Cue(new EnemyDeathProjectileCue.SpawnEffectActor(RoomEffectActorKind.SmallExplosion, EnemyDeathExplosionDefinitions.WideSpriteSpread)),
            Cue(new EnemyDeathProjectileCue.SpawnEffectActor(RoomEffectActorKind.Smoke, EnemyDeathExplosionDefinitions.WideSpriteSpread)),
            Cue(new EnemyDeathProjectileCue.SpawnEffectActor(RoomEffectActorKind.BigDustCloud, EnemyDeathExplosionDefinitions.WideSpriteSpread)),
            Show(EnemyProjectileFrameId.EnemyDeathBlankTwinB, 8),
            Cue(new EnemyDeathProjectileCue.Signal(K.PlaySmallExplosionSound)),
            Cue(new EnemyDeathProjectileCue.Signal(K.BecomePickup)),
        ],
        [P.NormalExplosion] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathFireballSmall, 5),
            Show(EnemyProjectileFrameId.EnemyDeathFireballLarge, 5),
            Show(EnemyProjectileFrameId.EnemyDeathFlamingRing, 5),
            Cue(new EnemyDeathProjectileCue.Signal(K.PlayEnemyKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFireballHuge, 5),
            Show(EnemyProjectileFrameId.EnemyDeathWideRing, 5),
            Show(EnemyProjectileFrameId.EnemyDeathWideRingFading, 5),
            Cue(new EnemyDeathProjectileCue.Signal(K.BecomePickup)),
        ],
        [P.SmallExplosion] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathSpark, 4),
            Show(EnemyProjectileFrameId.EnemyDeathFireballSmallTwin, 6),
            Show(EnemyProjectileFrameId.EnemyDeathFireballLargeTwin, 5),
            Cue(new EnemyDeathProjectileCue.Signal(K.PlayEnemyKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFlamingRingTwin, 5),
            Show(EnemyProjectileFrameId.EnemyDeathStarburst, 5),
            Show(EnemyProjectileFrameId.EnemyDeathEmbers, 6),
            Cue(new EnemyDeathProjectileCue.Signal(K.BecomePickup)),
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
            Show(EnemyProjectileFrameId.EnemyPickupEnergyScatteredTwin, 8),
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
            Show(EnemyProjectileFrameId.EnemyDeathFireballLargeTwinB, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFireballCluster, 2),
            Show(EnemyProjectileFrameId.EnemyDeathStarburstCluster, 2),
            Show(EnemyProjectileFrameId.EnemyDeathEmbersCluster, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourFireballsAmidEmbers, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourFireballsAmidAsh, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersApart62, 2),
            Cue(new EnemyDeathProjectileCue.Signal(K.PlayContactKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersApart70, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersApart76, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersApart80, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshApart86, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshApart94, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshApart98, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshApart106, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshApart118, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshApart130, 2),
            Cue(new EnemyDeathProjectileCue.Signal(K.BecomePickup)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EnemyDeathProjectileSegment, Animation<EnemyDeathProjectileSegment>> StartingAt { get; } = new Dictionary<EnemyDeathProjectileSegment, Animation<EnemyDeathProjectileSegment>>
    {
        [P.RespawnTail] = new([new(P.RespawnTail, 0, 3, 1)], AnimationEnd.Holds),
        [P.NormalExplosion] = new([new(P.NormalExplosion, 0, 8, 1)], AnimationEnd.Holds),
        [P.SmallExplosion] = new([new(P.SmallExplosion, 0, 8, 1)], AnimationEnd.Holds),
        [P.SmallEnergy] = new([new(P.SmallEnergy, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BigEnergy] = new([new(P.BigEnergy, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Missiles] = new([new(P.Missiles, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissiles] = new([new(P.SuperMissiles, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PowerBombs] = new([new(P.PowerBombs, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.KilledBySamusContact] = new([new(P.KilledBySamusContact, 0, 18, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EnemyDeathProjectileSegment> StartingWith(EnemyDeathProjectileSegment segment) => StartingAt[segment];

    /// <summary>
    /// The animation each death explosion starts (#2061): the big explosion's five bursts and Mini-Kraid's sixteen, each
    /// scattering its sprites, before it becomes the drop; the other explosions' animations start at their segments.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyDeathExplosion, Animation<EnemyDeathProjectileSegment>> Explosions { get; } =
        new Dictionary<EnemyDeathExplosion, Animation<EnemyDeathProjectileSegment>>
        {
            [EnemyDeathExplosion.Small] = StartingAt[P.SmallExplosion],
            [EnemyDeathExplosion.SamusContact] = StartingAt[P.KilledBySamusContact],
            [EnemyDeathExplosion.Normal] = StartingAt[P.NormalExplosion],
            [EnemyDeathExplosion.MiniKraid] = new([new(P.MiniKraidExplosion, 0, 5, 16), new(P.MiniKraidExplosion, 5, 1, 1)], AnimationEnd.Holds),
            [EnemyDeathExplosion.Big] = new([new(P.BigExplosion, 0, 4, 5), new(P.BigExplosion, 4, 1, 1)], AnimationEnd.Holds),
        };

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue> Cue(EnemyDeathProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue>.Event(cue);
}
