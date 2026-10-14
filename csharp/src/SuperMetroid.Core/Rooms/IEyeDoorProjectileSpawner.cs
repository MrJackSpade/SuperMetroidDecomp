namespace SuperMetroid.Core.Rooms;

/// <summary>Eye-door projectiles (#633).</summary>
public interface IEyeDoorProjectileSpawner
{
    /// <summary>Spawns one eye-door projectile.</summary>
    void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request);
}
