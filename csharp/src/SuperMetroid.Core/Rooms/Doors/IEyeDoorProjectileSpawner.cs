namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Eye-door projectiles (#633).</summary>
public interface IEyeDoorProjectileSpawner
{
    /// <summary>Spawns one eye-door sweat or smoke actor.</summary>
    void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request);

    /// <summary>Spawns the eye's attack.</summary>
    void SpawnEyeDoorAttack(EyeDoorAttackRequest request);
}
