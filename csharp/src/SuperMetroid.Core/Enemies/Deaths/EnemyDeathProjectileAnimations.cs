using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.SpriteObjects;
using K = SuperMetroid.Core.Enemies.Deaths.EnemyDeathProjectileCueKind;
using P = SuperMetroid.Core.Enemies.Deaths.EnemyDeathProjectileProgram;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The generic enemy-death explosions, the drops they become and the respawn tail (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="EnemyDeathProjectileFrame"/> pose and each command becomes a <see cref="EnemyDeathProjectileCue"/> with its typed operands.
/// </summary>
internal static class EnemyDeathProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue> Programs { get; } = new(
        new Dictionary<EnemyDeathProjectileProgram, AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue>[]>
    {
        // $86:ECA3
        [P.RespawnTail] =
        [
            Show(0, 64),
            Cue(new(K.HandleRespawningEnemy)),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:ECAB
        [P.BigExplosion] =
        [
            SetCounter(5),
            FallInto(P.BigExplosionLoop),
        ],
        // $86:ECAF
        [P.BigExplosionLoop] =
        [
            Cue(new(K.SpawnSpriteWithin16, SpriteObject: RoomSpriteObjectKind.SmallExplosion)),
            Cue(new(K.SpawnSpriteWithin16, SpriteObject: RoomSpriteObjectKind.Smoke)),
            Show(1, 8),
            Cue(new(K.PlaySmallExplosionSound)),
            RepeatWhileCounting(P.BigExplosionLoop),
            Cue(new(K.BecomePickup)),
            Hold,
        ],
        // $86:ECC5
        [P.MiniKraidExplosion] =
        [
            SetCounter(16),
            FallInto(P.MiniKraidExplosionLoop),
        ],
        // $86:ECC9
        [P.MiniKraidExplosionLoop] =
        [
            Cue(new(K.SpawnSpriteWithin32, SpriteObject: RoomSpriteObjectKind.SmallExplosion)),
            Cue(new(K.SpawnSpriteWithin32, SpriteObject: RoomSpriteObjectKind.Smoke)),
            Cue(new(K.SpawnSpriteWithin32, SpriteObject: RoomSpriteObjectKind.DustCloud)),
            Show(2, 8),
            Cue(new(K.PlaySmallExplosionSound)),
            RepeatWhileCounting(P.MiniKraidExplosionLoop),
            Cue(new(K.BecomePickup)),
            Hold,
        ],
        // $86:ED4B
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
            Hold,
        ],
        // $86:ED69
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
            Hold,
        ],
        // $86:ED8D
        [P.SmallEnergy] =
        [
            Show(31, 8),
            Show(32, 8),
            Show(33, 8),
            Show(34, 8),
            GoTo(P.SmallEnergy),
        ],
        // $86:EDA3
        [P.BigEnergy] =
        [
            Show(35, 8),
            Show(36, 8),
            Show(37, 8),
            Show(38, 8),
            GoTo(P.BigEnergy),
        ],
        // $86:EDB9
        [P.Missiles] =
        [
            Show(39, 5),
            Show(40, 5),
            GoTo(P.Missiles),
        ],
        // $86:EDDD
        [P.SuperMissiles] =
        [
            Show(41, 5),
            Show(42, 5),
            GoTo(P.SuperMissiles),
        ],
        // $86:EDEB
        [P.PowerBombs] =
        [
            Show(43, 5),
            Show(44, 5),
            Show(45, 5),
            Show(46, 5),
            GoTo(P.PowerBombs),
        ],
        // $86:EDFF
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
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue> Show(int pose, int duration) => AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue> Cue(EnemyDeathProjectileCue cue) => AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue>.Publish(cue);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue> SetCounter(int count) => AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue> RepeatWhileCounting(EnemyDeathProjectileProgram program) => AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue>.RepeatWhileCounting(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue> GoTo(EnemyDeathProjectileProgram program) => AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue> FallInto(EnemyDeathProjectileProgram program) => AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue> Hold => AnimationProgramStep<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue>.Hold;
}
