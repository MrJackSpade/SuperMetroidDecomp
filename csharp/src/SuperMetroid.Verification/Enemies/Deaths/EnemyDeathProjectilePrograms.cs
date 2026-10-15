using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Verification.Enemies.Deaths;

/// <summary>Reads which typed death or drop program a projectile runs (#1291).</summary>
internal static class EnemyDeathProjectilePrograms
{
    /// <summary>The death or drop program <paramref name="projectile"/> runs.</summary>
    internal static EnemyDeathProjectileProgram ProgramOf(RoomEnemyProjectileSlot projectile) =>
        projectile.Animation is EnemyDeathProjectileAnimation animation ? animation.Cursor.Program
            : throw new InvalidOperationException($"Projectile {projectile.SlotIndex} runs no death or drop program.");
}
