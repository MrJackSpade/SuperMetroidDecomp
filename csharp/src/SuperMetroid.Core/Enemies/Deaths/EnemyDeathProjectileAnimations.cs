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
            Show(EnemyProjectileFrameId.EnemyDeathBlank, 8),
            Cue(new EnemyDeathProjectileCue.Signal(K.PlaySmallExplosionSound)),
        ],
        [P.BigExplosionEnd] =
        [
            Cue(new EnemyDeathProjectileCue.Signal(K.BecomePickup)),
        ],
        [P.FakeKraidExplosion] =
        [
            Cue(new EnemyDeathProjectileCue.SpawnEffectActor(RoomEffectActorKind.SmallExplosion, EnemyDeathExplosionDefinitions.WideSpriteSpread)),
            Cue(new EnemyDeathProjectileCue.SpawnEffectActor(RoomEffectActorKind.Smoke, EnemyDeathExplosionDefinitions.WideSpriteSpread)),
            Cue(new EnemyDeathProjectileCue.SpawnEffectActor(RoomEffectActorKind.BigDustCloud, EnemyDeathExplosionDefinitions.WideSpriteSpread)),
            Show(EnemyProjectileFrameId.EnemyDeathBlank, 8),
            Cue(new EnemyDeathProjectileCue.Signal(K.PlaySmallExplosionSound)),
        ],
        [P.FakeKraidExplosionEnd] =
        [
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
            Show(EnemyProjectileFrameId.EnemyDeathFireballSmall, 6),
            Show(EnemyProjectileFrameId.EnemyDeathFireballLarge, 5),
            Cue(new EnemyDeathProjectileCue.Signal(K.PlayEnemyKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFlamingRing, 5),
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
            Cue(new EnemyDeathProjectileCue.Signal(K.PlayContactKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersSpreading, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersCooling, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourEmbersDimming, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshDrifting, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshSpreading, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshSinking, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshFalling, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshSettling, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFourAshDispersing, 2),
            Cue(new EnemyDeathProjectileCue.Signal(K.BecomePickup)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EnemyDeathProjectileSegment, Animation<EnemyDeathProjectileSegment>> StartingAt { get; } = new Dictionary<EnemyDeathProjectileSegment, Animation<EnemyDeathProjectileSegment>>
    {
        [P.RespawnTail] = new([Whole(P.RespawnTail)], AnimationEnd.Holds),
        [P.NormalExplosion] = new([Whole(P.NormalExplosion)], AnimationEnd.Holds),
        [P.SmallExplosion] = new([Whole(P.SmallExplosion)], AnimationEnd.Holds),
        [P.SmallEnergy] = new([Whole(P.SmallEnergy)], AnimationEnd.LoopsFrom(0)),
        [P.BigEnergy] = new([Whole(P.BigEnergy)], AnimationEnd.LoopsFrom(0)),
        [P.Missiles] = new([Whole(P.Missiles)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissiles] = new([Whole(P.SuperMissiles)], AnimationEnd.LoopsFrom(0)),
        [P.PowerBombs] = new([Whole(P.PowerBombs)], AnimationEnd.LoopsFrom(0)),
        [P.KilledBySamusContact] = new([Whole(P.KilledBySamusContact)], AnimationEnd.Holds),
    };

    /// <summary>How many bursts the big explosion makes before it becomes the drop.</summary>
    private const int BigExplosionBursts = 5;

    /// <summary>How many bursts Fake Kraid's explosion makes before it becomes the drop.</summary>
    private const int FakeKraidExplosionBursts = 16;

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EnemyDeathProjectileSegment> StartingWith(EnemyDeathProjectileSegment segment) => StartingAt[segment];

    /// <summary>
    /// The animation each death explosion starts (#2061): the big explosion's five bursts and Fake Kraid's sixteen, each
    /// scattering its sprites, before it becomes the drop; the other explosions' animations start at their segments.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyDeathExplosion, Animation<EnemyDeathProjectileSegment>> Explosions { get; } =
        new Dictionary<EnemyDeathExplosion, Animation<EnemyDeathProjectileSegment>>
        {
            [EnemyDeathExplosion.Small] = StartingAt[P.SmallExplosion],
            [EnemyDeathExplosion.SamusContact] = StartingAt[P.KilledBySamusContact],
            [EnemyDeathExplosion.Normal] = StartingAt[P.NormalExplosion],
            [EnemyDeathExplosion.FakeKraid] = new([Repeated(P.FakeKraidExplosion, FakeKraidExplosionBursts), Whole(P.FakeKraidExplosionEnd)], AnimationEnd.Holds),
            [EnemyDeathExplosion.Big] = new([Repeated(P.BigExplosion, BigExplosionBursts), Whole(P.BigExplosionEnd)], AnimationEnd.Holds),
        };

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<EnemyDeathProjectileSegment> Whole(EnemyDeathProjectileSegment segment) => Repeated(segment, 1);

    /// <summary><paramref name="plays"/> plays of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<EnemyDeathProjectileSegment> Repeated(EnemyDeathProjectileSegment segment, int plays) =>
        new(segment, 0, Segments.Beats(segment).Count, plays);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue> Cue(EnemyDeathProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue>.Event(cue);
}
