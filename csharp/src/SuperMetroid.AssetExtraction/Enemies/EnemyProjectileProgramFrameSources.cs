using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Where each installed enemy-projectile program frame is stored (<see cref="EnemyProjectileProgramFrameDefinitions"/>).</summary>
internal static class EnemyProjectileProgramFrameSources
{
    /// <summary>The bank-$86 operand installed program frame <paramref name="frame"/> is stored at.</summary>
    internal static ushort Operand(EnemyProjectileFrameId frame) =>
        EnemyProjectileProgramFrameDefinitions.Operands.ContainsKey(frame)
            ? EnemyProjectileProgramFrameDefinitions.Operands[frame]
            : throw new ArgumentOutOfRangeException(nameof(frame), frame, "No installed program frame shows this projectile frame.");
}
