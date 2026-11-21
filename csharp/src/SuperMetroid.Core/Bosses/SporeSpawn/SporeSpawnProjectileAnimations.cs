using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using C = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnCeilingSegment;
using S = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnSporeSegment;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's typed projectile animations (#641, #2011), each beat the installed <see cref="EnemyProjectileFrameId"/> it
/// shows, in two families (#2553): the ceiling's, whose only cue is a spawner's release of a spore, and a spore's, whose
/// cues are its burst's, so each family's owner answers only the cues its animations publish.
/// </summary>
internal static class SporeSpawnProjectileAnimations
{
    /// <summary>
    /// The ceiling's animations: the closed spawner's pose, held (#2235); the spawner's release, which spawns a spore after its
    /// second pose; and the stalk's pose, held.
    /// </summary>
    internal static UnpausedAnimationSegments<SporeSpawnCeilingSegment, EnemyProjectileFrameId, SporeSpawnCeilingCue> CeilingSegments { get; } = new(
        new Dictionary<SporeSpawnCeilingSegment, AnimationBeat<EnemyProjectileFrameId, SporeSpawnCeilingCue>.Unpaused[]>
    {
        [C.SpawnerClosed] =
        [
            Hold<SporeSpawnCeilingCue>(EnemyProjectileFrameId.SporeSpawnSpawnerClosed),
        ],
        [C.SpawnerRelease] =
        [
            Show<SporeSpawnCeilingCue>(EnemyProjectileFrameId.SporeSpawnSpawnerClosed, 1),
            Show<SporeSpawnCeilingCue>(EnemyProjectileFrameId.SporeSpawnSpawnerOpening, 6),
            Cue(SporeSpawnCeilingCue.SpawnSpore),
            Show<SporeSpawnCeilingCue>(EnemyProjectileFrameId.SporeSpawnSpawnerOpen, 16),
            Show<SporeSpawnCeilingCue>(EnemyProjectileFrameId.SporeSpawnSpawnerOpening, 6),
            Show<SporeSpawnCeilingCue>(EnemyProjectileFrameId.SporeSpawnSpawnerClosed, 1),
        ],
        [C.Stalk] =
        [
            Hold<SporeSpawnCeilingCue>(EnemyProjectileFrameId.SporeSpawnStalk),
        ],
    });

    /// <summary>
    /// A spore's animations: the falling spore's poses, and a shot spore's burst, which takes its impact properties after its
    /// first pose, queues its killed sound partway, then requests its drop and deletes itself.
    /// </summary>
    internal static UnpausedAnimationSegments<SporeSpawnSporeSegment, EnemyProjectileFrameId, SporeSpawnSporeCue> SporeSegments { get; } = new(
        new Dictionary<SporeSpawnSporeSegment, AnimationBeat<EnemyProjectileFrameId, SporeSpawnSporeCue>.Unpaused[]>
    {
        [S.Spore] =
        [
            Show<SporeSpawnSporeCue>(EnemyProjectileFrameId.SporeSpawnSporeGlowing, 5),
            Show<SporeSpawnSporeCue>(EnemyProjectileFrameId.SporeSpawnSporeDim, 5),
            Show<SporeSpawnSporeCue>(EnemyProjectileFrameId.SporeSpawnSporeSparkling, 5),
        ],
        [S.SporeShot] =
        [
            Show<SporeSpawnSporeCue>(EnemyProjectileFrameId.SporeSpawnSporeBurstSpark, 1),
            Cue(SporeSpawnSporeCue.SetImpactProperties),
            Show<SporeSpawnSporeCue>(EnemyProjectileFrameId.SporeSpawnSporeBurstSpark, 3),
            Show<SporeSpawnSporeCue>(EnemyProjectileFrameId.SporeSpawnSporeBurstFireballSmall, 6),
            Show<SporeSpawnSporeCue>(EnemyProjectileFrameId.SporeSpawnSporeBurstFireballLarge, 5),
            Cue(SporeSpawnSporeCue.QueueKilledSound),
            Show<SporeSpawnSporeCue>(EnemyProjectileFrameId.SporeSpawnSporeBurstFlamingRing, 5),
            Show<SporeSpawnSporeCue>(EnemyProjectileFrameId.SporeSpawnSporeBurstStarburst, 5),
            Show<SporeSpawnSporeCue>(EnemyProjectileFrameId.SporeSpawnSporeBurstEmbers, 6),
            Cue(SporeSpawnSporeCue.RequestDrop),
            Cue(SporeSpawnSporeCue.Delete),
        ],
    });

    /// <summary>The ceiling animation that starts at each of its segments: each plays its segment once and holds.</summary>
    internal static IReadOnlyDictionary<SporeSpawnCeilingSegment, Animation<SporeSpawnCeilingSegment>> CeilingStartingAt { get; } = new Dictionary<SporeSpawnCeilingSegment, Animation<SporeSpawnCeilingSegment>>
    {
        [C.SpawnerClosed] = new([CeilingSegments.Whole(C.SpawnerClosed)], AnimationEnd.Holds),
        [C.SpawnerRelease] = new([CeilingSegments.Whole(C.SpawnerRelease)], AnimationEnd.Holds),
        [C.Stalk] = new([CeilingSegments.Whole(C.Stalk)], AnimationEnd.Holds),
    };

    /// <summary>The spore animation that starts at each of its segments: the falling spore loops, and the burst plays once and holds.</summary>
    internal static IReadOnlyDictionary<SporeSpawnSporeSegment, Animation<SporeSpawnSporeSegment>> SporeStartingAt { get; } = new Dictionary<SporeSpawnSporeSegment, Animation<SporeSpawnSporeSegment>>
    {
        [S.Spore] = Animation<SporeSpawnSporeSegment>.Looping([SporeSegments.Whole(S.Spore)]),
        [S.SporeShot] = new([SporeSegments.Whole(S.SporeShot)], AnimationEnd.Holds),
    };

    /// <summary>The ceiling animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SporeSpawnCeilingSegment> StartingWith(SporeSpawnCeilingSegment segment) => CeilingStartingAt[segment];

    /// <summary>The spore animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SporeSpawnSporeSegment> StartingWith(SporeSpawnSporeSegment segment) => SporeStartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for good: a still picture (#2235).</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TCue>.Unpaused Hold<TCue>(EnemyProjectileFrameId frame)
        where TCue : notnull =>
        new AnimationBeat<EnemyProjectileFrameId, TCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TCue>.Unpaused Show<TCue>(EnemyProjectileFrameId frame, int duration)
        where TCue : notnull =>
        new AnimationBeat<EnemyProjectileFrameId, TCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TCue>.Unpaused Cue<TCue>(TCue cue)
        where TCue : notnull =>
        new AnimationBeat<EnemyProjectileFrameId, TCue>.Event(cue);
}
