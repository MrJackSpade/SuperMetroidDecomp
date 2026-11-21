namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>A free enemy-projectile place the spawn search found (#2553), which a spawn takes (<see cref="EnemyProjectileEngine.Spawn"/>).</summary>
/// <param name="Place">The place, zero for the last place the search reaches.</param>
internal readonly record struct EnemyProjectileVacancy(int Place);
