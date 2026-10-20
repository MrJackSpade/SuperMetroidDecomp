using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileProgram;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Cartridge values and helpers of <see cref="SporeSpawnProjectileAnimations"/> that only verification reads.</summary>
internal static class SporeSpawnProjectileAnimationsConstants
{
    /// <summary>The poses of each program's run.</summary>
    internal static int PoseCount(SporeSpawnProjectileProgram program) => program switch
    {
        P.SpawnerClosed or P.Stalk => 1,
        P.SpawnerRelease => SporeSpawnProjectileAnimations.ReleaseDurations.Length,
        P.Spore => SporeSpawnProjectileAnimations.SporePoseCount,
        P.SporeShot => SporeSpawnProjectileAnimations.ShotDurations.Length,
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "Undefined Spore Spawn projectile program."),
    };
}
