using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using C = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileCue;
using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileSegment;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's typed projectile animations (#641, #2011), each beat the installed <see cref="EnemyProjectileFrameId"/> it
/// shows.
/// </summary>
internal static class SporeSpawnProjectileAnimations
{
    /// <summary>
    /// Every projectile animation: the closed emitter's one pose; the emitter's release, which spawns a spore after its
    /// second pose; the falling spore's three five-update poses and the stalk's one; and a shot spore's burst, which takes its
    /// impact properties after its first pose, queues its killed sound after its fourth, then requests its drop and deletes
    /// itself.
    /// </summary>
    internal static AnimationSegments<SporeSpawnProjectileSegment, EnemyProjectileFrameId, SporeSpawnProjectileCue> Segments { get; } = new(
        new Dictionary<SporeSpawnProjectileSegment, AnimationBeat<EnemyProjectileFrameId, SporeSpawnProjectileCue>[]>
    {
        [P.SpawnerClosed] =
        [
            Show(EnemyProjectileFrameId.SporeSpawnSpawnerClosed, 1),
        ],
        [P.SpawnerRelease] =
        [
            Show(EnemyProjectileFrameId.SporeSpawnSpawnerClosedTwin, 1),
            Show(EnemyProjectileFrameId.SporeSpawnSpawnerOpening, 6),
            Cue(C.SpawnSpore),
            Show(EnemyProjectileFrameId.SporeSpawnSpawnerOpen, 16),
            Show(EnemyProjectileFrameId.SporeSpawnSpawnerOpeningTwin, 6),
            Show(EnemyProjectileFrameId.SporeSpawnSpawnerClosedTwinB, 1),
        ],
        [P.Spore] =
        [
            Show(EnemyProjectileFrameId.SporeSpawnSporeGlowing, 5),
            Show(EnemyProjectileFrameId.SporeSpawnSporeDim, 5),
            Show(EnemyProjectileFrameId.SporeSpawnSporeSparkling, 5),
        ],
        [P.Stalk] =
        [
            Show(EnemyProjectileFrameId.SporeSpawnStalk, 5),
        ],
        [P.SporeShot] =
        [
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstSpark, 1),
            Cue(C.SetImpactProperties),
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstSparkTwin, 3),
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstFireballSmall, 6),
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstFireballLarge, 5),
            Cue(C.QueueKilledSound),
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstFlamingRing, 5),
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstStarburst, 5),
            Show(EnemyProjectileFrameId.SporeSpawnSporeBurstEmbers, 6),
            Cue(C.RequestDrop),
            Cue(C.Delete),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at: each plays its segment once and holds,
    /// but the falling spore, which loops.
    /// </summary>
    internal static IReadOnlyDictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>> StartingAt { get; } = new Dictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>>
    {
        [P.SpawnerClosed] = new([Whole(P.SpawnerClosed)], AnimationEnd.Holds),
        [P.SpawnerRelease] = new([Whole(P.SpawnerRelease)], AnimationEnd.Holds),
        [P.Spore] = new([Whole(P.Spore)], AnimationEnd.LoopsFrom(0)),
        [P.Stalk] = new([Whole(P.Stalk)], AnimationEnd.Holds),
        [P.SporeShot] = new([Whole(P.SporeShot)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SporeSpawnProjectileSegment> StartingWith(SporeSpawnProjectileSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<SporeSpawnProjectileSegment> Whole(SporeSpawnProjectileSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SporeSpawnProjectileCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, SporeSpawnProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SporeSpawnProjectileCue> Cue(SporeSpawnProjectileCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, SporeSpawnProjectileCue>.Event(cue);
}
