using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.SpriteObjects;
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
            Show(EnemyProjectileFrameId.EnemyDeathRespawnTail, 64),
            Cue(new(K.HandleRespawningEnemy)),
            Cue(new(K.Delete)),
        ],
        [P.BigExplosion] =
        [
        ],
        [P.BigExplosionLoop] =
        [
            Cue(new(K.SpawnSpriteWithin16, SpriteObject: RoomSpriteObjectKind.SmallExplosion)),
            Cue(new(K.SpawnSpriteWithin16, SpriteObject: RoomSpriteObjectKind.Smoke)),
            Show(EnemyProjectileFrameId.EnemyDeathBigExplosion, 8),
            Cue(new(K.PlaySmallExplosionSound)),
            Cue(new(K.BecomePickup)),
        ],
        [P.MiniKraidExplosion] =
        [
        ],
        [P.MiniKraidExplosionLoop] =
        [
            Cue(new(K.SpawnSpriteWithin32, SpriteObject: RoomSpriteObjectKind.SmallExplosion)),
            Cue(new(K.SpawnSpriteWithin32, SpriteObject: RoomSpriteObjectKind.Smoke)),
            Cue(new(K.SpawnSpriteWithin32, SpriteObject: RoomSpriteObjectKind.DustCloud)),
            Show(EnemyProjectileFrameId.EnemyDeathMiniKraidExplosion, 8),
            Cue(new(K.PlaySmallExplosionSound)),
            Cue(new(K.BecomePickup)),
        ],
        [P.NormalExplosion] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathNormalExplosionFirst, 5),
            Show(EnemyProjectileFrameId.EnemyDeathNormalExplosionSecond, 5),
            Show(EnemyProjectileFrameId.EnemyDeathNormalExplosionThird, 5),
            Cue(new(K.PlayEnemyKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathNormalExplosionFourth, 5),
            Show(EnemyProjectileFrameId.EnemyDeathNormalExplosionFifth, 5),
            Show(EnemyProjectileFrameId.EnemyDeathNormalExplosionSixth, 5),
            Cue(new(K.BecomePickup)),
        ],
        [P.SmallExplosion] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathSmallExplosionFirst, 4),
            Show(EnemyProjectileFrameId.EnemyDeathSmallExplosionSecond, 6),
            Show(EnemyProjectileFrameId.EnemyDeathSmallExplosionThird, 5),
            Cue(new(K.PlayEnemyKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathSmallExplosionFourth, 5),
            Show(EnemyProjectileFrameId.EnemyDeathSmallExplosionFifth, 5),
            Show(EnemyProjectileFrameId.EnemyDeathSmallExplosionSixth, 6),
            Cue(new(K.BecomePickup)),
        ],
        [P.SmallEnergy] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupSmallEnergyFirst, 8),
            Show(EnemyProjectileFrameId.EnemyPickupSmallEnergySecond, 8),
            Show(EnemyProjectileFrameId.EnemyPickupSmallEnergyThird, 8),
            Show(EnemyProjectileFrameId.EnemyPickupSmallEnergyFourth, 8),
        ],
        [P.BigEnergy] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupBigEnergyFirst, 8),
            Show(EnemyProjectileFrameId.EnemyPickupBigEnergySecond, 8),
            Show(EnemyProjectileFrameId.EnemyPickupBigEnergyThird, 8),
            Show(EnemyProjectileFrameId.EnemyPickupBigEnergyFourth, 8),
        ],
        [P.Missiles] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupMissilesFirst, 5),
            Show(EnemyProjectileFrameId.EnemyPickupMissilesSecond, 5),
        ],
        [P.SuperMissiles] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupSuperMissilesFirst, 5),
            Show(EnemyProjectileFrameId.EnemyPickupSuperMissilesSecond, 5),
        ],
        [P.PowerBombs] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupPowerBombsFirst, 5),
            Show(EnemyProjectileFrameId.EnemyPickupPowerBombsSecond, 5),
            Show(EnemyProjectileFrameId.EnemyPickupPowerBombsThird, 5),
            Show(EnemyProjectileFrameId.EnemyPickupPowerBombsFourth, 5),
        ],
        [P.KilledBySamusContact] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactFirst, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactSecond, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactThird, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactFourth, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactFifth, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactSixth, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactSeventh, 2),
            Cue(new(K.PlayContactKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactEighth, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactNinth, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactTenth, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactEleventh, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactTwelfth, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactThirteenth, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactFourteenth, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactFifteenth, 2),
            Show(EnemyProjectileFrameId.EnemyDeathKilledBySamusContactSixteenth, 2),
            Cue(new(K.BecomePickup)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EnemyDeathProjectileSegment, Animation<EnemyDeathProjectileSegment>> StartingAt { get; } = new Dictionary<EnemyDeathProjectileSegment, Animation<EnemyDeathProjectileSegment>>
    {
        [P.RespawnTail] = new([new(P.RespawnTail, 0, 3, 1)], AnimationEnd.Holds),
        [P.BigExplosion] = new([new(P.BigExplosionLoop, 0, 4, 5), new(P.BigExplosionLoop, 4, 1, 1)], AnimationEnd.Holds),
        [P.MiniKraidExplosion] = new([new(P.MiniKraidExplosionLoop, 0, 5, 16), new(P.MiniKraidExplosionLoop, 5, 1, 1)], AnimationEnd.Holds),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue> Cue(EnemyDeathProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue>.Event(cue);
}
