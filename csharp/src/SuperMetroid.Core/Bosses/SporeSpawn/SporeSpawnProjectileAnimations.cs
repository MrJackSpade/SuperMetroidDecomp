using SuperMetroid.Core.Animation;
using C = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileCue;
using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileSegment;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spore Spawn's typed projectile animations (#641).</summary>
internal static class SporeSpawnProjectileAnimations
{
    /// <summary>The emitter's release poses' dwells.</summary>
    internal static readonly int[] ReleaseDurations = [1, 6, 16, 6, 1];
    /// <summary>Each falling-spore and stalk pose's dwell.</summary>
    private const int SporeDuration = 5;
    /// <summary>The shot spore's poses' dwells.</summary>
    internal static readonly int[] ShotDurations = [1, 3, 6, 5, 5, 5, 6];
    /// <summary>The falling spore's poses.</summary>
    internal const int SporePoseCount = 3;

    /// <summary>Every projectile animation.</summary>
    internal static AnimationSegments<SporeSpawnProjectileSegment, SporeSpawnProjectileFrame, SporeSpawnProjectileCue> Segments { get; } = new(
        new Dictionary<SporeSpawnProjectileSegment, AnimationBeat<SporeSpawnProjectileFrame, SporeSpawnProjectileCue>[]>
    {
        [P.SpawnerClosed] =
        [
            Show(P.SpawnerClosed, 0, 1),
        ],
        [P.SpawnerRelease] =
        [
            Show(P.SpawnerRelease, 0, ReleaseDurations[0]),
            Show(P.SpawnerRelease, 1, ReleaseDurations[1]),
            Cue(C.SpawnSpore),
            Show(P.SpawnerRelease, 2, ReleaseDurations[2]),
            Show(P.SpawnerRelease, 3, ReleaseDurations[3]),
            Show(P.SpawnerRelease, 4, ReleaseDurations[4]),
        ],
        [P.Spore] =
        [
            .. Enumerable.Range(0, SporePoseCount).Select(phase => Show(P.Spore, phase, SporeDuration)),
        ],
        [P.Stalk] =
        [
            Show(P.Stalk, 0, SporeDuration),
        ],
        [P.SporeShot] =
        [
            Show(P.SporeShot, 0, ShotDurations[0]),
            Cue(C.SetImpactProperties),
            Show(P.SporeShot, 1, ShotDurations[1]),
            Show(P.SporeShot, 2, ShotDurations[2]),
            Show(P.SporeShot, 3, ShotDurations[3]),
            Cue(C.QueueKilledSound),
            Show(P.SporeShot, 4, ShotDurations[4]),
            Show(P.SporeShot, 5, ShotDurations[5]),
            Show(P.SporeShot, 6, ShotDurations[6]),
            Cue(C.RequestDrop),
            Cue(C.Delete),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>> StartingAt { get; } = new Dictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>>
    {
        [P.SpawnerClosed] = new([new(P.SpawnerClosed, 0, 1, 1)], AnimationEnd.Holds),
        [P.SpawnerRelease] = new([new(P.SpawnerRelease, 0, 6, 1)], AnimationEnd.Holds),
        [P.Spore] = new([new(P.Spore, 0, Segments.Beats(P.Spore).Count, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Stalk] = new([new(P.Stalk, 0, 1, 1)], AnimationEnd.Holds),
        [P.SporeShot] = new([new(P.SporeShot, 0, 11, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SporeSpawnProjectileSegment> StartingWith(SporeSpawnProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="phase"/> of <paramref name="segment"/>'s run for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<SporeSpawnProjectileFrame, SporeSpawnProjectileCue> Show(
        SporeSpawnProjectileSegment segment, int phase, int duration) =>
        new AnimationBeat<SporeSpawnProjectileFrame, SporeSpawnProjectileCue>.Frame(new(segment, phase), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<SporeSpawnProjectileFrame, SporeSpawnProjectileCue> Cue(SporeSpawnProjectileCue cue) =>
        new AnimationBeat<SporeSpawnProjectileFrame, SporeSpawnProjectileCue>.Event(cue);

}
