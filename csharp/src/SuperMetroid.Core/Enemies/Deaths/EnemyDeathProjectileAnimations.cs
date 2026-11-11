using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.SpriteObjects;
using K = SuperMetroid.Core.Enemies.Deaths.EnemyDeathProjectileCueKind;
using P = SuperMetroid.Core.Enemies.Deaths.EnemyDeathProjectileProgram;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The generic enemy-death explosions, the drops they become and the respawn tail (#641). Each
/// frame shows a <see cref="EnemyDeathProjectileFrame"/> pose and each command becomes a <see cref="EnemyDeathProjectileCue"/> with its typed operands.
/// </summary>
internal static class EnemyDeathProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue> Segments { get; } = new(
        new Dictionary<EnemyDeathProjectileProgram, AnimationBeat<EnemyDeathProjectileFrame, EnemyDeathProjectileCue>[]>
    {
        [P.RespawnTail] =
        [
            Show(0, 64),
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
            Show(1, 8),
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
            Show(2, 8),
            Cue(new(K.PlaySmallExplosionSound)),
            Cue(new(K.BecomePickup)),
        ],
        [P.NormalExplosion] =
        [
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
            Cue(new(K.PlayEnemyKilledSound)),
            Show(6, 5),
            Show(7, 5),
            Show(8, 5),
            Cue(new(K.BecomePickup)),
        ],
        [P.SmallExplosion] =
        [
            Show(9, 4),
            Show(10, 6),
            Show(11, 5),
            Cue(new(K.PlayEnemyKilledSound)),
            Show(12, 5),
            Show(13, 5),
            Show(14, 6),
            Cue(new(K.BecomePickup)),
        ],
        [P.SmallEnergy] =
        [
            Show(31, 8),
            Show(32, 8),
            Show(33, 8),
            Show(34, 8),
        ],
        [P.BigEnergy] =
        [
            Show(35, 8),
            Show(36, 8),
            Show(37, 8),
            Show(38, 8),
        ],
        [P.Missiles] =
        [
            Show(39, 5),
            Show(40, 5),
        ],
        [P.SuperMissiles] =
        [
            Show(41, 5),
            Show(42, 5),
        ],
        [P.PowerBombs] =
        [
            Show(43, 5),
            Show(44, 5),
            Show(45, 5),
            Show(46, 5),
        ],
        [P.KilledBySamusContact] =
        [
            Show(15, 2),
            Show(16, 2),
            Show(17, 2),
            Show(18, 2),
            Show(19, 2),
            Show(20, 2),
            Show(21, 2),
            Cue(new(K.PlayContactKilledSound)),
            Show(22, 2),
            Show(23, 2),
            Show(24, 2),
            Show(25, 2),
            Show(26, 2),
            Show(27, 2),
            Show(28, 2),
            Show(29, 2),
            Show(30, 2),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyDeathProjectileFrame, EnemyDeathProjectileCue> Show(int pose, int duration) => new AnimationBeat<EnemyDeathProjectileFrame, EnemyDeathProjectileCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyDeathProjectileFrame, EnemyDeathProjectileCue> Cue(EnemyDeathProjectileCue cue) => new AnimationBeat<EnemyDeathProjectileFrame, EnemyDeathProjectileCue>.Event(cue);
}
