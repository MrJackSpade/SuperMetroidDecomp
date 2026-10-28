using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>Chooses the death program an enemy's death animation starts.</summary>
internal static class EnemyDeathExplosions
{
    /// <summary>The death program death-animation selector <paramref name="animation"/> starts.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="animation"/> is outside zero through four.</exception>
    internal static EnemyDeathProjectileProgram Program(int animation) =>
        (uint)animation < (uint)EnemyDeathExplosionDefinitions.Programs.Length
            ? EnemyDeathExplosionDefinitions.Programs[animation]
            : throw new ArgumentOutOfRangeException(nameof(animation), animation, "Enemy death animation must be zero through four.");
}
