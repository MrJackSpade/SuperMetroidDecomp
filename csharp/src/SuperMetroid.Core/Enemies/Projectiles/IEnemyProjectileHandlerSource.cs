namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>An owner of enemy-projectile behavior that registers its handlers with the projectile engine.</summary>
internal interface IEnemyProjectileHandlerSource
{
    /// <summary>Registers every projectile handler this owner implements.</summary>
    void RegisterEnemyProjectileHandlers(EnemyProjectileHandlerTable table);
}
