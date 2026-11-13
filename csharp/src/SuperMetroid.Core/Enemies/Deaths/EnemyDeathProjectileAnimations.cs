using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.SpriteObjects;
using K = SuperMetroid.Core.Enemies.Deaths.EnemyDeathProjectileCueKind;
using P = SuperMetroid.Core.Enemies.Deaths.EnemyDeathProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The generic enemy-death explosions, the drops they become and the respawn tail (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each command becomes a <see cref="EnemyDeathProjectileCue"/> with its typed operands.
/// </summary>
internal static class EnemyDeathProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EnemyDeathProjectileProgram, EnemyProjectileFrameId, EnemyDeathProjectileCue> Segments { get; } = new(
        new Dictionary<EnemyDeathProjectileProgram, AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue>[]>
    {
        [P.RespawnTail] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathFrame00, 64),
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
            Show(EnemyProjectileFrameId.EnemyDeathFrame01, 8),
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
            Show(EnemyProjectileFrameId.EnemyDeathFrame02, 8),
            Cue(new(K.PlaySmallExplosionSound)),
            Cue(new(K.BecomePickup)),
        ],
        [P.NormalExplosion] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathFrame03, 5),
            Show(EnemyProjectileFrameId.EnemyDeathFrame04, 5),
            Show(EnemyProjectileFrameId.EnemyDeathFrame05, 5),
            Cue(new(K.PlayEnemyKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFrame06, 5),
            Show(EnemyProjectileFrameId.EnemyDeathFrame07, 5),
            Show(EnemyProjectileFrameId.EnemyDeathFrame08, 5),
            Cue(new(K.BecomePickup)),
        ],
        [P.SmallExplosion] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathFrame09, 4),
            Show(EnemyProjectileFrameId.EnemyDeathFrame10, 6),
            Show(EnemyProjectileFrameId.EnemyDeathFrame11, 5),
            Cue(new(K.PlayEnemyKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFrame12, 5),
            Show(EnemyProjectileFrameId.EnemyDeathFrame13, 5),
            Show(EnemyProjectileFrameId.EnemyDeathFrame14, 6),
            Cue(new(K.BecomePickup)),
        ],
        [P.SmallEnergy] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupFrame00, 8),
            Show(EnemyProjectileFrameId.EnemyPickupFrame01, 8),
            Show(EnemyProjectileFrameId.EnemyPickupFrame02, 8),
            Show(EnemyProjectileFrameId.EnemyPickupFrame03, 8),
        ],
        [P.BigEnergy] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupFrame04, 8),
            Show(EnemyProjectileFrameId.EnemyPickupFrame05, 8),
            Show(EnemyProjectileFrameId.EnemyPickupFrame06, 8),
            Show(EnemyProjectileFrameId.EnemyPickupFrame07, 8),
        ],
        [P.Missiles] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupFrame08, 5),
            Show(EnemyProjectileFrameId.EnemyPickupFrame09, 5),
        ],
        [P.SuperMissiles] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupFrame10, 5),
            Show(EnemyProjectileFrameId.EnemyPickupFrame11, 5),
        ],
        [P.PowerBombs] =
        [
            Show(EnemyProjectileFrameId.EnemyPickupFrame12, 5),
            Show(EnemyProjectileFrameId.EnemyPickupFrame13, 5),
            Show(EnemyProjectileFrameId.EnemyPickupFrame14, 5),
            Show(EnemyProjectileFrameId.EnemyPickupFrame15, 5),
        ],
        [P.KilledBySamusContact] =
        [
            Show(EnemyProjectileFrameId.EnemyDeathFrame15, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame16, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame17, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame18, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame19, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame20, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame21, 2),
            Cue(new(K.PlayContactKilledSound)),
            Show(EnemyProjectileFrameId.EnemyDeathFrame22, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame23, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame24, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame25, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame26, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame27, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame28, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame29, 2),
            Show(EnemyProjectileFrameId.EnemyDeathFrame30, 2),
            Cue(new(K.BecomePickup)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EnemyDeathProjectileProgram, Animation<EnemyDeathProjectileProgram>> StartingAt { get; } = new Dictionary<EnemyDeathProjectileProgram, Animation<EnemyDeathProjectileProgram>>
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
    internal static Animation<EnemyDeathProjectileProgram> StartingWith(EnemyDeathProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue> Cue(EnemyDeathProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyDeathProjectileCue>.Event(cue);
}
