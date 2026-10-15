using SuperMetroid.Core.Programs;
using C = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileCue;
using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileProgram;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spore Spawn's typed projectile programs (#641), in place of their bank-$86 lists.</summary>
internal static class SporeSpawnProjectileAnimations
{
    /// <summary>The emitter's release poses' dwells ($86:DC06..DC18).</summary>
    private static readonly int[] ReleaseDurations = [1, 6, 16, 6, 1];
    /// <summary>Each falling-spore and stalk pose's dwell ($86:DC1E..DC2E).</summary>
    private const int SporeDuration = 5;
    /// <summary>The shot spore's poses' dwells ($86:DC34..DC50).</summary>
    private static readonly int[] ShotDurations = [1, 3, 6, 5, 5, 5, 6];
    /// <summary>The falling spore's poses.</summary>
    internal const int SporePoseCount = 3;

    /// <summary>The poses of each program's run.</summary>
    internal static int PoseCount(SporeSpawnProjectileProgram program) => program switch
    {
        P.SpawnerClosed or P.Stalk => 1,
        P.SpawnerRelease => ReleaseDurations.Length,
        P.Spore => SporePoseCount,
        P.SporeShot => ShotDurations.Length,
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "Undefined Spore Spawn projectile program."),
    };

    /// <summary>Every projectile program.</summary>
    internal static AnimationProgramSet<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue> Programs { get; } = new(
        new Dictionary<SporeSpawnProjectileProgram, AnimationProgramStep<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue>[]>
    {
        [P.SpawnerClosed] = [Show(P.SpawnerClosed, 0, 1), Hold],
        [P.SpawnerRelease] =
        [
            Show(P.SpawnerRelease, 0, ReleaseDurations[0]),
            Show(P.SpawnerRelease, 1, ReleaseDurations[1]),
            Cue(C.SpawnSpore),
            Show(P.SpawnerRelease, 2, ReleaseDurations[2]),
            Show(P.SpawnerRelease, 3, ReleaseDurations[3]),
            Show(P.SpawnerRelease, 4, ReleaseDurations[4]),
            Hold,
        ],
        [P.Spore] =
        [
            .. Enumerable.Range(0, SporePoseCount).Select(phase => Show(P.Spore, phase, SporeDuration)),
            AnimationProgramStep<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue>.GoTo(P.Spore),
        ],
        [P.Stalk] = [Show(P.Stalk, 0, SporeDuration), Hold],
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
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="phase"/> of <paramref name="program"/>'s run for <paramref name="duration"/> passes.</summary>
    private static AnimationProgramStep<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue> Show(
        SporeSpawnProjectileProgram program, int phase, int duration) =>
        AnimationProgramStep<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue>.Show(new(program, phase), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue> Cue(SporeSpawnProjectileCue cue) =>
        AnimationProgramStep<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue>.Publish(cue);

    /// <summary>Holds the last frame, as the native sleep and deletion do.</summary>
    private static AnimationProgramStep<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue> Hold =>
        AnimationProgramStep<SporeSpawnProjectileProgram, SporeSpawnProjectileFrame, SporeSpawnProjectileCue>.Hold;
}
